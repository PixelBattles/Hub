using Nito.AsyncEx;
using PixelBattles.Chunkler.Client;
using PixelBattles.Hub.Server.Handlers.Chunk;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PixelBattles.Hub.Server.Handlers.Battle
{
    public class BattleHandler: IBattleHandler
    {   
        private readonly IChunklerClient _chunklerClient;
        private readonly IChunkHandlerFactory _chunkHandlerFactory;
        private readonly ConcurrentDictionary<ChunkKey, AsyncLazy<IChunkHandler>> _chunkHandlers;
        private readonly Dictionary<ChunkKey, AsyncLazy<IChunkHandler>> _compactedChunkHandlers;

        private int _subscriptionCounter;
        public int SubscriptionCounter => _subscriptionCounter;

        private readonly long _battleId;
        public long BattleId => _battleId;

        public long _lastUpdatedTicksUTC;
        public long LastUpdatedTicksUTC => _lastUpdatedTicksUTC;

        public BattleHandler(long battleId, IChunklerClient chunklerClient, IChunkHandlerFactory chunkHandlerFactory)
        {
            _battleId = battleId;
            _subscriptionCounter = 0;
            _lastUpdatedTicksUTC = DateTime.UtcNow.Ticks;
            _chunklerClient = chunklerClient ?? throw new ArgumentNullException(nameof(chunklerClient));
            _chunkHandlerFactory = chunkHandlerFactory ?? throw new ArgumentNullException(nameof(chunkHandlerFactory));
            _chunkHandlers = new ConcurrentDictionary<ChunkKey, AsyncLazy<IChunkHandler>>();
            _compactedChunkHandlers = new Dictionary<ChunkKey, AsyncLazy<IChunkHandler>>();
        }

        public async Task<IChunkHandler> GetOrCreateChunkHandlerAsync(ChunkKey chunkKey)
        {
            //we do not need an accurate value
            _lastUpdatedTicksUTC = DateTime.UtcNow.Ticks;

            var chunkHandler = await _chunkHandlers.GetOrAdd(
                key: chunkKey,
                valueFactory: key => new AsyncLazy<IChunkHandler>(
                    () => _chunkHandlerFactory.CreateChunkHandlerAsync(_battleId, chunkKey, _chunklerClient),
                    AsyncLazyFlags.RetryOnFailure));

            if (chunkHandler == null)
            {
                throw new InvalidOperationException($"Chunk {chunkKey} is not found.");
            }

            return chunkHandler;
        }
        
        public async Task<(int chunkHandlersNotCompacted, int chunkHandlersCompacted)> CompactChunkHandlersAsync(long unusedChunkHanlderTicksUTCLimit)
        {
            int chunkHandlersNotCompacted = 0;
            int chunkHandlersCompacted = 0;
            foreach (var chunkHandlerKVPair in _chunkHandlers)
            {
                if (_compactedChunkHandlers.ContainsKey(chunkHandlerKVPair.Key))
                {
                    ++chunkHandlersNotCompacted;
                    continue;
                }

                var chunkHandler = await chunkHandlerKVPair.Value;
                if (chunkHandler == null || (chunkHandler.LastUpdatedTicksUTC < unusedChunkHanlderTicksUTCLimit && chunkHandler.SubscriptionCounter == 0))
                {
                    if (_chunkHandlers.TryRemove(chunkHandlerKVPair.Key, out var ignore))
                    {
                        _compactedChunkHandlers.Add(chunkHandlerKVPair.Key, chunkHandlerKVPair.Value);
                        ++chunkHandlersCompacted;
                    }
                    else
                    {
                        throw new InvalidOperationException("ChunkHandler was removed during compaction in unexpected way.");
                    }
                }
                else
                {
                    ++chunkHandlersNotCompacted;
                }
            }
            return (chunkHandlersNotCompacted, chunkHandlersCompacted);
        }

        public async Task<(int chunkHandlersNotRemoved, int chunkHandlersRemoved)> ClearCompactedChunkHandlersAsync()
        {
            int chunkHandlersNotRemoved = 0;
            int chunkHandlersRemoved = 0;
            if (_compactedChunkHandlers.Count == 0)
            {
                return (chunkHandlersNotRemoved, chunkHandlersRemoved);
            }

            var chunkHandlersToDelete = new List<IChunkHandler>(_compactedChunkHandlers.Count);
            foreach (var chunkHandlerLazy in _compactedChunkHandlers)
            {
                var chunkHandler = await chunkHandlerLazy.Value;
                if (chunkHandler.SubscriptionCounter == 0)
                {
                    chunkHandlersToDelete.Add(chunkHandler);
                    ++chunkHandlersRemoved;
                }
                else
                {
                    ++chunkHandlersNotRemoved;
                }
            }

            foreach (var chunkHandler in chunkHandlersToDelete)
            {
                if (!_compactedChunkHandlers.Remove(chunkHandler.ChunkKey))
                {
                    throw new InvalidOperationException("ChunkHandler was deleted in unexpected way from compaction.");
                }
                await chunkHandler.CloseAsync();
            }
            return (chunkHandlersNotRemoved, chunkHandlersRemoved);
        }

        public void IncrementSubscriptionCounter()
        {
            Interlocked.Increment(ref _subscriptionCounter);
        }

        public void DecrementSubscriptionCounter()
        {
            Interlocked.Decrement(ref _subscriptionCounter);
        }

        public void Dispose()
        {
            _chunklerClient?.Dispose();
        }
    }
}
