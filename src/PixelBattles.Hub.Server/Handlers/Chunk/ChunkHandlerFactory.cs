using PixelBattles.Chunkler.Client;
using System.Threading.Tasks;

namespace PixelBattles.Hub.Server.Handlers.Chunk
{
    public class ChunkHandlerFactory : IChunkHandlerFactory
    {

        public ChunkHandlerFactory()
        {
        }

        public async Task<IChunkHandler> CreateChunkHandlerAsync(long battleId, ChunkKey chunkKey, IChunklerClient chunklerClient)
        {
            var handler = new ChunkHandler(battleId, chunkKey, chunklerClient);
            await handler.SubscribeToChunkAsync();
            return handler;
        }
    }
}