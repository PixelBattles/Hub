using PixelBattles.Chunkler.Client;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace PixelBattles.Hub.Server.Handlers.Chunk
{
    internal class ChunkHandler : IChunkHandler
    {
        private readonly long _battleId;
        private readonly IChunklerClient _chunklerClient;
        private readonly ConcurrentDictionary<IChunkHandlerSubscription, Func<ChunkKey, ChunkUpdate, Task>> _subscriptions;
        private IChunkSubscription _chunkSubscription;

        private int _subscriptionCounter;
        public int SubscriptionCounter => _subscriptionCounter;

        private readonly ChunkKey _chunkKey;
        public ChunkKey ChunkKey => _chunkKey;

        public long _lastUpdatedTicksUTC;
        public long LastUpdatedTicksUTC => _lastUpdatedTicksUTC;

        private bool _isClosing;
        public bool IsClosing => Volatile.Read(ref _isClosing);

        public ChunkHandler(long battleId, ChunkKey chunkKey, IChunklerClient chunklerClient)
        {
            _chunkKey = chunkKey;
            _chunklerClient = chunklerClient ?? throw new ArgumentNullException(nameof(chunklerClient));
            _subscriptions = new ConcurrentDictionary<IChunkHandlerSubscription, Func<ChunkKey, ChunkUpdate, Task>>();
            _lastUpdatedTicksUTC = DateTime.MinValue.Ticks;
            _subscriptionCounter = 0;
            _battleId = battleId;
        }

        public async Task<ChunkState> GetStateAsync()
        {
            //we do not need an accurate value
            _lastUpdatedTicksUTC = DateTime.UtcNow.Ticks;

            var state = await _chunklerClient.GetChunkStateAsync(new Chunkler.ChunkKey
            {
                BattleId = _battleId,
                ChunkXIndex = ChunkKey.X,
                ChunkYIndex = ChunkKey.Y
            });

            return new ChunkState
            {
                ChangeIndex = state.ChangeIndex,
                Image = state.Image
            };
        }

        public ChunkHandlerSubscription CreateSubscription(Func<ChunkKey, ChunkUpdate, Task> onUpdate)
        {
            //we do not need an accurate value
            _lastUpdatedTicksUTC = DateTime.UtcNow.Ticks;

            var subscription = new ChunkHandlerSubscription(this);
            if (_subscriptions.TryAdd(subscription, onUpdate))
            {
                return subscription;
            }
            else
            {
                throw new InvalidOperationException("Error during add subscription handler to chunk.");
            }
        }

        public async Task<int> ProcessAsync(ChunkAction chunkAction)
        {
            //we do not need an accurate value
            _lastUpdatedTicksUTC = DateTime.UtcNow.Ticks;

            return await _chunklerClient.ProcessChunkActionAsync(
                new Chunkler.ChunkKey
                {
                    BattleId = _battleId,
                    ChunkXIndex = ChunkKey.X,
                    ChunkYIndex = ChunkKey.Y
                },
                new Chunkler.ChunkAction
                {
                    XIndex = chunkAction.X,
                    YIndex = chunkAction.Y,
                    Color = chunkAction.Color
                });
        }

        public async Task EnqueueAsync(ChunkAction chunkAction)
        {
            //we do not need an accurate value
            _lastUpdatedTicksUTC = DateTime.UtcNow.Ticks;

            await _chunklerClient.EnqueueChunkActionAsync(
                new Chunkler.ChunkKey
                {
                    BattleId = _battleId,
                    ChunkXIndex = ChunkKey.X,
                    ChunkYIndex = ChunkKey.Y
                },
                new Chunkler.ChunkAction
                {
                    XIndex = chunkAction.X,
                    YIndex = chunkAction.Y,
                    Color = chunkAction.Color
                });
        }

        public void Unsubscribe(ChunkHandlerSubscription subscription)
        {
            //we do not need an accurate value
            _lastUpdatedTicksUTC = DateTime.UtcNow.Ticks;

            if (_subscriptions.TryRemove(subscription, out var ignore))
            {
                Interlocked.Decrement(ref _subscriptionCounter);
            }
        }

        private async Task OnChunkUpdateAsync(Chunkler.ChunkUpdate chunkUpdate)
        {
            //we do not need an accurate value
            _lastUpdatedTicksUTC = DateTime.UtcNow.Ticks;

            var update = new ChunkUpdate
            {
                X = chunkUpdate.XIndex,
                Y = chunkUpdate.YIndex,
                Color = chunkUpdate.Color,
                ChangeIndex = chunkUpdate.ChangeIndex
            };

            foreach (var subscription in _subscriptions)
            {
                await subscription.Value(ChunkKey, update);
            }
        }
        
        public async Task CloseAsync()
        {
            await _chunkSubscription.CloseAsync();
        }

        public void MarkAsClosing()
        {
            Volatile.Write(ref _isClosing, true);
        }

        public async Task SubscribeToChunkAsync()
        {
            _chunkSubscription = await _chunklerClient.SubscribeOnChunkUpdateAsync(
                key: new Chunkler.ChunkKey
                {
                    BattleId = _battleId,
                    ChunkXIndex = _chunkKey.X,
                    ChunkYIndex = _chunkKey.Y
                },
                onUpdate: OnChunkUpdateAsync);
        }

        public void IncrementSubscriptionCounter()
        {
            Interlocked.Increment(ref _subscriptionCounter);
        }

        public void DecrementSubscriptionCounter()
        {
            Interlocked.Decrement(ref _subscriptionCounter);
        }
    }
}