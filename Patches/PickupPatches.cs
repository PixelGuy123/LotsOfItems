using System.Collections;
using HarmonyLib;
using LotsOfItems.CustomItems.YTPs;
using UnityEngine;

namespace LotsOfItems.Patches;

[HarmonyPatch(typeof(Pickup))]
internal static class MakePickupBounce
{

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