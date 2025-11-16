using HarmonyLib;
using LotsOfItems.Components;

namespace LotsOfItems.Patches;

[HarmonyPatch(typeof(PlayerManager))]
static class PlayerManagerPatch
{
    [HarmonyPatch(nameof(PlayerManager.Start)), HarmonyPrefix]
    static bool AutoAddPlayerPosHistory(PlayerManager __instance) // really important since it needs to keep track beginning from spawn
    {
        if (!__instance.GetComponent<PlayerPositionHistory>())
            __instance.gameObject.AddComponent<PlayerPositionHistory>().pm = __instance;

        return true;
    }
}