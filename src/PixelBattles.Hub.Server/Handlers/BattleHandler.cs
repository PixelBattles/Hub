using PixelBattles.Chunkler.Client;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace PixelBattles.Hub.Server.Handlers
{
    public class BattleHandler
    {
        private readonly Guid _battleId;
        private readonly IChunklerClient _chunklerClient;

        private readonly ConcurrentDictionary<ChunkKey, Lazy<ChunkHandler>> _chunkHandlers = new ConcurrentDictionary<ChunkKey, Lazy<ChunkHandler>>();

        public BattleHandler(Guid battleId, IChunklerClient chunklerClient)
        {
            _battleId = battleId;
            _chunklerClient = chunklerClient;
        }

        public ChunkHandler GetOrCreateChunkHandler(ChunkKey chunkKey)
        {
            return _chunkHandlers.GetOrAdd(chunkKey,
                key => new Lazy<ChunkHandler>(() => CreateChunkHandler(key), LazyThreadSafetyMode.ExecutionAndPublication)).Value;
        }

        private ChunkHandler CreateChunkHandler(ChunkKey chunkKey)
        {
            return new ChunkHandler(_battleId, chunkKey, _chunklerClient);
        }
    }
}
