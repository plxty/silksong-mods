using BepInEx;
using BepInEx.Logging;
using GlobalSettings;
using HarmonyLib;
using System;
using static SilkSpool;

namespace double_silk
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

        [HarmonyPatch(typeof(HeroController), "AddSilk", new[] { typeof(int), typeof(bool), typeof(SilkSpool.SilkAddSource), typeof(bool) })]
        [HarmonyPrefix]
        private static void AddSilkPrefix(ref int amount, ref bool heroEffect, ref SilkSpool.SilkAddSource source, ref bool forceCanBindEffect)
        {
            amount *= 2;
        }
    }
}
