namespace PixelBattles.Hub.Server.Handlers.Chunk
{
    public class ChunkStreamMessage
    {
        public ChunkStreamMessage(ChunkKey key, ChunkState state)
        {
            Key = key;
            State = state;
        }

        public ChunkStreamMessage(ChunkKey key, ChunkUpdate update)
        {
            Key = key;
            Update = update;
        }

        public ChunkKey Key;
        public ChunkState State;
        public ChunkUpdate Update;
    }
}