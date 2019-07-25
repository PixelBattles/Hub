using System;

namespace PixelBattles.Hub.Server.Handlers.Chunk
{
    public interface IChunkHandlerSubscription : IDisposable
    {
        IChunkHandler ChunkHandler { get; }
    }
}