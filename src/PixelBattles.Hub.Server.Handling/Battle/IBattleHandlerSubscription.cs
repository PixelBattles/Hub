using System;

namespace PixelBattles.Hub.Server.Handlers.Battle
{
    public interface IBattleHandlerSubscription : IDisposable
    {
        IBattleHandler BattleHandler { get; }
    }
}