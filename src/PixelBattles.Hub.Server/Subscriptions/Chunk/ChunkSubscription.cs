using System;

namespace PixelBattles.Hub.Server.Subscriptions
{
    public class ChunkSubscription : IChunkSubscription
    {
        private ChunkSubscriptions _chunkSubscriptions;

        public ChunkSubscription(ChunkSubscriptions chunkSubscriptions)
        {
            _chunkSubscriptions = chunkSubscriptions ?? throw new ArgumentNullException(nameof(chunkSubscriptions));
        }

        public void Dispose()
        {
            _chunkSubscriptions.CloseSubscription(this);
        }
    }
}
