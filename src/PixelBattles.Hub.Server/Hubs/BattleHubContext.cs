using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using PixelBattles.Chunkler.Client;

namespace PixelBattles.Hub.Server.Hubs
{
    public class BattleHubContext
    {
        public IHubContext<BattleHub> GameHubContext { get; set; }

        public IChunklerClient ChunklerClient { get; set; }

        public IServiceScopeFactory ServiceScopeFactory { get; set; }

        public BattleHubContext(
            IServiceScopeFactory serviceScopeFactory,
            IHubContext<BattleHub> gameHubContext)
        {
            this.GameHubContext = gameHubContext;
            this.ServiceScopeFactory = serviceScopeFactory;
        }
    }
}