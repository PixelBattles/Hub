using System.Threading.Tasks;

namespace PixelBattles.Hub.Server.Handlers.Chunk
{
    public interface IChunkHandler
    {
        ChunkKey ChunkKey { get; }

        Task<ChunkState> GetStateAsync();
        Task<int> ProcessAsync(ChunkAction chunkAction);
        Task EnqueueAsync(ChunkAction chunkAction);
    }
}