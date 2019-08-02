using Microsoft.Extensions.DependencyInjection;
using PixelBattles.API.Client;
using PixelBattles.Chunkler.Client;
using PixelBattles.Hub.Server.Handlers.Chunk;
using PixelBattles.Hub.Server.Handling.Battle;
using PixelBattles.Hub.Server.Handling.Chunk;
using System;
using System.Threading.Tasks;

namespace PixelBattles.Hub.Server.Handlers.Battle
{
    internal class BattleHandlerFactory : IBattleHandlerFactory
    {
        private IServiceProvider _serviceProvider;
        private IApiClient _apiClient;

        public BattleHandlerFactory(IServiceProvider serviceProvider, IApiClient apiClient)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        }

        public async Task<BattleHandler> CreateBattleHandlerAsync(long battleId)
        {
            BattleSettings battleSettings;
            try
            {
                var battleDto = await _apiClient.GetBattleAsync(battleId);
                battleSettings = new BattleSettings
                {
                    ChunkSettings = new ChunkSettings
                    {
                        ChunkHeight = battleDto.Settings.ChunkHeight,
                        ChunkWidth = battleDto.Settings.ChunkWidth
                    },
                    Cooldown = battleDto.Settings.Cooldown,
                    MaxHeightIndex = battleDto.Settings.MaxHeightIndex,
                    MinHeightIndex = battleDto.Settings.MinHeightIndex,
                    MaxWidthIndex = battleDto.Settings.MaxWidthIndex,
                    MinWidthIndex = battleDto.Settings.MinWidthIndex,
                    StartDateUTC = battleDto.StartDateUTC,
                    EndDateUTC = battleDto.EndDateUTC
                };
            }
            catch (Exception)
            {
                return null;
            }

            var chunklerClient = _serviceProvider.GetRequiredService<IChunklerClient>();
            var chunkHandlerFactory = _serviceProvider.GetRequiredService<IChunkHandlerFactory>();
            return new BattleHandler(battleId, battleSettings, chunklerClient, chunkHandlerFactory);
        }
    }
}