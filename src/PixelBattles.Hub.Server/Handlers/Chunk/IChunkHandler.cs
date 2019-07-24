using System;
using System.Threading.Tasks;

namespace PixelBattles.Hub.Server.Handlers.Chunk
{
    public interface IChunkHandler
    {
        long LastUpdatedTicksUTC { get; }
        int SubscriptionCounter { get; }
        ChunkKey ChunkKey { get; }

        Task<ChunkState> GetStateAsync();
        Task<IChunkHandlerSubscription> SubscribeAsync(Func<ChunkKey, ChunkUpdate, Task> onUpdate);
        Task<int> ProcessAsync(ChunkAction chunkAction);
        Task EnqueueAsync(ChunkAction chunkAction);
        Task CloseAsync();
        Task SubscribeToChunkAsync();
    }
}
