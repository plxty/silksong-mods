using System;
using BepInEx;
using BepInEx.Logging;
using double_rosaries;
using GlobalSettings;
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

        [HarmonyPatch(typeof(PlayerData), "AddGeo")]
        [HarmonyPrefix]
        private static void AddGeoPrefix(PlayerData __instance, ref int amount)
        {
            amount *= 2;
        }

        [HarmonyPatch(typeof(HeroController), "CocoonBroken", new[] { typeof(bool), typeof(bool) })]
        [HarmonyPrefix]
        private static void CocoonBrokenPrefix(ref bool doAirPause, ref bool forceCanBind, HeroController __instance)
        {
            // Prevent player from getting double rosaries when picking up their dead body
            __instance.playerData.HeroCorpseMoneyPool /= 2;
        }
    }
}
