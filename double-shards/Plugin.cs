using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace double_shards
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Hollow Knight Silksong.exe")]
    public class Plugin : BaseUnityPlugin
    {
        private static float _shardsMultiplier = 2;
        
        private void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(Plugin));
            _shardsMultiplier = Config.Bind("Cheats", "ShardsMultiplier", 2.0f, "The multiplier for collecting shards. Note that most fractional values won't work.").Value;
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }

        [HarmonyPatch(typeof(PlayerData), "AddShards")]
        [HarmonyPrefix]
        private static void AddShardsPrefix(PlayerData __instance, ref int amount)
        {
            amount = (int)Math.Round(amount* _shardsMultiplier);
        }
    }
}
