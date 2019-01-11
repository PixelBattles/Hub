using PixelBattles.Chunkler;
using System;
using System.Threading.Tasks;

namespace PixelBattles.Hub.Server.Subscriptions
{
    public interface IChunkSubscriptions : IDisposable
    {
        Task<IChunkSubscription> SubscribeAsync(Action<ChunkUpdate> onUpdate);
        Task<ChunkState> GetStateAsync();
        Task<int> ProcessActionAsync(ChunkAction chunkAction);
    }
}