using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PixelBattles.Hub.Server.Handlers;
using System;
using System.Collections.Concurrent;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace PixelBattles.Hub.Server.Hubs
{
    [Authorize(JwtBearerDefaults.AuthenticationScheme)]
    public class BattleHub : Hub<IBattleHubClient>
    {
        private readonly MainHandler _battleHandlerManager;
        private BattleHandler _battleHandler;

        public BattleHub(MainHandler battleHandlerManager)
        {
            _battleHandlerManager = battleHandlerManager ?? throw new ArgumentNullException();
        }

        private Guid GetBattleId()
        {
            return Guid.Parse(Context.User.FindFirst(BattleTokenConstants.BattleIdClaim).Value);
        }

        public async override Task OnConnectedAsync()
        {
            Context.Items["battleHandler"] = await _battleHandlerManager.GetOrCreateBattleHandlerAsync(GetBattleId());
            Context.Items["subscriptions"] = new ConcurrentBag<ChunkHandler.Subscription>();
            await base.OnConnectedAsync();
        }

        public async override Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);

            foreach (var subscription in Context.Items["subscriptions"] as ConcurrentBag<ChunkHandler.Subscription>)
            {
                subscription.Dispose();
            }
        }

        public ChannelReader<ChunkStreamMessage> SubscribeToChunkStream()
        {
            var channel = Channel.CreateUnbounded<ChunkStreamMessage>();
            Context.Items["stream"] = channel;
            return channel.Reader;
        }

        public void GetState(ChunkKey key)
        {
            var ignore = WriteStateAsync(key);
        }

        public async Task<int> Process(ChunkKey key, ChunkUpdate update)
        {
            return await (Context.Items["battleHandler"] as BattleHandler).GetOrCreateChunkHandler(key).ProcessAsync(update);
        }

        public void UnsubscribeFromChunk(ChunkKey key)
        {
        }
        
        public async Task SubscribeToChunk(ChunkKey key)
        {
            var subscription = await (Context.Items["battleHandler"] as BattleHandler).GetOrCreateChunkHandler(key).SubscribeAsync(OnChunkUpdate);
            (Context.Items["subscriptions"] as ConcurrentBag<ChunkHandler.Subscription>).Add(subscription);
            var ignore = WriteStateAsync(key);
        }

        private async Task WriteStateAsync(ChunkKey key)
        {
            var channel = (Channel<ChunkStreamMessage>)Context.Items["stream"];
            var state = await (Context.Items["battleHandler"] as BattleHandler).GetOrCreateChunkHandler(key).GetStateAsync();
            await channel.Writer.WriteAsync(new ChunkStreamMessage { State = state, Key = key });
        }

        private void OnChunkUpdate(ChunkUpdate chunkUpdate)
        {

        }
    }
}