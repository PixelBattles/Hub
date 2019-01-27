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
            throw new NotImplementedException();
            /*
            foreach (var subscription in _subscriptions)
            {
                
                //await _battleHubContext.ChunklerClient.Unsubscribe(subscription);
            }*/
        }

        public Task<ChunkState> GetChunkState(ChunkKey key)
        {
            return (Context.Items["battleHandler"] as BattleHandler).GetOrCreateChunkHandler(key).GetStateAsync();
        }

        //public Task<int> ProcessAction(ChunkKey key, ChunkAction action)
        //{
        //    throw new NotImplementedException();
        //}

        public async Task SubscribeChunk(ChunkKey key)
        {
            throw new NotImplementedException();
            /*
            if (_subscriptions.Add(key))
            {
                
                //await _battleHubContext.ChunklerClient.SubscribeAsync(key, (update) => Clients.Caller.OnUpdate(key, update));
            }
            */
        }

        public void UnsubscribeChunk(ChunkKey key)
        {
            throw new NotImplementedException();
        }
    }
}