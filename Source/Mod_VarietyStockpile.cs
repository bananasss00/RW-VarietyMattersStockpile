using System.Reflection;
using Verse;
using UnityEngine;
using HarmonyLib;

namespace VarietyMattersStockpile
{
    class Mod_VarietyStockpile : Mod
    {
        public Mod_VarietyStockpile(ModContentPack content) : base(content)
        {
            Log.Message("Stockpiles patched for variety");
            Harmony harmony = new Harmony("varietymattersstockpile");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            GetSettings<ModSettings_VarietyStockpile>();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Rect rect = new Rect(260f, 50f, inRect.width * .4f, inRect.height);
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(rect);
            listingStandard.CheckboxLabeled("Avoid overfill: ", ref ModSettings_VarietyStockpile.avoidOverfill);
            listingStandard.CheckboxLabeled("Single-stack items can be duplicates: ", ref ModSettings_VarietyStockpile.limitNonStackables);
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "VarietyMattersStockpile".Translate();
        }
    }
}
