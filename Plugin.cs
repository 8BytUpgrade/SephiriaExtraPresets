using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine.UI;

namespace SephiriaPresets;

[BepInPlugin("com.byt.ExtraPresets", "ExtraPresets", "1.0.0")]
[BepInProcess("Sephiria.exe")]
public class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log;

    public void Awake()
    {
        // Plugin startup logic
        Log = Logger;
        var harmony = new Harmony("com.byt.ExtraPresets");
        harmony.PatchAll();
        Logger.LogInfo("Plugin ExtraPresets is loaded!");
    }
}