using HarmonyLib;
using LotsOfItems.Components;
using UnityEngine;

namespace LotsOfItems.Patches;

[HarmonyPatch(typeof(Playtime))]
static class TeddyBearPatches
{
    [HarmonyPatch(nameof(Playtime.StartJumprope))]
    [HarmonyPrefix]
    private static bool BribePlaytime(Playtime __instance, PlayerManager player)
    {
        if (player.itm.Has(teddyBearItem))
        {
            __instance.currentJumprope?.Destroy();

            player.itm.Remove(teddyBearItem);
            __instance.audMan.FlushQueue(true);
            __instance.audMan.PlaySingle(audPlaytimeHappy);

            var friend = __instance.gameObject.GetComponent<Playtime_TeddyBearFriend>() ?? __instance.gameObject.AddComponent<Playtime_TeddyBearFriend>();
            friend.Initialize(__instance);

            __instance.Navigator.maxSpeed = __instance.normSpeed;
            __instance.Navigator.SetSpeed(__instance.normSpeed);
            __instance.behaviorStateMachine.ChangeState(new Playtime_Cooldown(__instance, __instance, 120f));
            __instance.spriteRenderer[0].sprite = playtimeSprite;
            return false; // Skip jump rope
        }
        return true;
    }

    internal static Items teddyBearItem;
    internal static SoundObject audPlaytimeHappy;
    internal static Sprite playtimeSprite;
}