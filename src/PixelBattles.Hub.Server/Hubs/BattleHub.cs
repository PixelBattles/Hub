using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PixelBattles.Chunkler;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PixelBattles.Server.Hubs
{
    [Authorize(JwtBearerDefaults.AuthenticationScheme)]
    public class BattleHub : Hub
    {
        private BattleHubContext BattleHubContext { get; set; }
        private HashSet<ChunkKey> Subscriptions { get; set; }
        
        public BattleHub(BattleHubContext battleHubContext)
        {
            this.BattleHubContext = battleHubContext;
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
        }

        public Task GetChunkState(ChunkKey key)
        {
            throw new NotImplementedException();
        }

        public Task<int> ProcessAction(ChunkKey key, ChunkAction action)
        {
            throw new NotImplementedException();
        }

        public Task SubscribeChunk(ChunkKey key)
        {
            throw new NotImplementedException();
        }

        public void UnsubscribeChunk(ChunkKey key)
        {
        }
    }
}