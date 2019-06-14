using PixelBattles.Chunkler;

namespace PixelBattles.Hub.Server.Hubs.Client
{
    public interface IBattleHubClient
    {
        void OnUpdate(ChunkKey chunkKey, ChunkUpdate chunkUpdate);
    }
}