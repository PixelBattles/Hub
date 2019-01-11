using PixelBattles.API.DataTransfer.Battle;
using System;
using System.Collections.Generic;

namespace PixelBattles.Hub.Server.Subscriptions
{
    public class BattleSubscriptions
    {
        internal int _state;
        public SubscriptionState State => (SubscriptionState)_state;

        private readonly Guid _battleId;
        private readonly Dictionary<string, ChunkSubscriptions> _chunkSubscriptions;

        public BattleSubscriptions(BattleDTO battle)
        {
            if (battle == null)
            {
                throw new ArgumentNullException(nameof(battle));
            }
            _battleId = battle.BattleId;

            _chunkSubscriptions = new Dictionary<string, ChunkSubscriptions>();
        }

        //private async ChunkSubscriptions GetOrAddChunkSubscriptionsAsync(int xIndex, int yIndex)
        //{

        //}
    }
}
