using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace ScannableFireExit;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("MaxWasUnavailable.LethalModDataLib")]
public class ScannableFireExit : BaseUnityPlugin {
    internal static ConfigEntry<bool> onlySeenFireExits = null!;
    public static ScannableFireExit Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger { get; private set; } = null!;
    internal static Harmony? Harmony { get; set; }

    private void Awake() {
        Logger = base.Logger;
        Instance = this;

        Patch();

        onlySeenFireExits = Config.Bind("General", "1. Only seen fire exits", false,
                                        "If true, will only allow you to scan fire exits you've used");

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
    }

    internal static void Patch() {
        Harmony ??= new(MyPluginInfo.PLUGIN_GUID);

        Logger.LogDebug("Patching...");

        Harmony.PatchAll();

        Logger.LogDebug("Finished patching!");
    }

    internal static void Unpatch() {
        Logger.LogDebug("Unpatching...");

        Harmony?.UnpatchSelf();

        Logger.LogDebug("Finished unpatching!");
    }
}