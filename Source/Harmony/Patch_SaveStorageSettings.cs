using RimWorld;
using Verse;
using HarmonyLib;

namespace VarietyMattersStockpile
{
    [HarmonyPatch(typeof(StorageSettings), nameof(StorageSettings.ExposeData))]
    public class StorageSettings_ExposeData
    {
        [HarmonyPostfix]
        public static void ExposeData(StorageSettings __instance)
        {
            StorageLimits storageLimits = StorageLimits.GetLimitSettings(__instance);
            Scribe_Deep.Look<StorageLimits>(ref storageLimits, "limitSettings", new object[0]);
            if (storageLimits != null)
            {
                StorageLimits.SetLimitSettings(__instance, storageLimits);
            }
        }
    }
}
