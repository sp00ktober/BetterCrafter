using BetterCrafter.Managers;
using HarmonyLib;
using UltimateSurvival;
using UltimateSurvival.GUISystem;

namespace BetterCrafter.Patches.Dynamic
{
    [HarmonyPatch(typeof(ItemContainer))]
    class ItemContainer_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ItemContainer), "TryAddItem", new System.Type[] { typeof(ItemData), typeof(int) })]
        public static bool TryAddItem_Prefix(ItemData itemData, int amount)
        {
            if (CraftingManager.skipGiveItem > 0 && !CraftingManager.lockSkipGiveItem)
            {
                CraftingManager.skipGiveItem--;
                return false;
            }
            return true;
        }
    }
}
