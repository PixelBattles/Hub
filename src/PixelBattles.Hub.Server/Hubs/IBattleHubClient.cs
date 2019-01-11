using PixelBattles.Chunkler;

namespace PixelBattles.Hub.Server.Hubs
{
    public interface IBattleHubClient
    {
        void OnUpdate(ChunkKey chunkKey, ChunkUpdate chunkUpdate);
    }
}
