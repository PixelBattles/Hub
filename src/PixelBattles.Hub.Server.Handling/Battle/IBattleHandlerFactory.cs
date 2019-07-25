using System.Threading.Tasks;

namespace PixelBattles.Hub.Server.Handlers.Battle
{
    internal interface IBattleHandlerFactory
    {
        Task<BattleHandler> CreateBattleHandlerAsync(long battleId);
    }
}