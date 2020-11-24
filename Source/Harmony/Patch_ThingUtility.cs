using HarmonyLib;
using UnityEngine;
using Verse;

namespace VarietyMattersStockpile
{
    //[HarmonyPatch(typeof(ThingUtility), "TryAbsorbStackNumToTake")]
    public static class Patch_ThingUtility
    {
        [HarmonyPriority(Priority.Low)]
        public static void Postfix(ref int __result, Thing thing, Thing other, bool respectStackLimit)
        {
            if (respectStackLimit)
            {
                var t = StorageLimits.CalculateSizeLimit(thing) - thing.stackCount;
                if (t < 0) t = 0;
                __result = Mathf.Min(other.stackCount, t);
            }
        }
    }
}
