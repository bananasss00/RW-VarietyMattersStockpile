using RimWorld;
using HarmonyLib;

namespace VarietyMattersStockpile
{
    [HarmonyPatch(typeof(StorageSettingsClipboard), "Copy")]
    public static class CopyPatch
    {
        public static void Postfix(StorageSettings s, StorageSettings ___clipboard)
        {
            StorageLimits.SetLimitSettings(___clipboard, StorageLimits.GetLimitSettings(s));
        }
    }
    [HarmonyPatch(typeof(StorageSettingsClipboard), "PasteInto")]
    public static class PasteIntoPatch
    {
        public static void Postfix(StorageSettings s, StorageSettings ___clipboard)
        {
            StorageLimits copySettings = StorageLimits.GetLimitSettings(___clipboard);
            StorageLimits.GetLimitSettings(s).dupStackLimit = copySettings.dupStackLimit;
            StorageLimits.GetLimitSettings(s).stackSizeLimit = copySettings.stackSizeLimit;
            StorageLimits.GetLimitSettings(s).cellFillPercentage = copySettings.cellFillPercentage;
        }
    }
}
