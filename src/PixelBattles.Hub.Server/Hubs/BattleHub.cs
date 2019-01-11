using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PixelBattles.Chunkler;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PixelBattles.Hub.Server.Hubs
{
    [Authorize(JwtBearerDefaults.AuthenticationScheme)]
    public class BattleHub : Hub<IBattleHubClient>
    {
        private BattleHubContext _battleHubContext;
        private HashSet<ChunkKey> _subscriptions = new HashSet<ChunkKey>();
        
        public BattleHub(BattleHubContext battleHubContext)
        {
            _battleHubContext = battleHubContext ?? throw new ArgumentNullException(nameof(battleHubContext));
        }

        private Guid GetBattleId()
        {
            return Guid.Parse(Context.User.FindFirst(BattleTokenConstants.BattleIdClaim).Value);
        }

        public async override Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public async override Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
            foreach (var subscription in _subscriptions)
            {
                await _battleHubContext.ChunklerClient.Unsubscribe(subscription);
            }
        }

        public Task<ChunkState> GetChunkState(ChunkKey key)
        {
            return _battleHubContext.ChunklerClient.GetChunkState(key);
        }

        public Task<int> ProcessAction(ChunkKey key, ChunkAction action)
        {
            throw new NotImplementedException();
        }

        public async Task SubscribeChunk(ChunkKey key)
        {
            if (_subscriptions.Add(key))
            {
                await _battleHubContext.ChunklerClient.Subscribe(key, (update) => Clients.Caller.OnUpdate(key, update));
            }
        }

        public void UnsubscribeChunk(ChunkKey key)
        {
            throw new NotImplementedException();
        }
    }
}