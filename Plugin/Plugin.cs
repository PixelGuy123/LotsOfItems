using BepInEx;
using HarmonyLib;
using LotsOfItems.Patches;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;
using PixelInternalAPI.Extensions;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

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

#pragma warning disable IDE0051 // Remover membros privados não utilizados
		private void Awake()
#pragma warning restore IDE0051 // Remover membros privados não utilizados
		{
			plug = this;
			ModPath = AssetLoader.GetModPath(this);
			AssetLoader.LoadLocalizationFolder(Path.Combine(ModPath, "Language", "English"), Language.English);

			Harmony h = new(guid);
			h.PatchAllConditionals();

			LoadingEvents.RegisterOnAssetsLoaded(Info, () => {
				try
				{
					TheItemBuilder.StartBuilding();

					var fieldTrip = GenericExtensions.FindResourceObject<FieldTripBaseRoomFunction>();
					for (int i = 0; i < availableItems.Count; i++)
					{
						var itm = availableItems[i];
						if (!itm.acceptFieldTrips) continue;

						fieldTrip.potentialItems = fieldTrip.potentialItems.AddToArray(new() { selection = itm.itm, weight = itm.weight });
					}
					fieldTrip.MarkAsNeverUnload();
				}
				catch (System.Exception e)
				{
					Debug.LogWarning("A CRASH HAPPENED IN THE PRE LOAD EVENT");
					Debug.LogException(e);
					MTM101BaldiDevAPI.CauseCrash(Info, e);
				}
				}, false);

			GeneratorManagement.Register(this, GenerationModType.Addend, (name, num, sco) =>
			{
				if (!sco.levelObject)
					return;

				bool levelObjectUsed = false;
				
				for (int i = 0; i < availableItems.Count; i++)
				{
					if (availableItems[i].acceptableFloors.Contains(name))
					{
						levelObjectUsed = true;
						
						if (!availableItems[i].ItemForcedToSpawn)
						{
							var weight = new WeightedItemObject() { selection = availableItems[i].itm, weight = availableItems[i].weight };
							sco.levelObject.potentialItems = sco.levelObject.potentialItems.AddToArray(weight);
							if (availableItems[i].appearsInStore)
								sco.shopItems = sco.shopItems.AddToArray(weight);
						}
						else
							sco.levelObject.forcedItems.Add(availableItems[i].itm);

						
					}
				}

				if (levelObjectUsed)
				{
					sco.levelObject.maxItemValue += 125; // To make more items spawn :)
					sco.levelObject.MarkAsNeverUnload();
				}
			});

			LoadingEvents.RegisterOnAssetsLoaded(Info, PostLoad, true);

			
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
