using Microsoft.AspNetCore.SignalR;
using PixelBattles.Hub.Server.Handlers.Battle;
using PixelBattles.Hub.Server.Handlers.Chunk;
using PixelBattles.Hub.Server.Handlers.Main;
using PixelBattles.Hub.Server.Hubs.Client;
using System;
using System.Collections.Concurrent;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace PixelBattles.Hub.Server.Hubs
{
    public abstract class BaseHub: Hub<IBattleHubClient>
    {
        private const string BattleHandlerKey = "battleHandler";
        private const string SubscriptionsKey = "subscriptions";
        private const string OutgoingStreamKey = "stream";

        private readonly IMainHandler _mainHandler;

        public BaseHub(IMainHandler mainHandler)
        {
            _mainHandler = mainHandler ?? throw new ArgumentNullException(nameof(mainHandler));
        }

        public async override Task OnConnectedAsync()
        {
            BattleHandler = await _mainHandler.GetBattleHandlerAndSubscribeAsync(BattleId);
            Subscriptions = new ConcurrentDictionary<ChunkKey, IChunkHandlerSubscription>();
            OutgoingChannel = Channel.CreateUnbounded<ChunkStreamMessage>();
            await base.OnConnectedAsync();
        }

        public async override Task OnDisconnectedAsync(Exception exception)
        {
            BattleHandler?.DecrementSubscriptionCounter();
            foreach (var subscription in Subscriptions.Values)
            {
                subscription.Dispose();
            }
            Subscriptions.Clear();

            await base.OnDisconnectedAsync(exception);
        }
        
        private long? _battleId;
        protected long BattleId
        {
            get
            {
                if (!_battleId.HasValue)
                {
                    _battleId = long.Parse(Context.User.FindFirst(BattleTokenConstants.BattleIdClaim).Value);
                }
                return _battleId.Value;
            }
        }

        private ConcurrentDictionary<ChunkKey, IChunkHandlerSubscription> _subscriptions;
        protected ConcurrentDictionary<ChunkKey, IChunkHandlerSubscription> Subscriptions
        {
            get
            {
                if (_subscriptions == null)
                {
                    _subscriptions = (ConcurrentDictionary<ChunkKey, IChunkHandlerSubscription>)Context.Items[SubscriptionsKey];
                }
                return _subscriptions;
            }
            private set
            {
                _subscriptions = value;
                Context.Items[SubscriptionsKey] = value;
            }
        }

        private IBattleHandler _battleHandler;
        protected IBattleHandler BattleHandler
        {
            get
            {
                if (_battleHandler == null)
                {
                    _battleHandler = (IBattleHandler)Context.Items[BattleHandlerKey];
                }
                return _battleHandler;
            }
            private set
            {
                _battleHandler = value;
                Context.Items[BattleHandlerKey] = value;
            }
        }

        private Channel<ChunkStreamMessage> _outgoingChannel;
        protected Channel<ChunkStreamMessage> OutgoingChannel
        {
            get
            {
                if (_outgoingChannel == null)
                {
                    _outgoingChannel = (Channel<ChunkStreamMessage>)Context.Items[OutgoingStreamKey];
                }
                return _outgoingChannel;
            }
            private set
            {
                _outgoingChannel = value;
                Context.Items[OutgoingStreamKey] = value;
            }
        }
    }
}
