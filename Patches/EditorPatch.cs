using System.Collections.Generic;
using System.IO;
using BaldiLevelEditor;
using HarmonyLib;
using LotsOfItems.Plugin;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using PlusLevelLoader;
using UnityEngine;

namespace LotsOfItems.Patches
{
	[ConditionalPatchMod("mtm101.rulerp.baldiplus.leveleditor")]
	[HarmonyPatch]
	internal static class EditorPatch
	{

		public static void AddItemsToEditor()
		{
			HashSet<ItemObject> alreadySeenItems = [];
			for (int i = 0; i < LotOfItemsPlugin.plug.availableItems.Count; i++)
			{
				var itm = LotOfItemsPlugin.plug.availableItems[i].itm;
				if (!alreadySeenItems.Contains(itm))
				{
					AddItem(itm);
					alreadySeenItems.Add(itm);
				}
			}
		}

		static void AddItem(ItemObject itm)
		{
			var itmEnum = itm.itemType == Items.Points ? itm.item.name : itm.itemType.ToStringExtended();
			string name = LotOfItemsPlugin.modPrefix + itmEnum;

			itemsToAdd.Add(name, itm);
		}

		[HarmonyPatch(typeof(PlusLevelEditor), "Initialize")]
		[HarmonyPostfix]
		static void InitializeStuff(PlusLevelEditor __instance)
		{
			foreach (var tool in itemsToAdd)
				__instance.toolCats.Find(x => x.name == "items").tools.Add(new ItemTool(tool.Key));
		}

		[HarmonyPatch(typeof(LotOfItemsPlugin), "PostLoad")]
		[HarmonyPostfix]
		static void LevelEditorChangesHere()
		{
			//string[] files = Directory.GetFiles(Path.Combine(LotOfItemsPlugin.ModPath, "EditorUI"));
			//for (int i = 0; i < files.Length; i++)
			//	BaldiLevelEditorPlugin.Instance.assetMan.Add("UI/" + Path.GetFileNameWithoutExtension(files[i]), AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(files[i]), 40f));

			foreach (var item in itemsToAdd)
			{
				try
				{
					BaldiLevelEditorPlugin.itemObjects.Add(item.Key, item.Value);
					var maskRef = AssetLoader.TextureFromFile(Path.Combine(LotOfItemsPlugin.ModPath, "Mask_itemSlotMask.png"));

					Sprite icon = item.Value.itemSpriteSmall;
					Texture2D tex = icon.texture;
					if (icon == item.Value.itemSpriteLarge)
					{
						tex = icon.texture.ActualResize(32, 32);
						tex.name = "Resized_" + tex.name;
					}

					tex = tex.Mask(maskRef);
					tex.name = "EditorIcon_" + tex.name;

					BaldiLevelEditorPlugin.Instance.assetMan.Add("UI/ITM_" + item.Key, AssetLoader.SpriteFromTexture2D(tex, 40f));
				}
				catch (System.Exception e)
				{
					Debug.LogException(e);
					MTM101BaldiDevAPI.CauseCrash(LotOfItemsPlugin.plug.Info, new("Looks like an item failed to be loaded into the editor: " + item.Key));
				}
			}

		}

		readonly internal static Dictionary<string, ItemObject> itemsToAdd = [];



	}

	[ConditionalPatchMod("mtm101.rulerp.baldiplus.levelloader")]
	[HarmonyPatch]
	internal static class LevelLoaderPatch
	{
		[HarmonyPostfix]
		[HarmonyPatch(typeof(LotOfItemsPlugin), "PostLoad")]
		static void AddMyItems()
		{
			foreach (var item in EditorPatch.itemsToAdd)
				PlusLevelLoaderPlugin.Instance.itemObjects.Add(item.Key, item.Value);
		}
	}
}
