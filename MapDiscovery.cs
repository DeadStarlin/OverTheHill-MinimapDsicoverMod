using System;
using System.Collections.Generic;
using HarmonyLib;
using MelonLoader;

[assembly: MelonInfo(typeof(OverTheHill.MapDiscovery), "Map Discovery", "1.0.1", "DeadStarlin")]
[assembly: MelonGame("Funselektor", "over the hill")]

namespace OverTheHill;

public sealed class MapDiscovery : MelonMod
{
    private static MelonPreferences_Entry<DiscoveryArea> area;
    private static readonly Dictionary<IntPtr, (Il2Cpp.FogOfWarConfig Config, float Inner, float Outer)> configs = new();

    public override void OnInitializeMelon()
    {
        var category = MelonPreferences.CreateCategory("OTHMapDiscovery", "Map Discovery");
        area = category.CreateEntry("AreaMultiplier", DiscoveryArea.x4, "Discovery area multiplier",
            "x1 (vanilla), x2, x4, x10, x100. Multiplies area; radius scales by the square root.");
        area.OnEntryValueChanged.Subscribe((previous, current) => Apply());
        category.SaveToFile(false);
        LoggerInstance.Msg($"Discovery area: {area.Value}; author: DeadStarlin.");
    }

    internal static void Track(Il2Cpp.FogOfWarConfig config)
    {
        if (config == null) return;
        if (!configs.ContainsKey(config.Pointer))
            configs.Add(config.Pointer, (config, config.playerInnerRadius, config.playerOuterRadius));
        Apply();
    }

    private static void Apply(float? radius = null)
    {
        float scale = radius ?? AreaScale.Radius(area?.Value ?? DiscoveryArea.x1);
        foreach (var original in configs.Values)
        {
            if (original.Config == null) continue;
            original.Config.playerInnerRadius = original.Inner * scale;
            original.Config.playerOuterRadius = original.Outer * scale;
        }
    }

    public override void OnDeinitializeMelon()
    {
        Apply(1f);
        configs.Clear();
    }
}

// The native movement coroutine reads these fields directly for both fog and POI discovery.
[HarmonyPatch(typeof(Il2Cpp.FogOfWarManager), nameof(Il2Cpp.FogOfWarManager.InitializeFogOfWarTexture))]
internal static class PlayerDiscoveryRadius
{
    private static void Prefix(Il2Cpp.FogOfWarManager __instance) => MapDiscovery.Track(__instance.fogOfWarConfig);
}
