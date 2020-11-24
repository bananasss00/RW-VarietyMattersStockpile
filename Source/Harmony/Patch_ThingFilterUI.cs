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

            rect.yMin += 100f;
            //rect.yMax += 100f;
        }

        public static void Postfix(ref Rect rect)
        {
            ITab_Storage tab = Patch_ITab_StorageFillTabs.currentTab;
            if (tab == null)
                return;

            IStoreSettingsParent storeSettingsParent = (IStoreSettingsParent)typeof(ITab_Storage).GetProperty("SelStoreSettingsParent", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true).Invoke(tab, new object[0]);
            StorageSettings settings = storeSettingsParent.GetStoreSettings();
            StorageLimits limitSettings = StorageLimits.GetLimitSettings(settings);

            //Duplicates
            int dupLimit = limitSettings.dupStackLimit;
            bool limitDuplicates = dupLimit != -1;
            Widgets.CheckboxLabeled(new Rect(rect.xMin, rect.yMin - 24f - 3f - 100f, rect.width / 2 + 58f, 24f), "Limit duplicate stacks", ref limitDuplicates, false);
            if (limitDuplicates)
            {
                if (oldSettings != settings)
                    dupBuffer = dupLimit.ToString();

                Widgets.TextFieldNumeric<int>(new Rect(rect.xMin + (rect.width / 2) + 60f, rect.yMin - 24f - 3f - 100f, rect.width / 2 - 60f, 24f), ref dupLimit, ref dupBuffer, 1, max);
            }
            else
            {
                dupLimit = -1;
            }

            //Stack Limit
            int sizeLimit = limitSettings.stackSizeLimit;
            bool hasLimit = sizeLimit != -1;
            Widgets.CheckboxLabeled(new Rect(rect.xMin, rect.yMin - 24f - 3f - 76f, rect.width / 2 + 58f, 24f), "Limit stack size", ref hasLimit, false);
            if (hasLimit)
            {
                if (oldSettings != settings)
                    sizeBuffer = sizeLimit.ToString();

                Widgets.TextFieldNumeric<int>(new Rect(rect.xMin + (rect.width / 2) + 60f, rect.yMin - 24f - 3f - 76f, rect.width / 2 - 60f, 24f), ref sizeLimit, ref sizeBuffer, 1, max);
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
            cellFillPercentage = Widgets.HorizontalSlider(new Rect(0f, rect.yMin - 24f - 3f - 26f, rect.width, 36f), cellFillPercentage, 0f, 100f, false, label);
            if (cellFillPercentage < 100 && limitSettings.cellsFilled != numCells)
            {
                Widgets.CheckboxLabeled(new Rect(rect.xMin, rect.yMin - 24f - 3f - 52f, rect.width / 2 + 58f, 24f), llabel, ref startFilling, false);
            }
            //Update Settings
            oldSettings = settings;
            limitSettings.dupStackLimit = dupLimit;
            limitSettings.stackSizeLimit = sizeLimit;
            limitSettings.cellFillPercentage = cellFillPercentage / 100f;
            limitSettings.needsFilled = startFilling;
            StorageLimits.SetLimitSettings(settings, limitSettings);
            //Log.Message("Set stack size limit of " + limitSettings.stackSizeLimit);
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
