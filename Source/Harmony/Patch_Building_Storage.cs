﻿using RimWorld;
using HarmonyLib;

namespace VarietyMattersStockpile
{
    [HarmonyPatch]
    public class Patch_Building_Storage
    {
        [HarmonyPatch(typeof(Building_Storage), "Notify_ReceivedThing")]
        [HarmonyPostfix]
        public static void Postfix_ReceivedThing(Building_Storage __instance)
        {
            //Log.Message("Stockpile received something");
            StorageLimits.CheckIfFull(__instance.GetSlotGroup());
        }
        
        [HarmonyPatch(typeof(Building_Storage), "Notify_LostThing")]
        [HarmonyPostfix]
        public static void Postfix_LostThing(Building_Storage __instance)
        {
            //Log.Message("Something removed from stockpile");
            SlotGroup slotGroup = __instance.GetSlotGroup();
            StorageLimits.CheckNeedsFilled(slotGroup, ref StorageLimits.GetLimitSettings(slotGroup.Settings).needsFilled, ref StorageLimits.GetLimitSettings(slotGroup.Settings).cellsFilled);
        }
    }
}
