using BetterCrafter.Managers;
using HarmonyLib;
using System.Collections.Generic;
using UltimateSurvival;
using UltimateSurvival.GUISystem;
using UnityEngine;

namespace BetterCrafter.Patches.Dynamic
{
    [HarmonyPatch(typeof(ItemContainer))]
    class ItemContainer_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ItemContainer), "TryAddItem", new System.Type[] { typeof(ItemData), typeof(int) })]
        public static bool TryAddItem_Prefix(ItemContainer __instance, bool __result, ItemData itemData, int amount)
        {
            if(CraftingManager.craftDependencies.Count > 0 && CraftingManager.craftDependencies[CraftingManager.craftDependencies.Count - 1] != 0)
            {
                return false;
            }

            if(__instance.Name == "Inventory" || __instance.Name == "Hotbar")
            {
                if (itemData != null && CraftingManager.lastCraftedItem.ContainsKey(itemData.Name))
                {
                    CraftingManager.lastCraftedItem[itemData.Name] += 1;
                }
                else if (itemData != null)
                {
                    CraftingManager.lastCraftedItem.Clear();
                    CraftingManager.lastCraftedItem.Add(itemData.Name, 1);
                }
            }

            Debug.Log(__instance.Name);

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ItemContainer), "RemoveItems", new System.Type[] { typeof(string), typeof(int) })]
        public static bool RemoveItems_Prefix(ItemContainer __instance, string itemName, int amount)
        {
            if (CraftingManager.craftCatchTakeItems)
            {
                List<CraftingManager.itemData> iD = CraftingManager.craftUsedItems;

                int removed;
                CollectionUtils.RemoveItems(itemName, amount, __instance.Slots, out removed);

                if(removed > 0)
                {
                    iD[0].Name.Add(itemName);
                    iD[0].Amount.Add(removed);
                }

                return false;
            }

            return true;
        }
    }
}
