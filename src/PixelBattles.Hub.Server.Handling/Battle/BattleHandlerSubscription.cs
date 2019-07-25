using System;

namespace PixelBattles.Hub.Server.Handlers.Battle
{
    internal class BattleHandlerSubscription : IBattleHandlerSubscription
    {
        private BattleHandler _handler;
        public IBattleHandler BattleHandler => _handler;
        public BattleHandlerSubscription(BattleHandler handler)
        {
            _handler = handler ?? throw new ArgumentNullException();
        }

        public void Dispose()
        {
            _handler?.DecrementSubscriptionCounter();
            _handler = null;
        }
    }
}