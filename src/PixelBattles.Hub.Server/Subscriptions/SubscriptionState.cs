using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PixelBattles.Hub.Server.Subscriptions
{
    public enum SubscriptionState
    {
        Used = 0,
        NotUsed = 1,
        Disabling = 2,
    }
}