using System.Collections.Generic;
using HarmonyLib;

namespace LotsOfItems.Patches;

[HarmonyPatch(typeof(PlayerMovement))]
static class PlayerMovementPatches
{
    [HarmonyPatch(nameof(PlayerMovement.Start)), HarmonyPrefix]
    static void CheckForOtherPlayers()
    {
        while (disallowedPlayersToMove.ContainsKey(null))
            disallowedPlayersToMove.Remove(null); // If there's a null reference, clean it up
    }
    [HarmonyPatch("StaminaUpdate"), HarmonyPrefix]
    static bool PreventPlayerRunning(PlayerMovement __instance)
    {
        // Whether the player can run or not
        if (disallowedPlayersToMove.ContainsKey(__instance.pm))
        {
            Singleton<CoreGameManager>.Instance.GetHud(__instance.pm.playerNumber).SetStaminaValue(__instance.stamina / __instance.staminaMax);
            return false;
        }
        return true;
    }

    [HarmonyPatch("PlayerMove"), HarmonyPrefix]
    static bool PreventPlayerMoving(PlayerMovement __instance)
    {
        if (disallowedPlayersToMove.ContainsKey(__instance.pm))
        {
            __instance.StaminaUpdate(0f);
            return false;
        }
        return true;
    }

    readonly static Dictionary<PlayerManager, int> disallowedPlayersToMove = [];

    public static void AddDisabledPlayer(PlayerManager pm)
    {
        if (disallowedPlayersToMove.ContainsKey(pm))
            disallowedPlayersToMove[pm]++;
        else
            disallowedPlayersToMove.Add(pm, 1);
    }

    public static void RemoveDisabledPlayer(PlayerManager pm)
    {
        if (disallowedPlayersToMove.TryGetValue(pm, out int val) && val > 0)
            disallowedPlayersToMove[pm]--;
        else
            disallowedPlayersToMove.Remove(pm);
    }
}