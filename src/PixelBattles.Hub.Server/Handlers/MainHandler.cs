using PixelBattles.API.Client;
using PixelBattles.Chunkler.Client;
using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;

namespace PixelBattles.Hub.Server.Handlers
{
    public class MainHandler : IDisposable
    {
        private IServiceProvider _serviceProvider { get; set; }
        private IApiClient _apiClient { get; set; }

        private readonly ConcurrentDictionary<long, Lazy<BattleHandler>> _battleHandlers = new ConcurrentDictionary<long, Lazy<BattleHandler>>();

        public MainHandler(
            IApiClient apiClient,
            IServiceProvider serviceProvider)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException();
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException();
        }

        public Task<BattleHandler> GetOrCreateBattleHandlerAsync(long battleId)
        {
            return Task.FromResult(_battleHandlers.GetOrAdd(battleId, 
                key => new Lazy<BattleHandler>(() => CreateBattleHandlerAsync(key).Result, LazyThreadSafetyMode.ExecutionAndPublication)).Value);
        }

        private async Task<BattleHandler> CreateBattleHandlerAsync(long battleId)
        {
            var battleDto = await _apiClient.GetBattleAsync(battleId);
            var chunklerClient = _serviceProvider.GetRequiredService<IChunklerClient>();
            return new BattleHandler(battleDto.BattleId, chunklerClient);
        }

        public void Dispose()
        {
            //TODO
        }
    }
}