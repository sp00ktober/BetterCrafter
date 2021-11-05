using HarmonyLib;
using UltimateSurvival.GUISystem;

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
    }
}
