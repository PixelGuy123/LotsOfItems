using System.Collections.Generic;
using HarmonyLib;

namespace LotsOfItems.Patches;

[HarmonyPatch(typeof(Student))]
internal static class StudentPatches
{
    [HarmonyPatch(nameof(Student.ItemFits))]
    [HarmonyPostfix]
    static bool ForceAccept(Items item, Student __instance, ref bool __result) =>
        __result = __result || (!__instance.complete && globallyAcceptableLostItems.Contains(item));

    public readonly static HashSet<Items> globallyAcceptableLostItems = [];
}