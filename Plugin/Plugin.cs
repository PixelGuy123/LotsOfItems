using BepInEx;
using HarmonyLib;
using LotsOfItems.Components;
using LotsOfItems.Patches;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LotsOfItems.Plugin
{
	[BepInPlugin(guid, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
	[BepInDependency("mtm101.rulerp.bbplus.baldidevapi", BepInDependency.DependencyFlags.HardDependency)]
	[BepInDependency("pixelguy.pixelmodding.baldiplus.pixelinternalapi", BepInDependency.DependencyFlags.HardDependency)]
	[BepInDependency("mtm101.rulerp.baldiplus.levelloader", BepInDependency.DependencyFlags.SoftDependency)]
	[BepInDependency("mtm101.rulerp.baldiplus.leveleditor", BepInDependency.DependencyFlags.SoftDependency)]
	public class LotOfItemsPlugin : BaseUnityPlugin
	{
		public const string guid = "pixelguy.pixelmodding.baldiplus.lotsOfItems", modPrefix = "lotOfItems";
		public static string ModPath;
		public static LotOfItemsPlugin plug;
		internal List<ItemData> availableItems = [];
		internal static AssetManager assetMan = new();
		internal static LayerMask onlyNpcPlayerLayers = LayerMask.GetMask("NPCs", "Player", LayerMask.LayerToName(LayerStorage.standardEntities));

#pragma warning disable IDE0051 // Remover membros privados não utilizados
		private void Awake()
#pragma warning restore IDE0051 // Remover membros privados não utilizados
		{
			plug = this;
			ModPath = AssetLoader.GetModPath(this);
			AssetLoader.LoadLocalizationFolder(Path.Combine(ModPath, "Language", "English"), Language.English);

			Harmony h = new(guid);
			h.PatchAllConditionals();

			LoadingEvents.RegisterOnAssetsLoaded(Info, () =>
			{
				try
				{
					assetMan.Add("aud_explode", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(ModPath, "fogMachine_explode.wav")), "LtsOItems_Vfx_Explode", SoundType.Effect, Color.white));

					TheItemBuilder.StartBuilding();

					SoundObject noEatRule = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(ModPath, "PRI_NoEating.wav")), "Vfx_PRI_NoEating", SoundType.Voice, new(0f, 0.117f, 0.482f));
					foreach (var principal in GenericExtensions.FindResourceObjects<Principal>())
						principal.audNoEating = noEatRule;

					foreach (var player in GenericExtensions.FindResourceObjects<PlayerManager>()) // History position record thing
					{
						player.gameObject.AddComponent<PlayerPositionHistory>().pm = player;
						player.gameObject.AddComponent<PlayerCustomAttributes>();
					}
					
				}
				catch (System.Exception e)
				{
					Debug.LogWarning("A CRASH HAPPENED IN THE PRE LOAD EVENT");
					Debug.LogException(e);
					MTM101BaldiDevAPI.CauseCrash(Info, e);
				}
			}, false);

			GeneratorManagement.RegisterFieldTripLootChange(this, (_, tripLoot) =>
			{

				for (int i = 0; i < availableItems.Count; i++)
				{
					var itm = availableItems[i];
					if (!itm.acceptFieldTrips) continue; // No real check for tripType, idk why would I need to have specific selections

					tripLoot.potentialItems.Add(new() { selection = itm.itm, weight = itm.weight });
				}
			});

			GeneratorManagement.Register(this, GenerationModType.Override, (name, num, sco) => sco.levelObject.forcedItems.Clear()); // forced items screw up in F1 >:(

			GeneratorManagement.Register(this, GenerationModType.Addend, (name, num, sco) =>
			{
				if (!sco.levelObject)
					return;

				bool levelObjectUsed = false;

				for (int i = 0; i < availableItems.Count; i++)
				{
					// This will allow you to disable items you want. The release build won't have it to force people to enjoy every single bit of this mod lol
					//if (!Config.Bind("Item Settings", 
					//	$"Disable {Singleton<LocalizationManager>.Instance.GetLocalizedText(availableItems[i].itm.nameKey)}", 
					//	true, 
					//	"If True, this item will spawn naturally in the game (in levels made by the Level Generator).")
					//.Value)
					//	continue;

					if (availableItems[i].acceptableFloors.Contains(name))
					{
						levelObjectUsed = true;

						var weight = new WeightedItemObject() { selection = availableItems[i].itm, weight = availableItems[i].weight };
						sco.levelObject.potentialItems = sco.levelObject.potentialItems.AddToArray(weight);
						if (availableItems[i].appearsInStore)
							sco.shopItems = sco.shopItems.AddToArray(weight);

					}
				}

				if (levelObjectUsed)
				{
					sco.levelObject.maxItemValue += 125; // To make more items spawn :)
					sco.levelObject.MarkAsNeverUnload();
				}
			});

			LoadingEvents.RegisterOnAssetsLoaded(Info, PostLoad, true);

			// In case I need to know what layers are applied, I use this simple script
			//DebugLayers(131072);
			//DebugLayers(LayerStorage.principalLookerMask);
			//DebugLayers(LayerStorage.standardEntities);


			//static void DebugLayers(LayerMask layerMask)
			//{
			//	Debug.Log("Debugging layer: " + (int)layerMask);
			//	for (int i = 0; i < 32; i++)
			//	{
			//		if ((layerMask & (1 << i)) != 0)
			//		{
			//			string layerName = LayerMask.LayerToName(i);
			//			if (!string.IsNullOrEmpty(layerName))
			//			{
			//				Debug.Log($"\t({i}): {layerName}");
			//			}
			//		}
			//	}
			//}

		}
		void PostLoad()
		{
			try
			{
				for (int i = 0; i < availableItems.Count; i++)
					availableItems[i].Prefab?.SetupPrefabPost();

				EditorPatch.AddItemsToEditor();
			}
			catch (System.Exception e)
			{
				Debug.LogWarning("A CRASH HAPPENED IN THE POST LOAD EVENT");
				Debug.LogException(e);
				MTM101BaldiDevAPI.CauseCrash(Info, e);
			}
		}
	}
}
