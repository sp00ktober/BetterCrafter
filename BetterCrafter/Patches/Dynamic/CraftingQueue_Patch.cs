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
        private static bool lockStartNextDecrease = false;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CraftingQueue), "Try_CraftItem")]
        public static bool Try_CraftItem_Prefix(CraftingQueue __instance, ref bool __result, CraftData craftData)
        {
            CraftingManager.skipGiveItem = 0;

            bool res = deepCreateQueue(__instance, craftData, 0);
            if (res)
            {
                CraftingManager.lastCraftRequest = craftData;
            }

            __result = res;
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CraftingQueue), "StartNext")]
        public static void StartNext_Postfix()
        {
            if (!lockStartNextDecrease)
            {
                CraftingManager.craftDependencies.RemoveAt(CraftingManager.craftDependencies.Count - 1);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CraftingQueue), "On_CraftingCanceled")]
        public static bool On_CraftingCanceled(CraftingQueue __instance, QueueElement queueElement)
        {
            int index, depID;
            bool wasActiveItem = false;

            if (__instance.m_Queue.Contains(queueElement))
            {
                index = __instance.m_Queue.IndexOf(queueElement);
            }
            else
            {
                index = CraftingManager.craftDependencies.Count - 1;
                wasActiveItem = true;
            }

            depID = CraftingManager.craftDependencies[index];

            // example depID array: 0 1 0 4 3 2 1 0 1 0 (rightmost would be active item) (NOTE: invert this as dependencies for a recipe are added before the "big" recipe but they have a higher id)

            // go to the left point of connected recipes. We need to delete up to index as thats all depending on the recipe at index.
            int startDelIndex = index;
            while(startDelIndex - 1 >= 0 && CraftingManager.craftDependencies[startDelIndex - 1] < CraftingManager.craftDependencies[startDelIndex])
            {
                startDelIndex -= 1;
            }

            // go until index and delete
            int delAmount = index - startDelIndex + 1; // +1 to account for index == startDelIndex
            CraftingManager.craftDependencies.RemoveRange(startDelIndex, delAmount);

            // now delete games queue thats connected to the canceled recipe
            if (wasActiveItem)
            {
                if(delAmount == 1)
                {
                    lockStartNextDecrease = true;
                    CraftingManager.lockSkipGiveItem = true;

                    giveBackItem(__instance.m_ActiveElement);

                    CraftingManager.lockSkipGiveItem = false;
                    __instance.StartNext();
                    lockStartNextDecrease = false;
                }
                else
                {
                    // remove delAMount - 1 because one recipe is still marked the active one
                    for(int i = __instance.m_Queue.Count - delAmount + 1; i < delAmount - 1; i++)
                    {
                        CraftingManager.lockSkipGiveItem = true;

                        giveBackItem(__instance.m_Queue[i]);

                        CraftingManager.lockSkipGiveItem = false;
                        UnityEngine.Object.Destroy(__instance.m_Queue[i].gameObject);
                    }
                    __instance.m_Queue.RemoveRange(__instance.m_Queue.Count - delAmount + 1, delAmount - 1);
                    
                    lockStartNextDecrease = true;
                    CraftingManager.lockSkipGiveItem = true;

                    giveBackItem(__instance.m_ActiveElement);

                    CraftingManager.lockSkipGiveItem = false;
                    __instance.StartNext();
                    lockStartNextDecrease = false;
                }
            }
            else
            {
                for (int i = index; i < delAmount; i++)
                {
                    CraftingManager.lockSkipGiveItem = true;

                    giveBackItem(__instance.m_Queue[i]);

                    CraftingManager.lockSkipGiveItem = false;
                    UnityEngine.Object.Destroy(__instance.m_Queue[i].gameObject);
                }
                __instance.m_Queue.RemoveRange(index, delAmount);
            }

            return false;
        }

        private static void giveBackItem(QueueElement qe)
        {
            // TODO:
            /*
             * the idea is to store all used ingridients in the method below, or probably in queueElement.Initialize(). all ingridients per recipe used.
             * also store the amount if skipGiveItem used.
             * then i can restore all of that here.
             */
            if (this.m_AmountToCraftRemained > 0)
            {
                foreach (RequiredItem requiredItem in this.m_CraftData.Result.Recipe.RequiredItems)
                {
                    ItemData itemData;
                    MonoSingleton<InventoryController>.Instance.Database.FindItemByName(requiredItem.Name, out itemData);
                    if (!this.m_Inventory.TryAddItem(itemData, requiredItem.Amount * this.m_AmountToCraftRemained))
                    {
                        this.m_Hotbar.TryAddItem(itemData, requiredItem.Amount * this.m_AmountToCraftRemained);
                    }
                }
            }

            CraftingManager.skipGiveItem -= qe.m_CraftData.Amount;
        }

        private static bool deepCreateQueue(CraftingQueue cq, CraftData cd, int depID)
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

                    if(!deepCreateQueue(cq, cd_tmp, depID + 1))
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

                CraftingManager.craftDependencies.Insert(0, depID);

                return true;
            }
            return false;
        }
    }
}
