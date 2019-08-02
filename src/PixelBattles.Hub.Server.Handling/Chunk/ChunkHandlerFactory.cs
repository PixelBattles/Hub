using PixelBattles.Chunkler.Client;
using PixelBattles.Hub.Server.Handling.Chunk;
using System.Threading.Tasks;

namespace PixelBattles.Hub.Server.Handlers.Chunk
{
    internal class ChunkHandlerFactory : IChunkHandlerFactory
    {
        public ChunkHandlerFactory()
        {
        }

        public async Task<ChunkHandler> CreateChunkHandlerAsync(long battleId, ChunkSettings chunkSettings, ChunkKey chunkKey, IChunklerClient chunklerClient)
        {
            var handler = new ChunkHandler(battleId, chunkSettings, chunkKey, chunklerClient);
            await handler.SubscribeToChunkAsync();
            return handler;
        }
    }
}