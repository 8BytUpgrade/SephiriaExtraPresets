using HarmonyLib;

namespace SephiriaPresets;

[HarmonyPatch(typeof(UI_PresetPanel), "GetSlotLimitCount")]
internal static class PresetSlotPatch
{
    [HarmonyPrefix]
    private static bool Prefix(ref int __result)
    // Add button limit
    {
        __result = 50;
        Plugin.Log.LogInfo($"Preset limit is: {__result}");
        return false;
    }
}