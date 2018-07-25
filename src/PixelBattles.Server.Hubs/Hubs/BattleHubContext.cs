using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace PixelBattles.Server.Hubs
{
    public class BattleHubContext
    {
        //protected ConcurrentDictionary<Guid, BattleDTO> Battles { get; set; }

        protected IHubContext<BattleHub> GameHubContext { get; set; }

        protected IServiceScopeFactory ServiceScopeFactory { get; set; }

        public BattleHubContext(
            IServiceScopeFactory serviceScopeFactory,
            IHubContext<BattleHub> gameHubContext)
        {
            this.GameHubContext = gameHubContext;
            this.ServiceScopeFactory = serviceScopeFactory;
            //this.Battles = new ConcurrentDictionary<Guid, GameInfoDTO>();
        }

        //public async Task<BattleDTO> GetBattleAsync(Guid battleId)
        //{
        //    if (Battles.TryGetValue(battleId, out BattleDTO battle))
        //    {
        //        return battle;
        //    }
        //    else
        //    {
        //        using (var scope = ServiceScopeFactory.CreateScope())
        //        {
        //            var battleManager = scope.ServiceProvider.GetRequiredService<IBattleManager>();
        //            var battleResult = await battleManager.GetBattleAsync(battleId);
        //             = new GameInfoDTO()
        //            {
        //                GameId = gameId,
        //                GameSizeX = game.Width,
        //                GameSizeY = game.Height,
        //                ChunkSizeX = game.Width,
        //                ChunkSizeY = game.Height
        //            };
        //        }
        //        if (gameInfo != null)
        //        {
        //            gameInfo = Battles.AddOrUpdate(gameId, gameInfo, (k, v) => v);
        //        }
        //    }
        //    return gameInfo;
        //}
    }
}
