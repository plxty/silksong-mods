using BepInEx;
using GlobalEnums;
using HarmonyLib;
using UnityEngine;

namespace double_shards
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Hollow Knight Silksong.exe")]
    public class Plugin : BaseUnityPlugin
    {
        private static int _shardsMultiplier;
        private static int _rosariesMultiplier;

        private void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(Plugin));
            _shardsMultiplier = Config.Bind("Cheats", "ShardsMultiplier", 3, "The multiplier for collecting shards.").Value;
            _rosariesMultiplier = Config.Bind("Cheats", "RosariesMultiplier", 2, "The multiplier for collecting rosaries.").Value;
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

            // To speed up reflection while gaming, accessing private field at the first time is too costy:
            var healthManagerTraverse = Traverse.Create(typeof(HealthManager));
            _ = healthManagerTraverse.Field<int>("smallGeoDrops");
            _ = healthManagerTraverse.Field<int>("mediumGeoDrops");
            _ = healthManagerTraverse.Field<int>("largeGeoDrops");
            _ = healthManagerTraverse.Field<int>("largeSmoothGeoDrops");
            _ = healthManagerTraverse.Field<int>("shellShardDrops");
            var currencyObjectBaseTraverse = Traverse.Create(typeof(CurrencyObjectBase));
            _ = currencyObjectBaseTraverse.Field<bool>("isAttracted");
        }

        private static bool _isMagnetEquipped = false;

        [HarmonyPatch(typeof(HealthManager), "Die", [typeof(float?), typeof(AttackTypes), typeof(NailElements), typeof(GameObject), typeof(bool), typeof(float), typeof(bool), typeof(bool)])]
        [HarmonyPrefix]
        private static void HealthManagerDiePrefix(HealthManager __instance, float? attackDirection, AttackTypes attackType, NailElements nailElement, GameObject damageSource, bool ignoreEvasion, float corpseFlingMultiplier, bool overrideSpecialDeath, bool disallowDropFling)
        {
            if (__instance.EnemyType != HealthManager.EnemyTypes.Regular)
                return;

            // Team cherry won't call Die twice on the same GameObject, do they?
            var traverse = Traverse.Create(__instance);
            var smallGeoDrops = traverse.Field<int>("smallGeoDrops"); // 5
            var mediumGeoDrops = traverse.Field<int>("mediumGeoDrops"); // 10
            var largeGeoDrops = traverse.Field<int>("largeGeoDrops"); // 15
            var largeSmoothGeoDrops = traverse.Field<int>("largeSmoothGeoDrops");
            var shellShardDrops = traverse.Field<int>("shellShardDrops");

            // More rosaries, for largeSmoothGeo, it's "bound" to largeGeo:
            int geoCount = smallGeoDrops.Value + mediumGeoDrops.Value + largeGeoDrops.Value + largeSmoothGeoDrops.Value;
            if (geoCount == 0)
            {
                geoCount = shellShardDrops.Value * _rosariesMultiplier;
                smallGeoDrops.Value = (geoCount % 15) % 10;
                mediumGeoDrops.Value = (geoCount % 15) / 10;
                largeGeoDrops.Value = geoCount / 15;
                Debug.Log($"Drops {geoCount} rosaries");
            }
            else
            {
                smallGeoDrops.Value *= _rosariesMultiplier;
                mediumGeoDrops.Value *= _rosariesMultiplier;
                largeGeoDrops.Value *= _rosariesMultiplier;
                largeSmoothGeoDrops.Value *= _rosariesMultiplier;
                Debug.Log("Rosaries and shards multiplied");
            }

            // More shard now:
            shellShardDrops.Value *= _shardsMultiplier;
        }

        [HarmonyPatch(typeof(HealthManager), "TrySteal")]
        [HarmonyPostfix]
        private static void HealthManagerTryStealPostfix(HealthManager __instance, ref int stolenGeo, ref int flingGeo, ref int stolenShards, ref int flingShards)
        {
            // Fling means the stole "lose chance", and the hero should pick up them by their selves.
            stolenGeo *= _shardsMultiplier;
            flingGeo *= _shardsMultiplier;
            stolenShards *= _shardsMultiplier;
            flingShards *= _shardsMultiplier;
            Debug.Log("Stealing geo and shards");
        }

        [HarmonyPatch(typeof(ToolItemManager), "IsToolEquipped", [typeof(string)])]
        [HarmonyPostfix]
        private static void ToolItemManagerIsToolEquippedPostfix(ref bool __result, string name)
        {
            // Match with the original function:
            if (CollectableItemManager.IsInHiddenMode())
                return;

            if (name == "Compass")
                __result = true;
            else if (name == "Rosary Magnet")
                _isMagnetEquipped = __result;
            if (__result)
                Debug.Log($"Equipment {name} selected");
        }

        [HarmonyPatch(typeof(CurrencyObjectBase), "Land")]
        [HarmonyPostfix]
        private static void CurrencyObjectBaseLandPostfix(CurrencyObjectBase __instance)
        {
            // TODO: Cleaner way to detect if magent enabled? For shard, it seems it's always off.
            if (!_isMagnetEquipped)
                return;
            Traverse.Create(__instance).Field<bool>("isAttracted").Value = true;
        }
    }
}
