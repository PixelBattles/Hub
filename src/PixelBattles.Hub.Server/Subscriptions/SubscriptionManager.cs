using PixelBattles.API.Client;
using PixelBattles.API.DataTransfer.Battle;
using PixelBattles.Chunkler.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PixelBattles.Hub.Server.Subscriptions
{
    public class SubscriptionManager
    {
        private readonly IChunklerClient _chunklerClient;
        private readonly IApiClient _apiClient;

        private Dictionary<Guid, BattleSubscriptions> _battleSubscriptions;
        public SubscriptionManager(IChunklerClient chunklerClient, IApiClient apiClient)
        {
            _battleSubscriptions = new Dictionary<Guid, BattleSubscriptions>();
            _chunklerClient = chunklerClient ?? throw new ArgumentNullException(nameof(chunklerClient));
        }

        private Task<BattleDTO> GetBattleSubscription(Guid battleId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _apiClient.GetBattleAsync(battleId, cancellationToken);
        }

        private void Clear()
        {
            foreach (var battle in _battleSubscriptions.Values)
            {
                Interlocked.CompareExchange(ref battle._state, (int)SubscriptionState.Disabling, (int)SubscriptionState.NotUsed);
            }
        }
    }
}