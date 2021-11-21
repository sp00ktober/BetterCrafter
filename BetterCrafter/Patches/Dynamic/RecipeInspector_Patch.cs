using HarmonyLib;
using UltimateSurvival;
using UltimateSurvival.GUISystem;
using UnityEngine.UI;
using System.Collections.Generic;
using BetterCrafter.Managers;
using UnityEngine;

namespace BetterCrafter.Patches.Dynamic
{
    [HarmonyPatch(typeof(RecipeInspector))]
    public class RecipeInspector_Patch
    {
        // always enable craft button, needs finer tuning later
        [HarmonyPostfix]
        [HarmonyPatch(typeof(RecipeInspector), "ShowRecipeInfo")]
        public static void ShowRecipeInfo_Postfix(RecipeInspector __instance, ItemData itemData)
        {
            int amount = (int)AccessTools.Field(typeof(RecipeInspector), "m_CurrentDesiredAmount").GetValue(__instance);
            ItemContainer inventory = (ItemContainer)AccessTools.Field(typeof(RecipeInspector), "m_Inventory").GetValue(__instance);
            ItemContainer hotbar = (ItemContainer)AccessTools.Field(typeof(RecipeInspector), "m_Hotbar").GetValue(__instance);

            ((Button)AccessTools.Field(typeof(RecipeInspector), "craftButton").GetValue(__instance)).interactable = deepCheckRecipePossible(itemData, amount, inventory, hotbar, __instance);
        }

        // start crafting if the player can craft all needed materials
        [HarmonyPostfix]
        [HarmonyPatch(typeof(RecipeInspector), "Try_StartCrafting")]
        public static void Try_StartCrafting_Postfix(RecipeInspector __instance)
        {
            ItemData inspectedItem = (ItemData)AccessTools.Field(typeof(RecipeInspector), "m_InspectedItem").GetValue(__instance);
            int currentDesiredAmount = (int)AccessTools.Field(typeof(RecipeInspector), "m_CurrentDesiredAmount").GetValue(__instance);

            ItemContainer inventory = (ItemContainer)AccessTools.Field(typeof(RecipeInspector), "m_Inventory").GetValue(__instance);
            ItemContainer hotbar = (ItemContainer)AccessTools.Field(typeof(RecipeInspector), "m_Hotbar").GetValue(__instance);

            if(!deepCheckRecipePossible(inspectedItem, currentDesiredAmount, inventory, hotbar, __instance))
            {
                return;
            }

            CraftData arg = new CraftData
            {
                Result = inspectedItem,
                Amount = currentDesiredAmount
            };

            if(CraftingManager.lastCraftRequest != null && CraftingManager.lastCraftRequest.Result == arg.Result && CraftingManager.lastCraftRequest.Amount == arg.Amount)
            {
                // prevent double crafting
                CraftingManager.lastCraftRequest = null;
                return;
            }

            if (MonoSingleton<InventoryController>.Instance.CraftItem.Try(arg))
            {
                AccessTools.Field(typeof(RecipeInspector), "m_CurrentDesiredAmount").SetValue(__instance, 1);
                AccessTools.Method(typeof(RecipeInspector), "ShowRecipeInfo").Invoke(__instance, new object[] { inspectedItem });
            }
        }

        public static bool deepCheckRecipePossible(ItemData itemData, int amount, ItemContainer inventory, ItemContainer hotbar, RecipeInspector __instance)
        {
            CraftingManager.rItems.Clear();

            deepSearchRecipeRequire(itemData, amount, inventory, hotbar);

            foreach(KeyValuePair<string, int> entry in CraftingManager.rItems)
            {
                if(entry.Value > (inventory.GetItemCount(entry.Key) + hotbar.GetItemCount(entry.Key)))
                {
                    return false;
                }
                // __instance should only be null when comming from CraftingList_Patch which controls the color of the recipe in the crafting window which is okay if its white even tho no loom or mortar is nearby
                if(__instance != null && !checkRecipeNeedBuilding(itemData, __instance))
                {
                    return false;
                }
            }

            CraftingManager.inventory = inventory;
            CraftingManager.hotbar = hotbar;

            return true;
        }

        public static bool checkRecipeNeedBuilding(ItemData itemData, RecipeInspector __instance)
        {
            // check if loom is required
            for(int i = 0; i < itemData.PropertyValues.Count; i++)
            {
                if(itemData.PropertyValues[i].Name == "Loom Req")
                {
                    GameObject[] looms = GameObject.FindGameObjectsWithTag("Loom");
                    foreach(GameObject l in looms)
                    {
                        if((__instance.m_Player.transform.position - l.transform.position).magnitude <= 5f)
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }

            // check if mortar is required
            for (int i = 0; i < itemData.PropertyValues.Count; i++)
            {
                if (itemData.PropertyValues[i].Name == "Mortar Req")
                {
                    GameObject[] mortars = GameObject.FindGameObjectsWithTag("Mortar");
                    foreach (GameObject m in mortars)
                    {
                        if ((__instance.m_Player.transform.position - m.transform.position).sqrMagnitude <= 25f)
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }

            return true;
        }

        private static void deepSearchRecipeRequire(ItemData itemData, int amount, ItemContainer inventory, ItemContainer hotbar)
        {
            ItemDatabase db = MonoSingleton<InventoryController>.Instance.Database;

            foreach(RequiredItem reqItem in itemData.Recipe.RequiredItems)
            {
                foreach (ItemCategory category in db.Categories)
                {
                    foreach(ItemData iData in category.Items)
                    {
                        if(iData.Name == reqItem.Name && iData.IsCraftable && (reqItem.Amount > inventory.GetItemCount(reqItem.Name) + hotbar.GetItemCount(reqItem.Name)))
                        {
                            int alreadyHave = inventory.GetItemCount(reqItem.Name) + hotbar.GetItemCount(reqItem.Name);
                            deepSearchRecipeRequire(iData, reqItem.Amount * amount - alreadyHave, inventory, hotbar);
                        }
                        else if(iData.Name == reqItem.Name && !iData.IsCraftable)
                        {
                            // found base component of recipe, add to list
                            int alreadyHave = inventory.GetItemCount(reqItem.Name) + hotbar.GetItemCount(reqItem.Name);

                            if (CraftingManager.rItems.ContainsKey(reqItem.Name))
                            {
                                CraftingManager.rItems[reqItem.Name] += reqItem.Amount * amount;
                            }
                            else
                            {
                                CraftingManager.rItems.Add(reqItem.Name, reqItem.Amount * amount);
                            }

                            break;
                        }
                    }
                }
            }
        }
    }
}
