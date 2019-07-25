using Microsoft.Extensions.DependencyInjection;
using PixelBattles.Hub.Server.Handlers.Battle;
using PixelBattles.Hub.Server.Handlers.Chunk;
using PixelBattles.Hub.Server.Handling.Main;
using System;
using System.Collections.Generic;
using System.Text;

namespace PixelBattles.Hub.Server.Handling
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHandling(this IServiceCollection services)
        {
            services.AddSingleton<IMainHandler, MainHandler>();
            services.AddSingleton<IBattleHandlerFactory, BattleHandlerFactory>();
            services.AddSingleton<IChunkHandlerFactory, ChunkHandlerFactory>();
            return services;
        }
    }
}
