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
        private const string BattleHandlerKey = "battleHandler";
        private const string SubscriptionsKey = "subscriptions";
        private const string OutgoingStreamKey = "stream";

        private readonly MainHandler _battleHandlerManager;

        public BattleHub(MainHandler battleHandlerManager)
        {
            _battleHandlerManager = battleHandlerManager ?? throw new ArgumentNullException();
        }

        private long GetBattleId()
        {
            return long.Parse(Context.User.FindFirst(BattleTokenConstants.BattleIdClaim).Value);
        }

        public async override Task OnConnectedAsync()
        {
            Context.Items[BattleHandlerKey] = await _battleHandlerManager.GetOrCreateBattleHandlerAsync(GetBattleId());
            Context.Items[SubscriptionsKey] = new ConcurrentDictionary<ChunkKey, ChunkHandler.Subscription>();
            await base.OnConnectedAsync();
        }

        public async override Task OnDisconnectedAsync(Exception exception)
        {
            var subscriptions = Context.Items[SubscriptionsKey] as ConcurrentDictionary<ChunkKey, ChunkHandler.Subscription>;
            foreach (var subscription in subscriptions.Values)
            {
                subscription.Dispose();
            }
            subscriptions.Clear();
            await base.OnDisconnectedAsync(exception);
        }

        public ChannelReader<ChunkStreamMessage> SubscribeToChunkStream()
        {
            var channel = Channel.CreateUnbounded<ChunkStreamMessage>();
            Context.Items[OutgoingStreamKey] = channel;
            return channel.Reader;
        }

        public async Task GetState(ChunkKey key)
        {
            await WriteStateAsync(key);
        }

        public async Task<int> ProcessAction(ChunkKey key, ChunkAction action)
        {
            var battleHandler = Context.Items[BattleHandlerKey] as BattleHandler;
            return await battleHandler.GetOrCreateChunkHandler(key).ProcessAsync(action);
        }

        public async Task EnqueueAction(ChunkKey key, ChunkAction action)
        {
            var battleHandler = Context.Items[BattleHandlerKey] as BattleHandler;
            await battleHandler.GetOrCreateChunkHandler(key).EnqueueAsync(action);
        }

        public void UnsubscribeFromChunk(ChunkKey key)
        {
            var subscriptions = Context.Items[SubscriptionsKey] as ConcurrentDictionary<ChunkKey, ChunkHandler.Subscription>;
            if (subscriptions.TryRemove(key, out var subscription))
            {
                subscription.Dispose();
            }
        }
        
        public async Task SubscribeToChunk(ChunkKey key)
        {
            var channel = Context.Items[OutgoingStreamKey] as Channel<ChunkStreamMessage>;
            var battleHandler = Context.Items[BattleHandlerKey] as BattleHandler;
            var subscriptions = Context.Items[SubscriptionsKey] as ConcurrentDictionary<ChunkKey, ChunkHandler.Subscription>;
            var subscription = await battleHandler.GetOrCreateChunkHandler(key).SubscribeAsync((chunkKey, update) => OnChunkUpdateAsync(key, update, channel));
            subscriptions.TryAdd(key, subscription);
            await WriteStateAsync(key);
        }

        private async Task WriteStateAsync(ChunkKey key)
        {
            var channel = Context.Items[OutgoingStreamKey] as Channel<ChunkStreamMessage>;
            var battleHandler = Context.Items[BattleHandlerKey] as BattleHandler;
            var state = await battleHandler.GetOrCreateChunkHandler(key).GetStateAsync();
            await channel.Writer.WriteAsync(new ChunkStreamMessage { State = state, Key = key });
        }

        private async Task OnChunkUpdateAsync(ChunkKey chunkKey, ChunkUpdate chunkUpdate, Channel<ChunkStreamMessage> channel)
        {
            await channel.Writer.WriteAsync(new ChunkStreamMessage { Update = chunkUpdate, Key = chunkKey });
        }
    }
}