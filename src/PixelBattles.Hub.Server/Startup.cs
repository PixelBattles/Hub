using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Orleans.Configuration;
using PixelBattles.API.Client;
using PixelBattles.Chunkler.Client;
using PixelBattles.Hub.Server.Handlers;
using PixelBattles.Hub.Server.Handlers.Battle;
using PixelBattles.Hub.Server.Handlers.Chunk;
using PixelBattles.Hub.Server.Handlers.Main;
using PixelBattles.Hub.Server.Hubs;
using System;
using System.Text;
using System.Threading.Tasks;

namespace PixelBattles.Hub.Server
{
    public class Startup
    {
        public IHostingEnvironment HostingEnvironment { get; }

        public IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder =   new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddJsonFile("appsettings.Custom.json", optional: true);

            builder.AddEnvironmentVariables();

            Configuration = builder.Build();

            HostingEnvironment = env;
        }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();
            //TODO test    
            //.AddMessagePackProtocol();

            services.AddOptions();

            services.AddAuthorization(options =>
            {
                options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy =>
                {
                    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireClaim("BattleId");
                    policy.RequireClaim("UserId");
                });
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        LifetimeValidator = (before, expires, token, parameters) => expires > DateTime.UtcNow,
                        ValidateAudience = false,
                        ValidateIssuer = false,
                        ValidateActor = false,
                        ValidateLifetime = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration.GetBattleTokenOptions().SecretKey))
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            // If the request is for our hub...
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) &&
                                (path.StartsWithSegments("/hubs/battles")))
                            {
                                // Read the token out of the query string
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            services.Configure<ClusterOptions>(Configuration.GetSection(nameof(ClusterOptions)));
            services.Configure<ApiClientOptions>(Configuration.GetSection(nameof(ApiClientOptions)));
            services.Configure<MainHandlerOptions>(Configuration.GetSection(nameof(MainHandlerOptions)));
            services.AddSingleton<IApiClient, ApiClient>();
            services.AddSingleton<IMainHandler, MainHandler>();
            services.AddSingleton<IBattleHandlerFactory, BattleHandlerFactory>();
            services.AddSingleton<IChunkHandlerFactory, ChunkHandlerFactory>();
            services.AddTransient<IChunklerClient, ChunklerClient>(serviceProvider => new ChunklerClient(
                new ChunklerClientOptions
                {
                    ClusterOptions = serviceProvider.GetService<IOptions<ClusterOptions>>().Value
                },
                serviceProvider.GetService<ILogger<ChunklerClient>>()));
        }
        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();

            app.UseSignalR(routes =>
            {
                routes.MapHub<BattleHub>("/hubs/battles");
            });
        }
    }
}
