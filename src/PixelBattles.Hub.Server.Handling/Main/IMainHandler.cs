using PixelBattles.Hub.Server.Handlers.Battle;
using System;
using System.Threading.Tasks;

namespace PixelBattles.Hub.Server.Handling.Main
{
    public interface IMainHandler : IDisposable
    {
        Task<IBattleHandlerSubscription> GetBattleHandlerAndSubscribeAsync(long battleId);
    }
}