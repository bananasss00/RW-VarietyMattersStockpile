using Verse;

namespace VarietyMattersStockpile
{
    class ModSettings_VarietyStockpile : ModSettings
    {
        public static bool avoidOverfill = false;
        public static bool limitNonStackables = true;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref avoidOverfill, "avoidOverfill", false);
            Scribe_Values.Look(ref limitNonStackables, "limitNonStackables", true);
        }
    }
}
