using PixelBattles.Hub.Server.Handlers.Battle;
using System;
using System.Threading.Tasks;

namespace PixelBattles.Hub.Server.Handlers.Main
{
    public interface IMainHandler : IDisposable
    {
        Task<IBattleHandler> GetBattleHandlerAndSubscribeAsync(long battleId);
    }
}