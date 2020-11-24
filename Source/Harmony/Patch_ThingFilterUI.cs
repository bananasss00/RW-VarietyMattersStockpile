using RimWorld;
using System.Reflection;
using UnityEngine;
using Verse;
using HarmonyLib;

namespace VarietyMattersStockpile
{
    [HarmonyPatch(typeof(ThingFilterUI), "DoThingFilterConfigWindow")]
    class Patch_ThingFilterUI
    {
        //Stack Limits
        static string dupBuffer = "";
        static string sizeBuffer = "";
        static StorageSettings oldSettings = null;
        private const int max = 9999999;

        public static void Prefix(ref Rect rect)
        {
            ITab_Storage tab = Patch_ITab_StorageFillTabs.currentTab;
            if (tab == null)
                return;

            IStoreSettingsParent storeSettingsParent = tab.SelStoreSettingsParent;
            StorageSettings settings = storeSettingsParent.GetStoreSettings();
            StorageLimits limitSettings = StorageLimits.GetLimitSettings(settings);

            rect.yMin += limitSettings.cellFillPercentage < 1f ? 70f : 52f;
            //rect.yMax += 100f;
        }

        public static void Postfix(ref Rect rect)
        {
            ITab_Storage tab = Patch_ITab_StorageFillTabs.currentTab;
            if (tab == null)
                return;

            IStoreSettingsParent storeSettingsParent = tab.SelStoreSettingsParent;
            StorageSettings settings = storeSettingsParent.GetStoreSettings();
            StorageLimits limitSettings = StorageLimits.GetLimitSettings(settings);

            var fontBkp = Text.Font;
            Text.Font = GameFont.Small;

            float w1 = 100f, w2 = 30f, h = 18f;
            float y1 = rect.yMin + (limitSettings.cellFillPercentage < 1f ? 30f : 48f) - 24f - 3f - 100f;

            //Duplicates
            int dupLimit = limitSettings.dupStackLimit;
            bool limitDuplicates = dupLimit != -1;
            Widgets.CheckboxLabeled(new Rect(rect.xMin, y1, w1, h), "Dup. stacks", ref limitDuplicates, false);
            if (limitDuplicates)
            {
                if (oldSettings != settings)
                    dupBuffer = dupLimit.ToString();

                Widgets.TextFieldNumeric(new Rect(rect.xMin + w1, y1, w2, h), ref dupLimit, ref dupBuffer, 1, max);
            }
            else
            {
                dupLimit = -1;
            }

            //Stack Limit
            int sizeLimit = limitSettings.stackSizeLimit;
            bool hasLimit = sizeLimit != -1;
            Widgets.CheckboxLabeled(new Rect(rect.xMin + (rect.width / 2), y1, w1, h), "Stack size", ref hasLimit, false);
            if (hasLimit)
            {
                if (oldSettings != settings)
                    sizeBuffer = sizeLimit.ToString();

                Widgets.TextFieldNumeric(new Rect(rect.xMin + (rect.width / 2) + w1, y1, w2, h), ref sizeLimit, ref sizeBuffer, 1, max);
            }
            else
            {
                sizeLimit = -1;
            }

            //Refill
            float cellFillPercentage = limitSettings.cellFillPercentage * 100;
            ISlotGroupParent slotGroupParent = settings.owner as ISlotGroupParent;
            int numCells = slotGroupParent.AllSlotCellsList().Count;
            int numCellsStart = Mathf.CeilToInt((100 - cellFillPercentage) / 100 * numCells);
            bool startFilling = limitSettings.needsFilled;
            string label;
            string llabel;
            switch (numCellsStart)
            {
                case 0:
                    label = "Always keep fully stocked";
                    break;
                case 1:
                    label = "Start refilling when 1 space is available";
                    break;
                default:
                    if (numCellsStart == numCells)
                    {
                        label = "Start refilling when empty"; 
                    }
                    else
                    {
                        label = "Start refilling when " + numCellsStart.ToString("N0") + " spaces are available";
                    }
                    break;
            }
            switch (startFilling)
            {
                case true:
                    llabel = "Refilling now, click to halt:";
                    break;
                case false:
                    llabel = "Click to start refilling:";
                    break;
                default:
                    llabel = "Exception: bool is null";
                    break;
            }

            float y2 = y1 + 10f + h;
            cellFillPercentage = Widgets.HorizontalSlider(new Rect(0f, y2, rect.width, 18f), cellFillPercentage, 0f, 100f, false, label);
            if (cellFillPercentage < 100 && limitSettings.cellsFilled != numCells)
            {
                float y3 = y2 + 18f;
                Widgets.CheckboxLabeled(new Rect(rect.xMin, y3, rect.width / 2 + 58f, h), llabel, ref startFilling, false);
            }
            //Update Settings
            oldSettings = settings;
            limitSettings.dupStackLimit = dupLimit;
            limitSettings.stackSizeLimit = sizeLimit;
            limitSettings.cellFillPercentage = cellFillPercentage / 100f;
            limitSettings.needsFilled = startFilling;
            StorageLimits.SetLimitSettings(settings, limitSettings);
            //Log.Message("Set stack size limit of " + limitSettings.stackSizeLimit);

            Text.Font = fontBkp;
        }
    }
    [HarmonyPatch(typeof(ITab_Storage), "FillTab")]
    class Patch_ITab_StorageFillTabs
    {
        public static ITab_Storage currentTab = null;

        public static void Prefix(ITab_Storage __instance)
        {
            currentTab = __instance;
        }

        public static void Postfix()
        {
            currentTab = null;
        }
    }
}
