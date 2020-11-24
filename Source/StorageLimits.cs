using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VarietyMattersStockpile
{
    public class StorageLimits : IExposable
    {
        private static Dictionary<StorageSettings, StorageLimits> limitSettings = new Dictionary<StorageSettings, StorageLimits>();
        public int dupStackLimit = -1;
        public int stackSizeLimit = -1;
        public float cellFillPercentage = 1f;
        public bool needsFilled = true;
        public float cellsFilled = -1f;

        //public static  bool deepStorageActive = false;
        //public int extraTrips = 0;

        public static StorageLimits GetLimitSettings(StorageSettings settings)
        {
            if (limitSettings.ContainsKey(settings))
            {
                return limitSettings[settings];
            }
            return new StorageLimits();
        }

        public static void SetLimitSettings(StorageSettings settings, StorageLimits newSettings)
        {
            if (limitSettings.ContainsKey(settings))
            {
                limitSettings[settings] = newSettings;
            }
            else
            {
                limitSettings.Add(settings, newSettings);
            }
        }

        public static int CalculateSizeLimit(Thing t)
        {
            if (!t.Spawned) return t.def.stackLimit;
            var slotgroup = t.Map.haulDestinationManager.SlotGroupAt(t.Position);//t.GetSlotGroup();
            if (slotgroup == null) return t.def.stackLimit;
            var limit = GetLimitSettings(slotgroup.Settings).stackSizeLimit;
            return (limit > 0 && limit < t.def.stackLimit) ? limit : t.def.stackLimit;
        }

        public static int CalculateSizeLimit(SlotGroup slotGroup)
        {
            if (slotGroup == null) return 99999;
            var limit = GetLimitSettings(slotGroup.Settings).stackSizeLimit;
            return limit > 0 ? limit : 99999;
        }

        public static void CheckIfFull(SlotGroup slotGroup)
        {
            bool needsFilled = GetLimitSettings(slotGroup.Settings).needsFilled;
            if (needsFilled)
            {
                needsFilled = false;
                List<IntVec3> cellsList = slotGroup.CellsList;
                foreach (IntVec3 cell in cellsList)
                {
                    if (!slotGroup.parent.Map.thingGrid.ThingsListAt(cell).Any(t => t.def.EverStorable(false)))
                    {
                        needsFilled = true; 
                        break;
                    }
                }
                if (!needsFilled)
                {
                    //Log.Message("Stockpile full, stop stocking");
                    GetLimitSettings(slotGroup.Settings).needsFilled = needsFilled;
                    GetLimitSettings(slotGroup.Settings).cellsFilled = cellsList.Count;
                    return;
                }
            }
        }

        public static void CheckNeedsFilled(SlotGroup slotGroup, ref bool needsFilled, ref float cellsFilled)
        {
            if (!needsFilled)
            {
                cellsFilled -= 1;
                float totalCells = slotGroup.CellsList.Count;
                if (cellsFilled <= 0 || GetLimitSettings(slotGroup.Settings).cellFillPercentage >= cellsFilled / totalCells)
                {
                    //Log.Message("Low stock, time to refill");
                    needsFilled = true;
                }
            }
        }

        public void ExposeData()
        {
            Scribe_Values.Look<int>(ref this.dupStackLimit, "duplicatesLimit", -1, false);
            Scribe_Values.Look<int>(ref this.stackSizeLimit, "stackSizeLimit", -1, false);
            Scribe_Values.Look<float>(ref this.cellFillPercentage, "cellfillPercentage", 1f, false);
            Scribe_Values.Look<bool>(ref this.needsFilled, "needsFilled", true, false);
            Scribe_Values.Look<float>(ref this.cellsFilled, "cellsFilled", -1f, false);
            //Scribe_Values.Look<int>(ref this.extraTrips, "extraStacks", 0, false);
        }
    }
}

/* Failed attempt at LWM Deep Storage Work Around
 *         public static void CheckIfFull(SlotGroup slotGroup)
        {
            bool needsFilled = GetLimitSettings(slotGroup.Settings).needsFilled;
            if (needsFilled)
            {
                needsFilled = false;
                List<IntVec3> cellsList = slotGroup.CellsList;
                int numCells = cellsList.Count;
                int curStacks = 0;
                foreach (IntVec3 cell in cellsList)
                {
                    if (!slotGroup.parent.Map.thingGrid.ThingsListAt(cell).Any(t => t.def.EverStorable(false)))
                    {
                        needsFilled = true; 
                        break;
                    }
                    if (slotGroup.parent is ThingWithComps)
                    {
                        //Log.Message("I have a comp");
                        curStacks += slotGroup.parent.Map.thingGrid.ThingsListAt(cell).Count;
                    }
                }
                if (!needsFilled)
                {
                    if (slotGroup.parent is ThingWithComps && numCells > 1 && curStacks > numCells)
                    {
                        int totStacks = (curStacks - 1) / (numCells - 1) * numCells; //Calculate total capacity
                        Log.Message("Currently holding " + curStacks.ToString() + " stacks");
                        Log.Message("Estimated size of " + totStacks.ToString() + " stacks");
                        if (curStacks < totStacks)
                        {
                            GetLimitSettings(slotGroup.Settings).extraTrips = totStacks - curStacks;
                            Log.Message("I set " + (totStacks - curStacks).ToString() + " extra trips");
                        }
                    }
                    //Log.Message("Stockpile full, stop stocking");
                    GetLimitSettings(slotGroup.Settings).needsFilled = needsFilled;
                    GetLimitSettings(slotGroup.Settings).cellsFilled = numCells;
                    return;
                }
            }
        }
*/