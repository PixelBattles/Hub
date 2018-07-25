using Microsoft.Extensions.Configuration;
using System;

namespace PixelBattles.Server.Hubs
{
    public static class BattleTokenExtensions
    {
        private static IConfigurationSection GetBattleTokenSection(this IConfiguration configuration)
        {
            return configuration
                .GetSection(nameof(BattleTokenOptions));
        }

        public static BattleTokenOptions GetBattleTokenOptions(this IConfiguration configuration)
        {
            return configuration
                .GetBattleTokenSection().Get<BattleTokenOptions>();
        }
    }

    public class BattleTokenConstants
    {
        public const string BattleIdClaim = "BattleId";
        public const string UserIdClaim = "UserId";
    }

    public class BattleTokenOptions
    {
        public BattleTokenOptions()
        {
            //Default values
            SecretKey = "This is default key for dev purposes";
            DefaultIssuer = "PixelBattlesServer";
            DefaultAudience = "PixelBattlesClient";
            DefaultTimeLife = TimeSpan.FromDays(1);
        }

        public string SecretKey { get; set; }

        public string DefaultIssuer { get; set; }

        public string DefaultAudience { get; set; }

        public TimeSpan DefaultTimeLife { get; set; }
    }
}
