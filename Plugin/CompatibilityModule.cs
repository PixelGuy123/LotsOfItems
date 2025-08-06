using BBTimes.Extensions;
using BepInEx.Bootstrap;
using CustomVendingMachines;
using HarmonyLib;
using MTM101BaldAPI;
using UnityEngine;

namespace LotsOfItems.Plugin;

internal static class CompatibilityModule
{
    internal static void InitializeOnAwake()
    {
        if (Chainloader.PluginInfos.ContainsKey("pixelguy.pixelmodding.baldiplus.customvendingmachines"))
            CustomVendingMachinesCompat.Loadup();
    }
}

internal static class CustomVendingMachinesCompat
{
    internal static void Loadup() =>
        CustomVendingMachinesPlugin.AddDataFromDirectory(System.IO.Path.Combine(LotOfItemsPlugin.ModPath, "VendingMachines"));
}

// ****** Patches Here *******
[HarmonyPatch]
[ConditionalPatchMod("pixelguy.pixelmodding.baldiplus.bbextracontent")]
static class ReplacementCharactersSupport
{
    [HarmonyPrefix, HarmonyPatch(typeof(GameExtensions), nameof(GameExtensions.IsPrincipal))]
    static bool IsAPrincipalPatch(NPC npc, ref bool __result) // For now, it'll just use Times' game extensions while there's no official api for replacement characters
    {
        __result = npc.IsAPrincipal();
        return false;
    }
    [HarmonyPrefix, HarmonyPatch(typeof(GameExtensions), nameof(GameExtensions.CallOutPrincipals))]
    static bool MakeAllPrincipalsGo(EnvironmentController ec, Vector3 position)
    {
        BBTimes.Extensions.GameExtensions.CallOutPrincipals(ec, position);
        return false;
    }
}
