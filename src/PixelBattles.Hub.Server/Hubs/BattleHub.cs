using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using PixelBattles.Hub.Server.Handlers;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace PixelBattles.Hub.Server.Hubs
{
    [Authorize(JwtBearerDefaults.AuthenticationScheme)]
    public class BattleHub : BaseHub
    {
        public BattleHub(MainHandler mainHandler) : base(mainHandler)
        {

        }

        public ChannelReader<ChunkStreamMessage> GetUpdateStream()
        {
            return OutgoingChannel.Reader;
        }

        public async Task RequestChunkState(ChunkKey key)
        {
            await SendChunkStateAsync(key);
        }

        public async Task<int> ProcessChunkAction(ChunkKey key, ChunkAction action)
        {
            return await BattleHandler.GetOrCreateChunkHandler(key).ProcessAsync(action);
        }

        public async Task EnqueueChunkAction(ChunkKey key, ChunkAction action)
        {
            await BattleHandler.GetOrCreateChunkHandler(key).EnqueueAsync(action);
        }

        public void UnsubscribeFromChunk(ChunkKey key)
        {
            if (Subscriptions.TryRemove(key, out var subscription))
            {
                subscription.Dispose();
            }
        }
        
        public async Task SubscribeToChunk(ChunkKey key)
        {
            var channel = OutgoingChannel;//copy to local for closure;
            var subscription = await BattleHandler.GetOrCreateChunkHandler(key).SubscribeAsync((chunkKey, update) => OnChunkUpdateAsync(key, update, channel));
            if (Subscriptions.TryAdd(key, subscription))
            {
                await SendChunkStateAsync(key);
            }
            else
            {
                subscription.Dispose();
            }
        }

        private async Task SendChunkStateAsync(ChunkKey key)
        {
            var state = await BattleHandler.GetOrCreateChunkHandler(key).GetStateAsync();
            await OutgoingChannel.Writer.WriteAsync(new ChunkStreamMessage(key, state));
        }

        private async Task OnChunkUpdateAsync(ChunkKey key, ChunkUpdate update, Channel<ChunkStreamMessage> channel)
        {
            await OutgoingChannel.Writer.WriteAsync(new ChunkStreamMessage(key, update));
        }
    }
}