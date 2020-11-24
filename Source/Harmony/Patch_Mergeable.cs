using RimWorld;
using Verse;
using HarmonyLib;

namespace VarietyMattersStockpile
{
    [HarmonyPatch(typeof(ListerMergeables), "ShouldBeMergeable")]
    public class Patch_Mergeable
    {
        static bool Postfix(bool __result, Thing t)
        {
            return __result && t.stackCount != StorageLimits.CalculateSizeLimit(t);
        }
    }
}

