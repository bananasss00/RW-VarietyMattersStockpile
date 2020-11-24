using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using HarmonyLib;
using UnityEngine;


namespace VarietyMattersStockpile
{
    [HarmonyPatch(typeof(MassUtility), "CountToPickUpUntilOverEncumbered")]
    internal class MassUtilityPatch
    {
        private static void Postfix(Pawn pawn, Thing thing, ref int __result)
        {
            int curSizeLimit = StorageLimits.CalculateSizeLimit(thing);
            if (thing.stackCount > curSizeLimit)
            {
                int t = thing.stackCount - curSizeLimit;
                if (t < 0)
                {
                    t = 0;
                }
                __result = Mathf.Min(t, __result);
            }
        }
    }

    [HarmonyPatch(typeof(HaulAIUtility), "HaulToCellStorageJob")]
    public static class Patch_Haul
    {
        // Pawns can carry multiple stacks but may overfil size limited stacks
        public static bool Prefix(ref Job __result, Pawn p, Thing t, IntVec3 storeCell, bool fitInStoreCell)
        {
            if (ModSettings_VarietyStockpile.avoidOverfill)
            {
                return true;
            }

            //Log.Message("Remove excess for size limited stack.");
            int curSizeLimit = StorageLimits.CalculateSizeLimit(t);
            if (t.stackCount > curSizeLimit)
            {
                __result = JobMaker.MakeJob(JobDefOf.HaulToCell, t, storeCell);
                __result.count = t.stackCount - curSizeLimit;
                __result.haulOpportunisticDuplicates = true;
                __result.haulMode = HaulMode.ToCellStorage;
                return false;
            }

            //Log.Message("Use vanilla code for stack without size limits.");
            SlotGroup slotGroup = p.Map.haulDestinationManager.SlotGroupAt(storeCell);
            int sizeLimit = StorageLimits.CalculateSizeLimit(slotGroup);
            if (sizeLimit >= t.def.stackLimit)
            {
                return true;
            }

            //Log.Message("Adjust result when filling a size limited stack.");
            Thing thing = p.Map.thingGrid.ThingAt(storeCell, t.def);
            if (thing != null)
            {
                __result = JobMaker.MakeJob(JobDefOf.HaulToCell, t, storeCell);
                __result.count = sizeLimit - thing.stackCount;
                __result.haulOpportunisticDuplicates = true;
                __result.haulMode = HaulMode.ToCellStorage;
                return false;
            }

            //Log.Message("Use vanilla code when filling empty slots in storage without limits on duplicate stacks.")
            int dupStackLimit = StorageLimits.GetLimitSettings(slotGroup.Settings).dupStackLimit;
            if (dupStackLimit == -1)
            {
                return true;
            }

            //Log.Message("Special rules when storage is limited by size and duplicates.");
            __result = JobMaker.MakeJob(JobDefOf.HaulToCell, t, storeCell);
            __result.haulOpportunisticDuplicates = true;
            __result.haulMode = HaulMode.ToCellStorage;
            int num = sizeLimit;
            float statValue = p.GetStatValue(StatDefOf.CarryingCapacity, true);
            List<IntVec3> cellsList = slotGroup.CellsList;
            for (int i = 0; i < cellsList.Count; i++)
            {   
                if (dupStackLimit == 1 || (float)num > statValue)
                {
                    __result.count = num;
                    return false;
                }
                thing = p.Map.thingGrid.ThingAt(cellsList[i], t.def);
                if (thing != null && thing != t && thing.CanStackWith(t))
                {
                    num += sizeLimit - thing.stackCount;
                    dupStackLimit -= 1;
                }
            }
            __result.count = dupStackLimit * sizeLimit;
            return false;
        }

        // Pawns will never will never overfill size limited stacks but can't haul multiple stacks.
        public static void Postfix(ref Job __result, Pawn p, Thing t, IntVec3 storeCell, bool fitInStoreCell)
        {
            if (!ModSettings_VarietyStockpile.avoidOverfill)
            {
                return;
            }

            int sizeLimit = StorageLimits.CalculateSizeLimit(t);
            if (t.stackCount > sizeLimit)
            {
                __result.count = t.stackCount - sizeLimit;
                return;
            }

            sizeLimit = StorageLimits.CalculateSizeLimit(p.Map.haulDestinationManager.SlotGroupAt(storeCell));
            if (sizeLimit >= t.def.stackLimit)
            {
                return;
            }

            __result.count = sizeLimit;
            Thing thing = p.Map.thingGrid.ThingAt(storeCell, t.def);
            if (thing != null)
            {
                __result.count -= thing.stackCount;
            }
        }
    }
}
