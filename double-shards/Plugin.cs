using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace double_shards
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Hollow Knight Silksong.exe")]
    public class Plugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;

        private void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(Plugin));
            Logger = base.Logger;
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }

        [HarmonyPatch(typeof(PlayerData), "AddShards")]
        [HarmonyPrefix]
        private static void AddShardsPrefix(PlayerData __instance, ref int amount)
        {
            amount *= 2;
        }
    }
}
