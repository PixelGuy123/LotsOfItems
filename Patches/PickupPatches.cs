using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using LotsOfItems.CustomItems.YTPs;
using UnityEngine;

namespace LotsOfItems.Patches;

[HarmonyPatch(typeof(Pickup))]
internal static class MakePickupBounce
{

    // ********** Dancing YTP Pickup Patch ***********
    [HarmonyPatch("Start")]
    [HarmonyPatch("AssignItem")]
    [HarmonyPostfix]
    static void DancingPickupSpecificPatch(Pickup __instance)
    {
        var bounce = __instance.gameObject.GetComponent<DancingPickup>();

        if (bounce)
            Object.Destroy(bounce);

        if (__instance.item.item is ITM_DancingYTP)
        {
            __instance.gameObject.AddComponent<DancingPickup>().AttachToPickup(__instance, Singleton<BaseGameManager>.Instance.Ec);
            return;
        }
    }

    // ********** Teleporting YTP Pickup Patches ***********

    [HarmonyPatch("Collect")]
    [HarmonyPrefix]
    static void RegisterItemObjectForTpYTP(Pickup __instance, out ItemObject __state) =>
        __state = __instance.item;

    [HarmonyPatch("Collect")]
    [HarmonyPostfix]
    static void TeleportIfTeleportationYTP(Pickup __instance, ItemObject __state)
    {
        if (__state.item is not ITM_TeleportingYTP)
            return;

        potentialSpots.Clear();
        var currentRoom = Singleton<BaseGameManager>.Instance.Ec.CellFromPosition(__instance.transform.position).room;
        foreach (var room in Singleton<BaseGameManager>.Instance.Ec.rooms)
        {
            if (room == currentRoom)
                continue;

            var spawns = room.itemSpawnPoints;
            for (int i = 0; i < spawns.Count; i++)
            {
                potentialSpots.Add(spawns[i].position);
            }
        }

        if (potentialSpots.Count == 0)
            return;

        var comp = __instance.GetComponent<TeleportingYTP_TpMarker>();
        if (!comp)
            comp = __instance.gameObject.AddComponent<TeleportingYTP_TpMarker>();

        if (++comp.tpTimes > ITM_TeleportingYTP.maxTpsPerInstance)
        {
            Object.Destroy(comp);
            return;
        }
        __instance.transform.position = potentialSpots[Random.Range(0, potentialSpots.Count)];
        __instance.gameObject.SetActive(true);
        __instance.AssignItem(__state);
    }

    readonly static List<Vector2> potentialSpots = [];


    // ********** Something Pickup Patch ***********

    [HarmonyPatch("Clicked")]
    [HarmonyPrefix]
    static bool InflateIfSomethingYTP(Pickup __instance)
    {
        if (!__instance.free) return true;

        if (__instance.item.item is ITM_SomethingYTP)
        {
            if (__instance.GetComponent<SomethingYTP_InflationMarker>())
                return false;

            if (Random.value > ITM_SomethingYTP.pickupChance) // If it is above the pickup chance, it means it's going to be a normal ytp, not an item
                return true;

            var inflationMarker = __instance.gameObject.AddComponent<SomethingYTP_InflationMarker>();
            __instance.StartCoroutine(AnimateInflation());

            IEnumerator AnimateInflation()
            {
                Vector3 final = Vector3.one * 4f, start = Vector3.one;
                float t = 0f;
                while (t < 1f)
                {
                    t += Time.deltaTime * Singleton<BaseGameManager>.Instance.Ec.EnvironmentTimeScale * 7.5f;
                    __instance.itemSprite.transform.localScale = Vector3.Slerp(start, final, t);

                    yield return null;
                }

                __instance.itemSprite.transform.localScale = Vector3.one;
                __instance.AssignItem(ITM_SomethingYTP.GetItem);
                Object.Destroy(inflationMarker);
            }

            return false;
        }

        return true;


    }




}