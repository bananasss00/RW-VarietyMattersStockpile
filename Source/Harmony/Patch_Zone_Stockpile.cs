using RimWorld;
using HarmonyLib;

namespace VarietyMattersStockpile
{
    [HarmonyPatch]
    public class Patch_Zone_Stockpile
    {
        [HarmonyPatch(typeof(Zone_Stockpile), "Notify_ReceivedThing")]
        [HarmonyPostfix]
        public static void Postfix_ReceivedThing(Zone_Stockpile __instance)
        {
            //Log.Message("Stockpile received something");
            StorageLimits.CheckIfFull(__instance.GetSlotGroup());
        }

        [HarmonyPatch(typeof(Zone_Stockpile), "RemoveCell")]
        [HarmonyPostfix]
        public static void Postfix_RemoveCell(Zone_Stockpile __instance)
        {
            //Log.Message("Stockpile is smaller, check if it still needs to be filled");
            StorageLimits.CheckIfFull(__instance.GetSlotGroup());
        }
        
        [HarmonyPatch(typeof(Zone_Stockpile), "Notify_LostThing")]
        [HarmonyPostfix]
        public static void Postfix_LostThing(Zone_Stockpile __instance)
        {
            //Log.Message("Something removed from stockpile");
            SlotGroup slotGroup = __instance.GetSlotGroup();
            StorageLimits.CheckNeedsFilled(slotGroup, ref StorageLimits.GetLimitSettings(slotGroup.Settings).needsFilled, ref StorageLimits.GetLimitSettings(slotGroup.Settings).cellsFilled);
        }

        [HarmonyPatch(typeof(Zone_Stockpile), "AddCell")]
        [HarmonyPostfix]
        public static void Postfix_AddCell(Zone_Stockpile __instance)
        {
            //Log.Message("Stockpile is larger, start filling");
            SlotGroup slotGroup = __instance.GetSlotGroup();
            if(slotGroup != null)
            {
                StorageLimits.GetLimitSettings(slotGroup.Settings).needsFilled = true;
            }
        }
    }
}
