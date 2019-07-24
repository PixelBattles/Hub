using Microsoft.Extensions.DependencyInjection;
using PixelBattles.API.Client;
using PixelBattles.Chunkler.Client;
using PixelBattles.Hub.Server.Handlers.Chunk;
using System;
using System.Threading.Tasks;

namespace PixelBattles.Hub.Server.Handlers.Battle
{
    public class BattleHandlerFactory : IBattleHandlerFactory
    {
        private IServiceProvider _serviceProvider;
        private IApiClient _apiClient;

        public BattleHandlerFactory(IServiceProvider serviceProvider, IApiClient apiClient)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        }

        public async Task<IBattleHandler> CreateBattleHandlerAsync(long battleId)
        {
            try
            {
                var battleDto = await _apiClient.GetBattleAsync(battleId);
            }
            catch (Exception)
            {
                return null;
            }

            var chunklerClient = _serviceProvider.GetRequiredService<IChunklerClient>();
            var chunkHandlerFactory = _serviceProvider.GetRequiredService<IChunkHandlerFactory>();
            return new BattleHandler(battleId, chunklerClient, chunkHandlerFactory);
        }
    }
}