using HarmonyLib;
using RimWorld;
using Verse;

namespace VarietyMattersStockpile
{
    [HarmonyPatch]
    public class Patch_StoreUtility
    {
        [HarmonyPatch(typeof(StoreUtility), "NoStorageBlockersIn")]
        public static void Postfix(ref bool __result, IntVec3 c, Map map, Thing thing)
        {
            if (__result)
            {
                SlotGroup slotGroup = c.GetSlotGroup(map);
                if (slotGroup == null)
                {
                    return;
                }
                //Log.Message("Get Limit Settings");
                StorageLimits limitSettings = StorageLimits.GetLimitSettings(slotGroup.Settings);

                if (!limitSettings.needsFilled && limitSettings.cellFillPercentage < 1)
                {
                    __result = false;
                    return;
                }

                int sizeLimit = limitSettings.stackSizeLimit;
                if (sizeLimit > 0 && sizeLimit < thing.def.stackLimit)
                {
                    //Log.Message("Checking if stack size limit reached");
                    if (StackSizeReached(sizeLimit, c, map, thing))
                    {
                        __result = false; 
                        return;
                    }
                }
                if (limitSettings.dupStackLimit > 0)
                {
                    //Log.Message("Check if more stacks are allowed.")
                    if (!NewStackAllowed(slotGroup, c, map, thing))
                    {
                        __result = false;
                        return;
                    }
                }
                /*
                if (!limitSettings.needsFilled)
                {   
                     Failed attempt at LWD Deep Storage Work Around
                    if (limitSettings.extraTrips > 0)
                    {
                        StorageLimits.GetLimitSettings(slotGroup.Settings).extraTrips -= 1;
                    }

                    //Log.Message("Stock levels are satisfactory.");
                    else if (limitSettings.cellFillPercentage < 1)
                    {
                        __result = false;
                        return; 
                    }    
                }
                */
            }
            return;
        }

        [HarmonyPatch(typeof(StoreUtility), "TryFindBestBetterStoreCellFor")]
        public static void Prefix(Thing t, ref StoragePriority currentPriority)
        {
            if (t.stackCount > StorageLimits.CalculateSizeLimit(t))
            {
                currentPriority = StoragePriority.Low; // StoragePriority.Unstored?
            }
        }

        public static bool StackSizeReached(int sizeLimit, IntVec3 c, Map map, Thing thing)
        {
            //Log.Message("Stack size limit is " + sizeLimit);
            Thing thing2 = map.thingGrid.ThingAt(c, thing.def);
            {
                if (thing2 != null && thing2.CanStackWith(thing))
                {
                    if (thing2.stackCount >= sizeLimit)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool NewStackAllowed(SlotGroup slotGroup, IntVec3 c, Map map, Thing t)
        {
            // Log.Message("Checking for duplicates");
            int dupLimit = StorageLimits.GetLimitSettings(slotGroup.Settings).dupStackLimit;
            int numDuplicates = 0;
            foreach (IntVec3 cell in slotGroup.CellsList)
            {
                if (cell != c)
                {
                    Thing thing2 = map.thingGrid.ThingAt(cell, t.def);
                    if (thing2 != null)
                    {
                        if (thing2.CanStackWith(t) ||
                           (ModSettings_VarietyStockpile.limitNonStackables && t.def.stackLimit == 1 && t.def.defName == thing2.def.defName))
                        {
                            numDuplicates++;
                            if (numDuplicates == dupLimit)
                                return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}