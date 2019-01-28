using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PixelBattles.Hub.Server.Handlers;
using System;
using System.Threading.Tasks;

namespace PixelBattles.Hub.Server.Hubs
{
    [Authorize(JwtBearerDefaults.AuthenticationScheme)]
    public class BattleHub : Hub<IBattleHubClient>
    {
        private readonly MainHandler _battleHandlerManager;
        private BattleHandler _battleHandler;

        public BattleHub(MainHandler battleHandlerManager)
        {
            _battleHandlerManager = battleHandlerManager ?? throw new ArgumentNullException();
        }

        private Guid GetBattleId()
        {
            return Guid.Parse(Context.User.FindFirst(BattleTokenConstants.BattleIdClaim).Value);
        }

        public async override Task OnConnectedAsync()
        {
            Context.Items["battleHandler"] = await _battleHandlerManager.GetOrCreateBattleHandlerAsync(GetBattleId());
            await base.OnConnectedAsync();
        }

        public async override Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }

        public Task<ChunkState> GetState(ChunkKey key)
        {
            return (Context.Items["battleHandler"] as BattleHandler).GetOrCreateChunkHandler(key).GetStateAsync();
        }

        public Task<ChunkState> SubscribeAndGetState(ChunkKey key)
        {
            throw new NotImplementedException();
        }

        public Task<int> Process(ChunkKey key, Chunkler.ChunkAction action)
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe(ChunkKey key)
        {
            throw new NotImplementedException();
        }
    }
}