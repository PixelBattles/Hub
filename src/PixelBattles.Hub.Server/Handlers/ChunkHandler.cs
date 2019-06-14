using PixelBattles.Chunkler.Client;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace PixelBattles.Hub.Server.Handlers
{
    public class ChunkHandler
    {
        private long _battleId;
        private ChunkKey _chunkKey;
        private IChunklerClient _chunklerClient;

        private ConcurrentDictionary<IChunkSubscription, Func<ChunkKey, ChunkUpdate, Task>> _subscriptions = new ConcurrentDictionary<IChunkSubscription, Func<ChunkKey, ChunkUpdate, Task>>();

        public ChunkHandler(long battleId, ChunkKey chunkKey, IChunklerClient chunklerClient)
        {
            _battleId = battleId;
            _chunkKey = chunkKey;
            _chunklerClient = chunklerClient;

            _chunklerClient.SubscribeOnUpdateAsync(
            new Chunkler.ChunkKey
            {
                BattleId = battleId,
                ChunkXIndex = chunkKey.X,
                ChunkYIndex = chunkKey.Y    
            }, OnChunkUpdateAsync).Wait();
        }

        public async Task<ChunkState> GetStateAsync()
        {
            var state = await _chunklerClient.GetChunkStateAsync(new Chunkler.ChunkKey
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

        public Task<IChunkSubscription> SubscribeAsync(Func<ChunkKey, ChunkUpdate, Task> onUpdate)
        {
            var subscription = new ChunkSubscription(this);
            if (_subscriptions.TryAdd(subscription, onUpdate))
            {
                return Task.FromResult((IChunkSubscription)subscription);
            }
            else
            {
                throw new Exception();
            }
        }

        public async Task<int> ProcessAsync(ChunkAction chunkAction)
        {
            return await _chunklerClient.ProcessActionAsync(
                new Chunkler.ChunkKey
                {
                    BattleId = _battleId,
                    ChunkXIndex = _chunkKey.X,
                    ChunkYIndex = _chunkKey.Y
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
            await _chunklerClient.EnqueueActionAsync(
                new Chunkler.ChunkKey
                {
                    BattleId = _battleId,
                    ChunkXIndex = _chunkKey.X,
                    ChunkYIndex = _chunkKey.Y
                },
                new Chunkler.ChunkAction
                {
                    XIndex = chunkAction.X,
                    YIndex = chunkAction.Y,
                    Color = chunkAction.Color
                });
        }

        public void Unsubscribe(IChunkSubscription subscription)
        {
            _subscriptions.TryRemove(subscription, out var ignore);
        }

        private async Task OnChunkUpdateAsync(Chunkler.ChunkUpdate chunkUpdate)
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
                await subscription.Value(_chunkKey, update);
            }
        }
    }
}