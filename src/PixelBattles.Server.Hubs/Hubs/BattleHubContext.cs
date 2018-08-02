using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace PixelBattles.Server.Hubs
{
    public class BattleHubContext
    {
        protected IHubContext<BattleHub> GameHubContext { get; set; }

        protected IServiceScopeFactory ServiceScopeFactory { get; set; }

        public BattleHubContext(
            IServiceScopeFactory serviceScopeFactory,
            IHubContext<BattleHub> gameHubContext)
        {
            this.GameHubContext = gameHubContext;
            this.ServiceScopeFactory = serviceScopeFactory;
        }
    }
}
