using HarmonyLib;
using BaldiLevelEditor;
using MTM101BaldAPI;
using UnityEngine;
using System.Collections.Generic;
using LotsOfItems.Plugin;
using PlusLevelLoader;
using MTM101BaldAPI.AssetTools;

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
				BaldiLevelEditorPlugin.itemObjects.Add(item.Key, item.Value);

				Sprite icon = item.Value.itemSpriteSmall;
				if (icon == item.Value.itemSpriteLarge)
				{
					var tex = icon.texture.ActualResize(32, 32);
					tex.name = "Resized_" + icon.texture.name;

					
					icon = AssetLoader.SpriteFromTexture2D(tex, 40f);
				}


				BaldiLevelEditorPlugin.Instance.assetMan.Add("UI/ITM_" + item.Key, icon);
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
