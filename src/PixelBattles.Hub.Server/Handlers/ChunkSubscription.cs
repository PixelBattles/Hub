using System;

namespace PixelBattles.Hub.Server.Handlers
{
    public class ChunkSubscription : IChunkSubscription
    {
        private ChunkHandler _handler;
        public ChunkSubscription(ChunkHandler handler)
        {
            _handler = handler ?? throw new ArgumentNullException();
        }

        public void Dispose()
        {
            _handler?.Unsubscribe(this);
            _handler = null;
        }
    }
}