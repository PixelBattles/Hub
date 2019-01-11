using PixelBattles.Chunkler;
using PixelBattles.Chunkler.Client;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace PixelBattles.Hub.Server.Subscriptions
{
    public class ChunkSubscriptions : IChunkSubscriptions
    {
        private IChunklerClient _chunklerClient;

        private int? _latestChangeIndex = null;
        private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        private ConcurrentDictionary<IChunkSubscription, Action<ChunkUpdate>> _subscriptions = new ConcurrentDictionary<IChunkSubscription, Action<ChunkUpdate>>();

        private readonly ChunkKey _key;
        private ChunkState _chunkState;

        public static async Task<IChunkSubscriptions> CreateChunkSubscriptionsAsync(IChunklerClient chunklerClient, ChunkKey chunkKey)
        {
            var subs = new ChunkSubscriptions(chunklerClient, chunkKey);
            await subs.SubscribeToChunk();
            return subs;
        }

        private ChunkSubscriptions(IChunklerClient chunklerClient, ChunkKey key)
        {
            _chunklerClient = chunklerClient ?? throw new ArgumentNullException(nameof(chunklerClient));
            _key = key ?? throw new ArgumentNullException(nameof(key));
        }
        
        public Task<int> ProcessActionAsync(ChunkAction chunkAction)
        {
            return _chunklerClient.ProcessAction(_key, chunkAction);
        }

        public Task<IChunkSubscription> SubscribeAsync(Action<ChunkUpdate> onUpdate)
        {
            IChunkSubscription subscription = new ChunkSubscription(this);
            if (!_subscriptions.TryAdd(subscription, onUpdate))
            {
                throw new InvalidOperationException("Subscription is already was added.");
            }
            return Task.FromResult(subscription);
        }

        public async Task<ChunkState> GetStateAsync()
        {
            try
            {
                await _semaphoreSlim.WaitAsync();
                if (_chunkState == null || _latestChangeIndex > _chunkState.ChangeIndex)
                {
                    _chunkState = await _chunklerClient.GetChunkState(_key);
                }
                return _chunkState;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        public void CloseSubscription(ChunkSubscription subscription)
        {
            _subscriptions.TryRemove(subscription, out var ignore);
        }

        private void OnChunkUpdate(ChunkUpdate chunkUpdate)
        {
            _latestChangeIndex = chunkUpdate.ChangeIndex;
            foreach (var subscription in _subscriptions)
            {
                subscription.Value(chunkUpdate);
            }
        }

        private Task SubscribeToChunk()
        {
            return _chunklerClient.Subscribe(_key, OnChunkUpdate);
        }

        public void Dispose()
        {
            if (_chunklerClient != null)
            {
                _chunklerClient.Unsubscribe(_key).Wait();
                _chunklerClient = null;
            }
        }
    }
}