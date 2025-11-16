using System.Collections;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using LotsOfItems.Components;
using LotsOfItems.Patches;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI.SaveSystem;
using PixelInternalAPI;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.Plugin
{
	[BepInPlugin(guid, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
	[BepInDependency("mtm101.rulerp.bbplus.baldidevapi", BepInDependency.DependencyFlags.HardDependency)]
	[BepInDependency("pixelguy.pixelmodding.baldiplus.pixelinternalapi", BepInDependency.DependencyFlags.HardDependency)]
	[BepInDependency(editor_guid, BepInDependency.DependencyFlags.SoftDependency)]
	[BepInDependency(loader_guid, BepInDependency.DependencyFlags.SoftDependency)]
	public class LotOfItemsPlugin : BaseUnityPlugin
	{
		public const string guid = "pixelguy.pixelmodding.baldiplus.lotsOfItems", modPrefix = "lotOfItems",
			editor_guid = "mtm101.rulerp.baldiplus.levelstudio", loader_guid = "mtm101.rulerp.baldiplus.levelstudioloader",
			tileAlpha_shader = "Shader Graphs/TileStandard_AlphaClip";
		public static string ModPath;
		public static LotOfItemsPlugin plug;
		internal List<ItemData> availableItems = [];
		internal static AssetManager assetMan = new();
		internal static LayerMask onlyNpcPlayerLayers = LayerMask.GetMask("NPCs", "Player", LayerMask.LayerToName(LayerStorage.standardEntities)),
			onlyNpcLayers = LayerMask.GetMask("NPCs", LayerMask.LayerToName(LayerStorage.standardEntities));

		bool IsItemEnabled(ItemObject itm) =>
				Config.Bind("Item Settings",
							$"Disable {EnumExtensions.GetExtendedName<Items>((int)itm.itemType)}",
							true,
							"If True, this item will spawn naturally in the game (in levels made by the Level Generator).")
						.Value;
		private void Awake()
		{
			plug = this;
			ModPath = AssetLoader.GetModPath(this);
			AssetLoader.LoadLocalizationFolder(Path.Combine(ModPath, "Language", "English"), Language.English);

#if KOFI
			MTM101BaldiDevAPI.AddWarningScreen(
				"<color=#c900d4>Ko-fi Exclusive Build!</color>\nKo-fi members helped make this possible. This Lots O\' Items build was made exclusively for supporters. Please, don't share it publicly. If you'd like to support future content, visit my Ko-fi page!",
				false
			);
#endif

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

			LoadingEvents.RegisterOnAssetsLoaded(Info, PreLoad(), LoadingEventOrder.Pre);

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
				{
					if (levelObject.IsModifiedByMod(Info)) // To make sure no duplicated LevelObjects are being used twice
						continue;

					int removedItems = levelObject.forcedItems.RemoveAll(x => x.itemType != Items.BusPass); // forced items screw up in F1 >:(
					if (removedItems != 0)
						levelObject.MarkAsModifiedByMod(Info);
				}
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
					if (levelObject.IsModifiedByMod(Info)) // To make sure no duplicated LevelObjects are being used twice
						continue;

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
						levelObject.MarkAsModifiedByMod(Info);
					}

					BalanceOutWeights(objectDataPair, ref levelObject.potentialItems);
				}


			});

			LoadingEvents.RegisterOnAssetsLoaded(Info, PostLoad, LoadingEventOrder.Post);

			ResourceManager.AddGenStartCallback((_, _2, _3, _4) =>
			{
				PlayerMovementPatches.disallowedPlayersToMove.Clear(); // Clear to remove null players or smth
			});

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
		IEnumerator PreLoad()
		{
			int count = 2;
			bool hasLoader = Chainloader.PluginInfos.ContainsKey(loader_guid), hasEditor = Chainloader.PluginInfos.ContainsKey(editor_guid);
			if (hasLoader)
				count++;
			if (hasEditor)
				count++;
			yield return count;

			yield return "Loading basic assets...";
			assetMan.Add("aud_explode", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(ModPath, "fogMachine_explode.wav")), "LtsOItems_Vfx_Explode", SoundType.Effect, Color.white));
			assetMan.Add("audBump", GenericExtensions.FindResourceObjectByName<SoundObject>("Bang"));
			assetMan.Add("genericExplosionPrefab", ((LookAtGuy)NPCMetaStorage.Instance.Get(Character.LookAt).value).explosionPrefab);
			assetMan.Add("quickPop", GenericExtensions.FindResourceObjectByName<QuickExplosion>("QuickPop"));
			assetMan.Add("tex_white", TextureExtensions.CreateSolidTexture(480, 360, Color.white));
			assetMan.Add("audDrink", ((ITM_InvisibilityElixir)ItemMetaStorage.Instance.FindByEnum(Items.InvisibilityElixir).value.item).audUse);
			assetMan.Add("audBuzz", GenericExtensions.FindResourceObjectByName<SoundObject>("Elv_Buzz"));
			assetMan.Add("audThrow", ((ITM_NanaPeel)ItemMetaStorage.Instance.FindByEnum(Items.NanaPeel).value.item).audEnd);
			var wormSound = ObjectCreators.CreateSoundObject(GenericExtensions.FindResourceObjectByName<AudioClip>("WormholeAmbience"), string.Empty, SoundType.Effect, Color.white);
			wormSound.subtitle = false;
			assetMan.Add("WormholeAmbience", wormSound);
			for (int i = 0; i < 3; i++)
				assetMan.Add("YtpPickup_" + i, GenericExtensions.FindResourceObjectByName<SoundObject>("YTPPickup_" + i));
			SoundObject noEatRule = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(ModPath, "PRI_NoEating.wav")), "Vfx_PRI_NoEating", SoundType.Voice, new(0f, 0.117f, 0.482f));
			foreach (var principal in GenericExtensions.FindResourceObjects<Principal>())
				principal.audNoEating = noEatRule;

			// Main part of this code
			yield return "Loading lots of items...";
			try
			{
				TheItemBuilder.StartBuilding();
			}
			catch (System.Exception e)
			{
				Debug.LogWarning("A CRASH HAPPENED IN THE PRE LOAD EVENT");
				Debug.LogException(e);
				MTM101BaldiDevAPI.CauseCrash(Info, e);
			}

			if (hasLoader)
			{
				yield return "Loading items to the level loader...";
				EditorCompat.LoadLevelLoaderAssets();
			}
			if (hasEditor)
			{
				yield return "Registering level studio callback...";
				EditorCompat.LoadStudioEditorCallback();
			}

		}
		void PostLoad()
		{
			try
			{
				for (int i = 0; i < availableItems.Count; i++)
					availableItems[i].Prefab?.SetupPrefabPost();
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
				// Null checks because A-Grade somehow screws this up
				if (!weights[i].selection || !objectDataPair.TryGetValue(weights[i].selection, out var data))
					continue;

				int valueToBalance = 0, includedItems = 0;
				for (int z = 0; z < weights.Count; z++)
				{
					if (z == i || !weights[z].selection) // Another null check here for the same reason
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
