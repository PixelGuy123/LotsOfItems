using System.Collections;
using HarmonyLib;
using UnityEngine;

namespace LotsOfItems.Patches;

[HarmonyPatch]
internal static class SodaMachinePatches // Patches to handle custom quarters
{
    [HarmonyPatch(typeof(SodaMachine), nameof(SodaMachine.InsertItem))]
    [HarmonyPrefix]
    static bool OverrideItemInsertion(SodaMachine __instance, PlayerManager pm)
    {
        // Setup
        var itm = pm.itm.items[pm.itm.selectedItem];
        bool runOriginal = true;

        // Dollar Bill action
        if (dollarBill == itm.itemType)
        {
            runOriginal = false;
            __instance.StartCoroutine(DollarBill_Delay(__instance, pm));
        }

        // Rest of the original InsertItem code
        if (!runOriginal)
        {
            __instance.usesLeft--;
            if (__instance.usesLeft <= 0 && __instance.meshRenderer != null)
            {
                __instance._materials = __instance.meshRenderer.sharedMaterials;
                __instance._materials[1] = __instance.outOfStockMat;
                __instance.meshRenderer.sharedMaterials = __instance._materials;
            }
        }

        return runOriginal;
    }

    internal static Items dollarBill;

    // IEnumerators to replace
    static IEnumerator DollarBill_Delay(SodaMachine sodaMachine, PlayerManager pm)
    {
        yield return null;
        ItemObject foundItem;
        if (sodaMachine.potentialItems.Length != 0)
        {
            foundItem = WeightedSelection<ItemObject>.RandomSelection(sodaMachine.potentialItems);
            pm.itm.AddItem(foundItem);
        }
        else
        {
            foundItem = sodaMachine.item;
            pm.itm.AddItem(sodaMachine.item);
        }

        Vector2 position = new(pm.transform.position.x, pm.transform.position.z);
        var room = pm.ec.CellFromPosition(pm.transform.position).room;
        for (int i = 0; i < 3; i++)
        {
            pm.ec.CreateItem(room, foundItem, position + Random.insideUnitCircle * 1.25f);
            pm.ec.items.RemoveAt(pm.ec.items.Count - 1); // To prevent pickups from getting into this system... they shouldn't!
        }
        yield break;
    }
}