using System.Collections.Generic;
using System.IO;
using BBTimes.Extensions;
using BepInEx.Bootstrap;
using CustomVendingMachines;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using PlusLevelStudio;
using PlusLevelStudio.Editor;
using PlusLevelStudio.Editor.Tools;
using PlusStudioLevelLoader;
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

internal static class EditorCompat
{
    internal static void LoadStudioEditorCallback()
    {
        EditorInterfaceModes.AddModeCallback(LoadStudioEditorContent);
    }

    static void LoadStudioEditorContent(EditorMode mode, bool vanillaCompliant)
    {
        foreach (var item in itemsToAdd)
        {
            try
            {
                bool resized = false;
                Sprite icon = item.Value.itemSpriteSmall;
                Texture2D tex = icon.texture;
                if (tex.width != 32 || tex.height != 32)
                {
                    resized = true;
                    tex = icon.texture.ActualResize(32, 32);
                    tex.name = "Resized_" + tex.name;
                }
                tex.name = "EditorIcon_" + tex.name;

                EditorInterfaceModes.AddToolToCategory(mode,
                "items",
                !resized ? new ItemTool(item.Key) : new ItemTool(item.Key, AssetLoader.SpriteFromTexture2D(tex, 1f)));
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                MTM101BaldiDevAPI.CauseCrash(LotOfItemsPlugin.plug.Info, new("Looks like an item failed to be loaded into the editor: " + item.Key));
            }
        }
    }

    internal static void LoadLevelLoaderAssets()
    {
        HashSet<ItemObject> alreadySeenItems = [];
        for (int i = 0; i < LotOfItemsPlugin.plug.availableItems.Count; i++)
        {
            var itm = LotOfItemsPlugin.plug.availableItems[i].itm;
            if (!alreadySeenItems.Contains(itm))
            {
                var itmEnum = itm.itemType == Items.Points ? itm.item.name : itm.itemType.ToStringExtended();
                string name = LotOfItemsPlugin.modPrefix + itmEnum;

                itemsToAdd.Add(name, itm);
                LevelLoaderPlugin.Instance.itemObjects.Add(name, itm);
                alreadySeenItems.Add(itm);
            }
        }
    }

    readonly internal static Dictionary<string, ItemObject> itemsToAdd = [];
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
