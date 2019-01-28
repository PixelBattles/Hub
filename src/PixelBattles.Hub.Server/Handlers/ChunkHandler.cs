using PixelBattles.Chunkler.Client;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace PixelBattles.Hub.Server.Handlers
{
    public class ChunkHandler
    {
        private Guid _battleId;
        private ChunkKey _chunkKey;
        private IChunklerClient _chunklerClient;

        private ConcurrentDictionary<Subscription, Action<ChunkUpdate>> _subscriptions = new ConcurrentDictionary<Subscription, Action<ChunkUpdate>>();

        public ChunkHandler(Guid battleId, ChunkKey chunkKey, IChunklerClient chunklerClient)
        {
            _battleId = battleId;
            _chunkKey = chunkKey;
            _chunklerClient = chunklerClient;

            _chunklerClient.SubscribeAsync(
            new Chunkler.ChunkKey
            {
                BattleId = battleId,
                ChunkXIndex = chunkKey.X,
                ChunkYIndex = chunkKey.Y
            }, OnChunkUpdate).Wait();
        }

        public async Task<ChunkState> GetStateAsync()
        {
            var state = await _chunklerClient.GetChunkState(new Chunkler.ChunkKey
            {
                BattleId = _battleId,
                ChunkXIndex = _chunkKey.X,
                ChunkYIndex = _chunkKey.Y
            });

            return new ChunkState
            {
                ChangeIndex = state.ChangeIndex,
                Image = state.Image
            };
        }

        public Task<Subscription> SubscribeAsync(Action<ChunkUpdate> onUpdate)
        {
            var subscription = new Subscription(this);
            if (_subscriptions.TryAdd(subscription, onUpdate))
            {
                return Task.FromResult(subscription);
            }
            else
            {
                throw new Exception();
            }
        }

        public void Unsubscribe(Subscription subscription)
        {
            _subscriptions.TryRemove(subscription, out var ignore);
        }

        private void OnChunkUpdate(Chunkler.ChunkUpdate chunkUpdate)
        {
            var update = new ChunkUpdate
            {
                X = chunkUpdate.XIndex,
                Y = chunkUpdate.YIndex,
                Color = chunkUpdate.Color,
                ChangeIndex = chunkUpdate.ChangeIndex
            };

            foreach (var subscription in _subscriptions)
            {
                subscription.Value(update);
            }
        }

        public class Subscription : IDisposable
        {
            private ChunkHandler _handler;
            public Subscription(ChunkHandler handler)
            {
                _handler = handler ?? throw new ArgumentNullException();
            }

            public void Dispose()
            {
                _handler?.Unsubscribe(this);
                _handler = null;
            }
        }
    }
}
