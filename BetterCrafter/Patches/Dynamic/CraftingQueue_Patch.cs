using BetterCrafter.Managers;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UltimateSurvival;
using UltimateSurvival.GUISystem;
using UnityEngine;

namespace BetterCrafter.Patches.Dynamic
{
    [HarmonyPatch(typeof(CraftingQueue))]
    class CraftingQueue_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CraftingQueue), "Try_CraftItem")]
        public static bool Try_CraftItem_Prefix(CraftingQueue __instance, ref bool __result, CraftData craftData)
        {
            CraftingManager.skipGiveItem = 0;

            bool res = deepCreateQueue(__instance, craftData);
            if (res)
            {
                CraftingManager.lastCraftRequest = craftData;
            }

            __result = res;
            return false;
        }

        private static bool deepCreateQueue(CraftingQueue cq, CraftData cd)
        {
            foreach(RequiredItem ri in cd.Result.Recipe.RequiredItems)
            {
                int playerHasAmount = CraftingManager.inventory.GetItemCount(ri.Name) + CraftingManager.hotbar.GetItemCount(ri.Name);
                int diff = ri.Amount * cd.Amount - playerHasAmount;
                ItemData rid = CraftingManager.getItemData(ri.Name);

                if (diff > 0 && rid.IsCraftable)
                {
                    CraftData cd_tmp = new CraftData();
                    cd_tmp.Amount = diff;
                    cd_tmp.Result = rid;

                    CraftingManager.skipGiveItem += diff;

                    if(!deepCreateQueue(cq, cd_tmp))
                    {
                        return false;
                    }
                }
            }

            int maxElements = (int)AccessTools.Field(typeof(CraftingQueue), "m_MaxElements").GetValue(cq);
            QueueElement qE = (QueueElement)AccessTools.Field(typeof(CraftingQueue), "m_QueueElementTemplate").GetValue(cq);
            QueueElement activeElement = (QueueElement)AccessTools.Field(typeof(CraftingQueue), "m_ActiveElement").GetValue(cq);
            Transform queueParent = (Transform)AccessTools.Field(typeof(CraftingQueue), "m_QueueParent").GetValue(cq);
            List<QueueElement> queue = (List<QueueElement>)AccessTools.Field(typeof(CraftingQueue), "m_Queue").GetValue(cq);

            int num = cq.gameObject.GetComponentsInChildren<QueueElement>().Length;
            if (num < maxElements)
            {
                QueueElement queueElement = UnityEngine.Object.Instantiate<QueueElement>(qE);
                queueElement.gameObject.SetActive(true);
                queueElement.transform.SetParent(queueParent);
                queueElement.transform.SetAsFirstSibling();
                queueElement.transform.localPosition = Vector3.zero;
                queueElement.transform.localScale = Vector3.one;
                queueElement.Initialize(cd, CraftingManager.inventory, CraftingManager.hotbar);
                queueElement.Cancel.AddListener(new Action<QueueElement>(cq.On_CraftingCanceled));
                if (num == 0)
                {
                    queueElement.StartCrafting();
                    queueElement.Complete.AddListener(new Action(cq.StartNext));
                    activeElement = queueElement;
                }
                else
                {
                    queue.Insert(0, queueElement);
                }
                return true;
            }
            return false;
        }

        private static void deepTakeItems()
        {
            ItemContainer inventory = CraftingManager.inventory;
            ItemContainer hotbar = CraftingManager.hotbar;

            foreach (KeyValuePair<string, int> entry in CraftingManager.rItems)
            {
                if (inventory.GetItemCount(entry.Key) >= entry.Value)
                {
                    inventory.RemoveItems(entry.Key, entry.Value);
                }
                else if (inventory.GetItemCount(entry.Key) > 0 && inventory.GetItemCount(entry.Key) < entry.Value)
                {
                    int amount = entry.Value - inventory.GetItemCount(entry.Key);
                    inventory.RemoveItems(entry.Key, inventory.GetItemCount(entry.Key));
                    hotbar.RemoveItems(entry.Key, amount);
                }
                else
                {
                    hotbar.RemoveItems(entry.Key, entry.Value);
                }
            }
        }
    }
}
