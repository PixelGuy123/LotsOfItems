using HarmonyLib;
using BaldiLevelEditor;
using System.IO;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using System.Collections.Generic;
using LotsOfItems.Plugin;
using PlusLevelLoader;

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
			string[] files = Directory.GetFiles(Path.Combine(LotOfItemsPlugin.ModPath, "EditorUI"));
			for (int i = 0; i < files.Length; i++)
				BaldiLevelEditorPlugin.Instance.assetMan.Add("UI/" + Path.GetFileNameWithoutExtension(files[i]), AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(files[i]), 40f));

			foreach (var item in itemsToAdd)
				BaldiLevelEditorPlugin.itemObjects.Add(item.Key, item.Value);
			
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
