using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using PixelBattles.Hub.Server.Handlers.Chunk;
using PixelBattles.Hub.Server.Handling.Main;
using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace PixelBattles.Hub.Server.Hubs
{
    [Authorize(JwtBearerDefaults.AuthenticationScheme)]
    public class BattleHub : BaseHub
    {
        public BattleHub(IMainHandler mainHandler) : base(mainHandler)
        {

        }

        public ChannelReader<ChunkStreamMessage> GetUpdateStream()
        {
            return OutgoingChannel.Reader;
        }

        public async Task RequestChunkState(ChunkKey key)
        {
            if (Subscriptions.TryGetValue(key, out var chunkHandlerSubscription))
            {
                await SendChunkStateAsync(key, chunkHandlerSubscription);
            }
            else
            {
                throw new InvalidOperationException("Can't access chunk that not subscribed.");
            }
        }

        public async Task<int> ProcessChunkAction(ChunkKey key, ChunkAction action)
        {
            if (Subscriptions.TryGetValue(key, out var chunkHandlerSubscription))
            {
                return await chunkHandlerSubscription.ChunkHandler.ProcessAsync(action);
            }
            else
            {
                throw new InvalidOperationException("Can't access chunk that not subscribed.");
            }
        }

        public async Task EnqueueChunkAction(ChunkKey key, ChunkAction action)
        {
            if (Subscriptions.TryGetValue(key, out var chunkHandlerSubscription))
            {
                await chunkHandlerSubscription.ChunkHandler.EnqueueAsync(action);
                return;
            }
            else
            {
                throw new InvalidOperationException("Can't access chunk that not subscribed.");
            }
        }

        public void UnsubscribeFromChunk(ChunkKey key)
        {
            if (Subscriptions.Remove(key, out var subscription))
            {
                subscription.Dispose();
            }
        }
        
        public async Task SubscribeToChunk(ChunkKey key)
        {
            if (Subscriptions.ContainsKey(key))
            {
                return;
            }

            var channel = OutgoingChannel;//copy to local for closure;
            var chunkHandlerSubscription = await BattleHandler.GetChunkHandlerAndSubscribeAsync(key, (chunkKey, update) => OnChunkUpdateAsync(key, update, channel));
            if (Subscriptions.TryAdd(key, chunkHandlerSubscription))
            {
                await SendChunkStateAsync(key, chunkHandlerSubscription);
            }
            else
            {
                chunkHandlerSubscription.Dispose();
                throw new InvalidOperationException("Duplicate subscription is detected.");
            }
        }

        private async Task SendChunkStateAsync(ChunkKey key, IChunkHandlerSubscription chunkHandlerSubscription)
        {
            var state = await chunkHandlerSubscription.ChunkHandler.GetStateAsync();
            await OutgoingChannel.Writer.WriteAsync(new ChunkStreamMessage(key, state));
        }

        private async Task OnChunkUpdateAsync(ChunkKey key, ChunkUpdate update, Channel<ChunkStreamMessage> channel)
        {
            await OutgoingChannel.Writer.WriteAsync(new ChunkStreamMessage(key, update));
        }
    }
}