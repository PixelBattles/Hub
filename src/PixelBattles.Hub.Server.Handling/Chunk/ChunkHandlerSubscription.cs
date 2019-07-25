using System;

namespace PixelBattles.Hub.Server.Handlers.Chunk
{
    internal class ChunkHandlerSubscription : IChunkHandlerSubscription
    {
        private ChunkHandler _handler;
        public IChunkHandler ChunkHandler => _handler;
        public ChunkHandlerSubscription(ChunkHandler handler)
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