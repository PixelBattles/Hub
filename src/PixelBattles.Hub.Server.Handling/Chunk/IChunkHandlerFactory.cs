using PixelBattles.Chunkler.Client;
using PixelBattles.Hub.Server.Handling.Chunk;
using System.Threading.Tasks;

namespace PixelBattles.Hub.Server.Handlers.Chunk
{
    internal interface IChunkHandlerFactory
    {
        Task<ChunkHandler> CreateChunkHandlerAsync(long battleId, ChunkSettings chunkSettings, ChunkKey chunkKey, IChunklerClient chunklerClient);
    }
}