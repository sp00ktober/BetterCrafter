using HarmonyLib;
using UltimateSurvival.GUISystem;
using UnityEngine.UI;

namespace BetterCrafter.Patches.Dynamic
{
    [HarmonyPatch(typeof(CraftingList))]
    class CraftingList_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CraftingList), "ShowDoableRecipes")]
        public static void ShowDoableRecipes_Postfix(CraftingList __instance)
        {
            ItemContainer inventory = __instance.m_Inventory;
            ItemContainer hotbar = __instance.m_Hotbar;

            for(int i = 0; i < __instance.recipeGenerated.Count; i++)
            {
                bool flag = true;
                if(__instance.ItemsRecip[i].PropertyValues.Count > 0 && __instance.ItemsRecip[i].PropertyValues[0].Name == "Unlocked" && !__instance.ItemsRecip[i].PropertyValues[0].Bool)
                {
                    flag = false;
                }

                if(flag && RecipeInspector_Patch.deepCheckRecipePossible(__instance.ItemsRecip[i], 1, inventory, hotbar, null))
                {
                    __instance.recipeGenerated[i].GetComponent<Image>().color = __instance.colori[0];
                }
            }

            __instance.inspectorRecipe.RefreshPanel();
        }
    }
}
