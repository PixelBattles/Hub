using PixelBattles.Hub.Server.Handlers.Chunk;
using System;
using System.Threading.Tasks;

namespace PixelBattles.Hub.Server.Handlers.Battle
{
    public interface IBattleHandler : IDisposable
    {
        long BattleId { get; }

        Task<IChunkHandlerSubscription> GetChunkHandlerAndSubscribeAsync(ChunkKey chunkKey, Func<ChunkKey, ChunkUpdate, Task> onUpdate);
    }
}