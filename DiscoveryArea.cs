using System;

namespace OverTheHill;

public enum DiscoveryArea { x1 = 1, x2 = 2, x4 = 4, x10 = 10, x100 = 100 }

internal static class AreaScale
{
    internal static float Radius(DiscoveryArea area) => area switch
    {
        DiscoveryArea.x1 or DiscoveryArea.x2 or DiscoveryArea.x4 or DiscoveryArea.x10 or DiscoveryArea.x100
            => MathF.Sqrt((int)area),
        _ => 1f
    };
}
