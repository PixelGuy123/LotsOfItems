using HarmonyLib;
using LotsOfItems.Components;

namespace LotsOfItems.Patches;

[HarmonyPatch(typeof(Principal))]
internal static class TeaPatches
{
    [HarmonyPatch(nameof(Principal.SendToDetention))]
    [HarmonyPrefix]
    static bool CheckIfDetentionIsNecessary(Principal __instance)
    {
        if (!__instance.targetedPlayer.itm.Has(teaItem) && !__instance.GetComponent<Marker_PrincipalOccupied>()) return true;
        __instance.targetedPlayer.itm.Remove(teaItem);
        __instance.targetedPlayer.ClearGuilt();
        __instance.detentionLevel = 0; // Resets fully his detentions
        __instance.gameObject.AddComponent<Marker_PrincipalOccupied>().SetTimer(__instance, 60f);
        __instance.LoseTrackOfPlayer(__instance.targetedPlayer);
        __instance.behaviorStateMachine.ChangeState(new Principal_Wandering(__instance));
        return false;
    }

    [HarmonyPatch(nameof(Principal.ObservePlayer))]
    [HarmonyPrefix]
    static bool ShouldRunWhileDrinking(Principal __instance) => __instance.GetComponent<Marker_PrincipalOccupied>() == null;

    internal static Items teaItem;
}

