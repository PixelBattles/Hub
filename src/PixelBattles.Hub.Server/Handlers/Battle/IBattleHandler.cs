using PixelBattles.Hub.Server.Handlers.Chunk;
using System;
using System.Threading.Tasks;

namespace PixelBattles.Hub.Server.Handlers.Battle
{
    public interface IBattleHandler : IDisposable
    {
        int SubscriptionCounter { get; }
        long BattleId { get; }
        long LastUpdatedTicksUTC { get; }

        Task<IChunkHandler> GetOrCreateChunkHandlerAsync(ChunkKey chunkKey);
        Task<(int chunkHandlersNotCompacted, int chunkHandlersCompacted)> CompactChunkHandlersAsync(long unusedChunkHanlderTicksUTCLimit);
        Task<(int chunkHandlersNotRemoved, int chunkHandlersRemoved)> ClearCompactedChunkHandlersAsync();
        void IncrementSubscriptionCounter();
        void DecrementSubscriptionCounter();
    }
}