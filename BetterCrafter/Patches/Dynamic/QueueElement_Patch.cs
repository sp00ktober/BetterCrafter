using HarmonyLib;
using UltimateSurvival;
using UltimateSurvival.GUISystem;
using UnityEngine;

namespace BetterCrafter.Patches.Dynamic
{
    [HarmonyPatch(typeof(QueueElement))]
    class QueueElement_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(QueueElement), "StartCrafting")]
        public static void StartCrafting_Postfix(QueueElement __instance)
        {
            // refresh doable recipe hints earlier
            __instance.ListCraft.ShowDoableRecipes();
        }

        // as i implement my own logic of giving back items we skip the games one and only propagate the event
        [HarmonyPrefix]
        [HarmonyPatch(typeof(QueueElement), "CancelCrafting")]
        public static bool CancelCrafting_Prefix(QueueElement __instance)
        {
            if (!MonoSingleton<InventoryController>.Instance.IsClosed)
            {
                __instance.ListCraft.ShowDoableRecipes();
            }
            __instance.Cancel.Send(__instance); // propagate event
            Object.Destroy(__instance.gameObject);

            return false;
        }
    }
}
