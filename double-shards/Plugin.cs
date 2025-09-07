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
        private static int _damageMultiplier;

        private void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(Plugin));
            _shardsMultiplier = Config.Bind("Cheats", "ShardsMultiplier", 3, "The multiplier for collecting shards.").Value;
            _damageMultiplier = Config.Bind("Cheats", "DamageMultiplier", 2, "The multiplier for damage.").Value;
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

            // To speed up reflection while gaming, accessing private field at the first time is too costy:
            var traverse = Traverse.Create(new HealthManager());
            _ = traverse.Field<int>("smallGeoDrops");
            _ = traverse.Field<int>("mediumGeoDrops");
            _ = traverse.Field<int>("largeGeoDrops");
            _ = traverse.Field<int>("largeSmoothGeoDrops");
            _ = traverse.Field<int>("shellShardDrops");
        }

        [HarmonyPatch(typeof(HealthManager), "Die", [typeof(float?), typeof(AttackTypes), typeof(NailElements), typeof(GameObject), typeof(bool), typeof(float), typeof(bool), typeof(bool)])]
        [HarmonyPrefix]
        private static void HealthManagerDiePrefix(ref HealthManager __instance, float? attackDirection, AttackTypes attackType, NailElements nailElement, GameObject damageSource, bool ignoreEvasion, float corpseFlingMultiplier, bool overrideSpecialDeath, bool disallowDropFling)
        {
            if (__instance.EnemyType != HealthManager.EnemyTypes.Regular)
            {
                return;
            }

            // Team cherry won't call Die twice on the same GameObject, do they?
            var traverse = Traverse.Create(__instance);
            var smallGeoDrops = traverse.Field<int>("smallGeoDrops"); // 5
            var mediumGeoDrops = traverse.Field<int>("mediumGeoDrops"); // 10
            var largeGeoDrops = traverse.Field<int>("largeGeoDrops"); // 15
            var largeSmoothGeoDrops = traverse.Field<int>("largeSmoothGeoDrops");
            var shellShardDrops = traverse.Field<int>("shellShardDrops");

            // More shard now:
            shellShardDrops.Value *= _shardsMultiplier;

            // More rosaries, for largeSmoothGeo, it's "bound" to largeGeo:
            int geoCount = smallGeoDrops.Value + mediumGeoDrops.Value + largeGeoDrops.Value + largeSmoothGeoDrops.Value;
            if (geoCount == 0)
            {
                geoCount = shellShardDrops.Value;
                smallGeoDrops.Value = (geoCount % 15) % 10;
                mediumGeoDrops.Value = (geoCount % 15) / 10;
                largeGeoDrops.Value = geoCount / 15;
                Debug.Log($"Drops {geoCount} rosaries");
            }
            else
            {
                smallGeoDrops.Value *= _shardsMultiplier;
                mediumGeoDrops.Value *= _shardsMultiplier;
                largeGeoDrops.Value *= _shardsMultiplier;
                largeSmoothGeoDrops.Value *= _shardsMultiplier;
                Debug.Log("Rosaries and shards multiplied");
            }
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

        [HarmonyPatch(typeof(HeroController), "TakeDamage")]
        [HarmonyPrefix]
        private static void HeroControllerTakeDamage(HeroController __instance, GameObject go, CollisionSide damageSide, int damageAmount, ref HazardType hazardType, DamagePropertyFlags damagePropertyFlags)
        {
            // From enemy, check TakeDamageFromDamager (CheckForDamage).
            if (hazardType == HazardType.LAVA || hazardType == HazardType.STEAM)
            {
                Debug.Log($"Reducing hazard {hazardType} to 1 damage");
                hazardType = HazardType.SPIKES;
            }
        }

        [HarmonyPatch(typeof(HealthManager), "TakeDamage")]
        [HarmonyPrefix]
        private static void HealthManagerTakeDamage(HealthManager __instance, ref HitInstance hitInstance)
        {
            hitInstance.DamageDealt *= _damageMultiplier;
            Debug.Log($"Hit {hitInstance.DamageDealt} damage");
        }

        [HarmonyPatch(typeof(ToolItemManager), "IsToolEquipped", [typeof(string)])]
        [HarmonyPostfix]
        private static void ToolItemManagerIsToolEquippedPostfix(ref bool __result, string name)
        {
            // Match with the original function:
            if (CollectableItemManager.IsInHiddenMode())
            {
                return;
            }

            if (name == "Compass")
            {
                __result = true;
                Debug.Log($"Equipment {name} selected");
            }
        }
    }
}
