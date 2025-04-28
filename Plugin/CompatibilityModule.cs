using BepInEx.Bootstrap;
using CustomVendingMachines;

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