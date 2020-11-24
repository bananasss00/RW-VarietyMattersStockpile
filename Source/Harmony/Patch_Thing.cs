using HarmonyLib;
using Verse;

namespace VarietyMattersStockpile
{
    [HarmonyPatch(typeof(Thing), nameof(Thing.TryAbsorbStack))]
    class Patch_Thing
    {
        public static bool Prefix(Thing __instance, Thing other, bool respectStackLimit, ref bool __result)
        {
            //Log.Message("Thing.TryAbsorbStack begin");
            int sizeLimit = StorageLimits.CalculateSizeLimit(__instance);
            if (sizeLimit < __instance.def.stackLimit)
            {
                if (__instance.CanStackWith(other))
                {
                    if (ThingUtility.TryAbsorbStackNumToTake(__instance, other, respectStackLimit) <= 0)
                    { 
                        __result = false; 
                        return false; 
                    }
                }
            }
            return true;
        }
    }
}
