using System.Threading.Tasks;

namespace PixelBattles.Hub.Server.Handlers.Battle
{
    public interface IBattleHandlerFactory
    {
        Task<IBattleHandler> CreateBattleHandlerAsync(long battleId);
    }
}