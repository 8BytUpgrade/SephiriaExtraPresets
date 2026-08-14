using System.Collections.Generic;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using BepInEx.Logging;

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

[HarmonyPatch(typeof(UI_PresetPanel), "RebuildPresetSlotButtons")]
internal static class RebuildPresetSlotButtonsPatch
{
    [HarmonyPrefix]
    private static void Prefix(UI_PresetPanel __instance)
    {
        Plugin.Log.LogInfo("RebuildPresetSlotButtons is starting");
        
    }
}