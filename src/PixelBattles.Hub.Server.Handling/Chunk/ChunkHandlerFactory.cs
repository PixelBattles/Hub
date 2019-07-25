using PixelBattles.Chunkler.Client;
using System.Threading.Tasks;

namespace PixelBattles.Hub.Server.Handlers.Chunk
{
    internal class ChunkHandlerFactory : IChunkHandlerFactory
    {
        public ChunkHandlerFactory()
        {
        }

        public async Task<ChunkHandler> CreateChunkHandlerAsync(long battleId, ChunkKey chunkKey, IChunklerClient chunklerClient)
        {
            var handler = new ChunkHandler(battleId, chunkKey, chunklerClient);
            await handler.SubscribeToChunkAsync();
            return handler;
        }
    }
}