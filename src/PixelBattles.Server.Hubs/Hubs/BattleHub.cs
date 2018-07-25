using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace PixelBattles.Server.Hubs
{
    [Authorize(JwtBearerDefaults.AuthenticationScheme)]
    public class BattleHub : Hub
    {
        private BattleHubContext BattleHubContext { get;set;}
        
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

        //public Task<GameInfoDTO> GetGameInfoAsync()
        //{
        //    return BattleHubContext.GetBattleAsync(GetBattleId());
        //}

        //public Task<bool> SubscribeChunkAsync(int x, int y)
        //{
        //    return Task.FromResult(false);
        //}

        //public void UnsubscribeChunk()
        //{
        //    throw new NotImplementedException();
        //}

        //public void GetChunkState()
        //{
        //    throw new NotImplementedException();
        //}

        //public void ProcessAction()
        //{
        //    throw new NotImplementedException();
        //}
    }
}
