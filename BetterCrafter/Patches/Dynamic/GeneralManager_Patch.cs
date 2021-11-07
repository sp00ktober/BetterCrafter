using HarmonyLib;
using System.Collections.Generic;
using UltimateSurvival;
using UnityEngine;

// This is probably overcomplicated but it does its job C:

namespace BetterCrafter.Patches.Dynamic
{
    /*
    [HarmonyPatch(typeof(GeneralManager))]
    class GeneralManager_Patch
    {
        static bool cheated = false;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GeneralManager), "Update")]
        public static void Update_Postfix(GeneralManager __instance)
        {
            if (cheated)
            {
                return;
            }
            ItemDatabase db = MonoSingleton<InventoryController>.Instance.Database;

            foreach (ItemCategory category in db.Categories)
            {
                foreach (ItemData iData in category.Items)
                {
                    if(iData.Name == "ARTIFACT")
                    {
                        int num, added = 0;

                        List<ItemProperty.Value> list = new List<ItemProperty.Value>();
                        ItemProperty.Value v = new ItemProperty.Value();
                        v.m_Int.m_Current = 1;
                        v.m_Name = "Annales";
                        list.Add(v);

                        List<ItemProperty.Value> list2 = new List<ItemProperty.Value>();
                        ItemProperty.Value v2 = new ItemProperty.Value();
                        v2.m_Int.m_Current = 2;
                        v2.m_Name = "Annales";
                        list2.Add(v2);

                        List<ItemProperty.Value> list3 = new List<ItemProperty.Value>();
                        ItemProperty.Value v3 = new ItemProperty.Value();
                        v3.m_Int.m_Current = 3;
                        v3.m_Name = "Annales";
                        list3.Add(v3);

                        __instance.m_Inventory?.TryAddItem(iData, 4);
                        for(int i = 0; i < __instance.m_Inventory.Slots?.Count; i++)
                        {
                            Debug.Log("trying " + i);
                            if(__instance.m_Inventory.Slots[i]?.CurrentItem?.ItemData?.Name == "ARTIFACT" && added == 0)
                            {
                                Debug.Log("ends up as unknown");
                                AccessTools.Method(typeof(ItemHolder), "set_CurrentItem").Invoke(__instance.m_Inventory.Slots[i].ItemHolder, new object[] { new SavableItem(iData, 1, list) });
                                Debug.Log(__instance.m_Inventory.Slots[i].CurrentItem.GetPropertyValue("Annales").Int.Current);
                                added++;
                            }
                            else if (__instance.m_Inventory.Slots[i]?.CurrentItem?.ItemData?.Name == "ARTIFACT" && added == 1)
                            {
                                Debug.Log("will be 1st");
                                AccessTools.Method(typeof(ItemHolder), "set_CurrentItem").Invoke(__instance.m_Inventory.Slots[i].ItemHolder, new object[] { new SavableItem(iData, 1, list) });
                                Debug.Log(__instance.m_Inventory.Slots[i].CurrentItem.GetPropertyValue("Annales").Int.Current);
                                added++;
                            }
                            else if (__instance.m_Inventory.Slots[i]?.CurrentItem?.ItemData?.Name == "ARTIFACT" && added == 2)
                            {
                                Debug.Log("will be 2nd");
                                AccessTools.Method(typeof(ItemHolder), "set_CurrentItem").Invoke(__instance.m_Inventory.Slots[i].ItemHolder, new object[] { new SavableItem(iData, 1, list2) });
                                Debug.Log(__instance.m_Inventory.Slots[i].CurrentItem.GetPropertyValue("Annales").Int.Current);
                                added++;
                            }
                            else if (__instance.m_Inventory.Slots[i]?.CurrentItem?.ItemData?.Name == "ARTIFACT" && added == 3)
                            {
                                Debug.Log("will be 3rd");
                                AccessTools.Method(typeof(ItemHolder), "set_CurrentItem").Invoke(__instance.m_Inventory.Slots[i].ItemHolder, new object[] { new SavableItem(iData, 1, list3) });
                                Debug.Log(__instance.m_Inventory.Slots[i].CurrentItem.GetPropertyValue("Annales").Int.Current);
                                added++;
                            }
                        }
                    }
                }
            }

            cheated = true;
        }
    }
    */
}
