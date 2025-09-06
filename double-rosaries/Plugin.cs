using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GlobalSettings;
using HarmonyLib;

namespace double_rosaries
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Hollow Knight Silksong.exe")]
    public class Plugin : BaseUnityPlugin
    {
        private static float _rosaryMultiplier = 2;
        
        private void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(Plugin));
            _rosaryMultiplier = Config.Bind("Cheats", "RosaryMultiplier", 2.0f, "The multiplier for collecting rosaries. Note that most fractional values won't work.").Value;
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }

        [HarmonyPatch(typeof(PlayerData), "AddGeo")]
        [HarmonyPrefix]
        private static void AddGeoPrefix(PlayerData __instance, ref int amount)
        {
            amount = (int)Math.Round(amount*_rosaryMultiplier);
        }

        [HarmonyPatch(typeof(HeroController), "CocoonBroken", new[] { typeof(bool), typeof(bool) })]
        [HarmonyPrefix]
        private static void CocoonBrokenPrefix(ref bool doAirPause, ref bool forceCanBind, HeroController __instance)
        {
            // Prevent player from getting double rosaries when picking up their dead body
            __instance.playerData.HeroCorpseMoneyPool = (int)Math.Round(__instance.playerData.HeroCorpseMoneyPool/_rosaryMultiplier);
        }

        [HarmonyPatch(typeof(ShopItem), "Cost", MethodType.Getter)]
        [HarmonyPostfix]
        private static void CostPostfix(ShopItem __instance, ref int __result)
        {
            // Prevent player from getting double rosaries when picking up their dead body
            string key = Traverse.Create(__instance).Field("displayName").Field("Key").GetValue() as string;
            if (key == "INV_NAME_COIN_SET_S")
            {
                __result = (int)Math.Round(__result * _rosaryMultiplier);
            }
        }
    }
}
