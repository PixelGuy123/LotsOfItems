using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using LotsOfItems.ItemPrefabStructures;
using PixelInternalAPI.Extensions;
using LotsOfItems.CustomItems;
using LotsOfItems.CustomItems.YTPs;
using LotsOfItems.CustomItems.Teleporters;

namespace LotsOfItems.Plugin
{
	internal static class TheItemBuilder
	{
		public static void StartBuilding()
		{
			// ---------- YTPS
			var item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("squareYtp")
				.SetGeneratorCost(20)
				.SetEnum(Items.Points)
				.SetMeta(ItemFlags.InstantUse, [])
				.SetAsInstantUse()
				.SetPickupSound(GetGenericYtpAudio(1))
				.SetItemComponent<ITM_SquareYTP>()
				.SetNameAndDescription("LtsOItems_SquareYtp_Name", "LtsOItems_SquareYtp_Desc")
				.Build();
			item.StoreAsNormal(appearsInStore: false, weight: 65, acceptableFloors: ["F1", "F2", "F3", "END"]);

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("squareRootYtp")
				.SetGeneratorCost(16)
				.SetEnum(Items.Points)
				.SetMeta(ItemFlags.InstantUse, [])
				.SetAsInstantUse()
				.SetPickupSound(GetGenericYtpAudio(0))
				.SetItemComponent<ITM_SquareRootYTP>()
				.SetNameAndDescription("LtsOItems_SquareRootYtp_Name", "LtsOItems_SquareRootYtp_Desc")
				.Build();
			item.StoreAsNormal(appearsInStore: false, weight: 35, acceptableFloors: ["F1", "F2", "F3", "END"]);

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("piYtp")
				.SetGeneratorCost(14)
				.SetEnum(Items.Points)
				.SetMeta(ItemFlags.InstantUse, [])
				.SetAsInstantUse()
				.SetPickupSound(GetGenericYtpAudio(1))
				.SetItemComponent<ITM_PiYTP>()
				.SetNameAndDescription("LtsOItems_PiYtp_Name", "LtsOItems_PiYtp_Desc")
				.Build();
			item.StoreAsNormal(appearsInStore: false, weight: 65, acceptableFloors: ["F2", "F3", "END"]);

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("DancingYTP")
				.SetGeneratorCost(15)
				.SetEnum(Items.Points)
				.SetMeta(ItemFlags.InstantUse, [])
				.SetAsInstantUse()
				.SetPickupSound(GetGenericYtpAudio(2))
				.SetItemComponent<ITM_DancingYTP>()
				.SetNameAndDescription("LtsOItems_DancingYtp_Name", "LtsOItems_DancingYtp_Desc")
				.BuildAndSetup<ITM_DancingYTP>(out var ytp);
			item.StoreAsNormal(appearsInStore: false, weight: 25, acceptableFloors: ["F1", "F2", "F3", "END"]);
			ytp.value = 65;

			// ---------------- TELEPORTERS ---------------

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("CalibratedTeleporter")
				.SetGeneratorCost(30)
				.SetShopPrice(750)
				.SetMeta(ItemFlags.None, [])
				.SetEnum("CalibratedTeleporter")
				.SetItemComponent<ITM_CalibratedTeleporter>()
				.SetNameAndDescription("LtsOItems_CalibratedTp_Name", "LtsOItems_CalibratedTp_Desc")
				.BuildAndSetup();
			item.StoreAsNormal(goToFieldTrips:true, appearsInStore: true, weight: 45, acceptableFloors: ["F2", "F3", "END"]);

			// ------------- EATABLES ---------------

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("chocolatePi")
				.SetGeneratorCost(18)
				.SetShopPrice(314)
				.SetMeta(ItemFlags.None, ["food"])
				.SetEnum("ChocolatePi")
				.SetItemComponent<ITM_GenericZestyEatable>()
				.SetNameAndDescription("LtsOItems_ChocolatePi_Name", "LtsOItems_ChocolatePi_Desc")
				.BuildAndSetup<ITM_GenericZestyEatable>(out var genericZesty);
			item.StoreAsNormal(appearsInStore: true, weight: 50, acceptableFloors: ["F1", "F2", "F3", "END"]);

			genericZesty.affectorTime = 31.4f;
			genericZesty.staminaGain = 31.4f;
			genericZesty.speedMultiplier = 1.314f;
		}

		static SoundObject GetYtpAudio(string name) 
		{
			var sd = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(LotOfItemsPlugin.ModPath, name)), string.Empty, SoundType.Effect, Color.white);
			sd.subtitle = false;
			return sd;
		}

		static SoundObject GetGenericYtpAudio(int id) =>
			GenericExtensions.FindResourceObjectByName<SoundObject>("YTPPickup_" + id);

		static ItemBuilder AutoGetSprites(this ItemBuilder bld, string itemName)
		{
			string smallPath = Path.Combine(LotOfItemsPlugin.ModPath, $"{itemName}_smallIcon.png"),
				largePath = Path.Combine(LotOfItemsPlugin.ModPath, $"{itemName}_largeIcon.png");

			Sprite bigIcon, smallIcon;
			bool smallIconExists = File.Exists(smallPath), bigIconExists = File.Exists(largePath);

			if (smallIconExists && bigIconExists)
			{
				bigIcon = AssetLoader.SpriteFromFile(largePath, Vector2.one * 0.5f, 50f);
				smallIcon = AssetLoader.SpriteFromFile(smallPath, Vector2.one * 0.5f, 50f);
				bld.SetSprites(smallIcon, bigIcon);
				return bld;
			}
			else if (bigIconExists) // In case it's a ytp, this should be the case
			{
				bigIcon = AssetLoader.SpriteFromFile(largePath, Vector2.one * 0.5f, 50f);
				smallIcon = bigIcon;
				bld.SetSprites(smallIcon, bigIcon);
				return bld;
			}
			else if (smallIconExists) // If it only exists to be given and not spawn naturally
			{
				smallIcon = AssetLoader.SpriteFromFile(smallPath, Vector2.one * 0.5f, 50f);
				bigIcon = smallIcon;
				bld.SetSprites(smallIcon, bigIcon);
				return bld;
			}

			throw new FileNotFoundException("Could not find a small or big icon of item: " + itemName);
		}

		static ItemObject BuildAndSetup(this ItemBuilder bld)
		{
			var itm = bld.Build();
			if (itm.item is IItemPrefab pre)
				pre.SetupPrefab(itm);
			return itm;
		}

		static ItemObject BuildAndSetup<T>(this ItemBuilder bld, out T item) where T : Item
		{
			var itm = bld.Build();
			if (itm.item is IItemPrefab pre)
				pre.SetupPrefab(itm);
			item = itm.item as T;
			return itm;
		}

		static ItemObject StoreAsNormal(this ItemObject itm, bool goToFieldTrips = false, bool appearsInStore = true, int weight = 100, params string[] acceptableFloors)
		{
			LotOfItemsPlugin.plug.availableItems.Add(new(itm, goToFieldTrips, appearsInStore, weight, acceptableFloors));
			return itm;
		}

		static ItemBuilder SetItemComponent<T>(this ItemBuilder bld, Items item, bool needACopyOfFields = true) where T : Item
		{
			var itm = ItemMetaStorage.Instance.FindByEnum(item).value.item;

			itm.gameObject.SetActive(false); // To make sure the prefab is disabled and no Awake() is called
			itm = Object.Instantiate(itm);
			itm.name = typeof(T).Name;

			var newItm = itm.gameObject.AddComponent<T>();
			if (needACopyOfFields)
				itm.ReplaceComponent<T, Item>();
			else
				Object.Destroy(itm);

			newItm.gameObject.ConvertToPrefab(true);

			return bld;
		}
	}

	internal readonly struct ItemData(ItemObject itm, int weight = 100, params string[] acceptableFloors)
	{
		internal ItemData(ItemObject itm, bool acceptFieldTrips, bool appearsInStore, int weight = 100, params string[] acceptableFloors) : this(itm, weight, acceptableFloors)
		{
			this.acceptFieldTrips = acceptFieldTrips;
			this.appearsInStore = appearsInStore;
		}
		
		readonly public ItemObject itm = itm;
		readonly public int weight = Mathf.Max(1, weight);
		readonly public IItemPrefab Prefab => itm.item is IItemPrefab pre ? pre : null;
		readonly public HashSet<string> acceptableFloors = [.. acceptableFloors];
		readonly public bool acceptFieldTrips = false, appearsInStore = true;
	}


}
