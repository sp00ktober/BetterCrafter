using System.Collections.Generic;
using UltimateSurvival;
using UltimateSurvival.GUISystem;

namespace BetterCrafter.Managers
{
    public static class CraftingManager
    {
        public static Dictionary<string, int> rItems = new Dictionary<string, int>();
        public static ItemContainer inventory { get; set; }
        public static ItemContainer hotbar { get; set; }
        public static int skipGiveItem { get; set; }
        public static bool lockSkipGiveItem { get; set; }
        public static CraftData lastCraftRequest { get; set; } // not perfet but should fix it for now. As we postfix patch we might double craftings for items which can be vanilla crafted
        public static List<int> craftDependencies = new List<int>(); // if crafting something that relies on crafting something else first it will have increasing numbers here at the same index as the games queue. needed to remove all items in chain if crafting is canceled.

        public static ItemData getItemData(string itemName)
        {
            ItemDatabase db = MonoSingleton<InventoryController>.Instance.Database;

            foreach (ItemCategory category in db.Categories)
            {
                foreach (ItemData iData in category.Items)
                {
                    if (iData.Name == itemName)
                    {
                        return iData;
                    }
                }
            }

            return null;
        }
    }
}
