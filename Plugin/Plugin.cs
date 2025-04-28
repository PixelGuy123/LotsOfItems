using System.Collections.Generic;
using System.IO;
using BepInEx;
using HarmonyLib;
using LotsOfItems.Components;
using LotsOfItems.Patches;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI.SaveSystem;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
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

		bool IsItemEnabled(ItemObject itm) =>
				Config.Bind("Item Settings",
							$"Disable {Singleton<LocalizationManager>.Instance.GetLocalizedText(itm.nameKey)
							.Replace('\'', ' ')}",
							true,
							"If True, this item will spawn naturally in the game (in levels made by the Level Generator).")
						.Value;
		private void Awake()
		{
			plug = this;
			ModPath = AssetLoader.GetModPath(this);
			AssetLoader.LoadLocalizationFolder(Path.Combine(ModPath, "Language", "English"), Language.English);

			try
			{
				CompatibilityModule.InitializeOnAwake();
			}
			catch (System.Exception e)
			{
				Debug.LogWarning("LOTSOFITEMS: Failed to initialize a compatibility module!");
				Debug.LogException(e);
			}

			ModdedSaveGame.AddSaveHandler(Info);

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
				Dictionary<ItemObject, ItemData> objectDataPair = [];
				for (int i = 0; i < availableItems.Count; i++)
				{
					// This will allow you to disable items you want. The release build won't have it to force people to enjoy every single bit of this mod lol
					if (!IsItemEnabled(availableItems[i].itm))
						continue;

					var itm = availableItems[i];
					if (!itm.acceptFieldTrips) continue; // No real check for tripType, idk why would I need to have specific selections

					tripLoot.potentialItems.Add(new() { selection = availableItems[i].itm, weight = availableItems[i].weight });
					objectDataPair.Add(availableItems[i].itm, availableItems[i]);
				}

				BalanceOutListWeights(objectDataPair, tripLoot.potentialItems);
			});

			GeneratorManagement.Register(this, GenerationModType.Override, (name, num, sco) =>
			{
				foreach (var levelObject in sco.GetCustomLevelObjects())
					levelObject.forcedItems.RemoveAll(x => x.itemType != Items.BusPass); // forced items screw up in F1 >:(
			});

			GeneratorManagement.Register(this, GenerationModType.Addend, (name, num, sco) =>
			{
				// **** Adds shopItems to the SceneObject
				Dictionary<ItemObject, ItemData> objectDataPair = [];

				for (int i = 0; i < availableItems.Count; i++)
				{
					// This will allow you to disable items you want.
					if (!IsItemEnabled(availableItems[i].itm))
						continue;

					if (availableItems[i].appearsInStore)
					{
						sco.shopItems = sco.shopItems.AddToArray(new() { selection = availableItems[i].itm, weight = availableItems[i].weight });

						objectDataPair.Add(availableItems[i].itm, availableItems[i]);
					}
				}

				BalanceOutWeights(objectDataPair, ref sco.shopItems);


				// **** Adds stuff to individual Level Objects


				foreach (var levelObject in sco.GetCustomLevelObjects())
				{
					bool levelObjectUsed = false;

					objectDataPair.Clear();

					for (int i = 0; i < availableItems.Count; i++)
					{
						// This will allow you to disable items you want.
						if (!IsItemEnabled(availableItems[i].itm))
							continue;

						if (availableItems[i].AcceptsLevel(name, levelObject))
						{
							levelObjectUsed = true;

							levelObject.potentialItems = levelObject.potentialItems.AddToArray(new() { selection = availableItems[i].itm, weight = availableItems[i].weight });

							objectDataPair.Add(availableItems[i].itm, availableItems[i]);
						}
					}

					if (levelObjectUsed)
					{
						levelObject.maxItemValue += 125; // To make more items spawn :)
						levelObject.MarkAsNeverUnload();
					}

					BalanceOutWeights(objectDataPair, ref levelObject.potentialItems);
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

		void BalanceOutListWeights(Dictionary<ItemObject, ItemData> objectDataPair, List<WeightedItemObject> weights)
		{
			//Debug.Log("------ new weight iteration coming -------");

			for (int i = 0; i < weights.Count; i++)
			{
				if (!objectDataPair.TryGetValue(weights[i].selection, out var data))
					continue;

				int valueToBalance = 0, includedItems = 0;
				for (int z = 0; z < weights.Count; z++)
				{
					if (z == i)
						continue;

					if (weights[i].selection.itemType == data.replacingItem || (objectDataPair.TryGetValue(weights[z].selection, out var newData) && newData.replacingItem != data.replacingItem))
					{
						valueToBalance += weights[i].weight;
						includedItems++;
					}
				}

				if (includedItems == 0)
					continue;

				weights[i].weight = Mathf.FloorToInt((valueToBalance + weights[i].weight) / (float)includedItems); // Mathf to avoid 0 division
				weights[i].weight = Mathf.Max(1, weights[i].weight); // Make sure to not go below 1

				//Debug.Log($"New weight for {Singleton<LocalizationManager>.Instance.GetLocalizedText(weights[i].selection.nameKey)} is {weights[i].weight}");
			}
		}

		void BalanceOutWeights(Dictionary<ItemObject, ItemData> objectDataPair, ref WeightedItemObject[] weights)
		{
			//Debug.Log("------ new weight iteration coming -------");

			for (int i = 0; i < weights.Length; i++)
			{
				if (!objectDataPair.TryGetValue(weights[i].selection, out var data))
					continue;

				int valueToBalance = 0, includedItems = 0;
				for (int z = 0; z < weights.Length; z++)
				{
					if (z == i)
						continue;

					if (weights[i].selection.itemType == data.replacingItem || (objectDataPair.TryGetValue(weights[z].selection, out var newData) && newData.replacingItem != data.replacingItem))
					{
						valueToBalance += weights[i].weight;
						includedItems++;
					}
				}

				if (includedItems == 0)
					continue;

				weights[i].weight = Mathf.FloorToInt((valueToBalance + weights[i].weight) / (float)includedItems); // Mathf to avoid 0 division
				weights[i].weight = Mathf.Max(1, weights[i].weight); // Make sure to not go below 1

				//Debug.Log($"New weight for {Singleton<LocalizationManager>.Instance.GetLocalizedText(weights[i].selection.nameKey)} is {weights[i].weight}");
			}
		}
	}
}
