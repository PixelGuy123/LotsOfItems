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
using LotsOfItems.CustomItems.Eatables;
using LotsOfItems.CustomItems.Quarters;

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
			item.StoreAsNormal(goToFieldTrips: true, appearsInStore: false, weight: 65, acceptableFloors: ["F1", "F2", "F3", "END"]);

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
				.BuildAndSetup<ITM_YTPs>(out var ytp);
			item.StoreAsNormal(appearsInStore: false, weight: 25, acceptableFloors: ["F1", "F2", "F3", "END"]);
			ytp.value = 65;

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("greenFakeYtp")
				.SetGeneratorCost(20)
				.SetEnum(Items.Points)
				.SetMeta(ItemFlags.InstantUse, [])
				.SetAsInstantUse()
				.SetPickupSound(GetYtpAudio("greenFakeYtp.wav"))
				.SetItemComponent<ITM_YTPs>()
				.SetNameAndDescription("LtsOItems_FakeGreenYtp_Name", "LtsOItems_FakeGreenYtp_Desc")
				.BuildAndSetup(out ytp);

			item.StoreAsNormal(appearsInStore: false, weight: 76, acceptableFloors: ["F1", "F2", "F3", "END"]);
			ytp.value = -25;

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("silverFakeYtp")
				.SetGeneratorCost(25)
				.SetEnum(Items.Points)
				.SetMeta(ItemFlags.InstantUse, [])
				.SetAsInstantUse()
				.SetPickupSound(GetYtpAudio("silverFakeYtp.wav"))
				.SetItemComponent<ITM_YTPs>()
				.SetNameAndDescription("LtsOItems_FakeSilverYtp_Name", "LtsOItems_FakeSilverYtp_Desc")
				.BuildAndSetup(out ytp);

			item.StoreAsNormal(appearsInStore: false, weight: 55, acceptableFloors: ["F2", "F3", "END"]);
			ytp.value = -50;

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("goldenFakeYtp")
				.SetGeneratorCost(28)
				.SetEnum(Items.Points)
				.SetMeta(ItemFlags.InstantUse, [])
				.SetAsInstantUse()
				.SetPickupSound(GetYtpAudio("goldenFakeYtp.wav"))
				.SetItemComponent<ITM_YTPs>()
				.SetNameAndDescription("LtsOItems_FakeGoldenYtp_Name", "LtsOItems_FakeGoldenYtp_Desc")
				.BuildAndSetup(out ytp);

			item.StoreAsNormal(appearsInStore: false, weight: 35, acceptableFloors: ["F2", "F3", "END"]);
			ytp.value = -100;

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("mysteryYtp")
				.SetGeneratorCost(13)
				.SetEnum(Items.Points)
				.SetMeta(ItemFlags.InstantUse, [])
				.SetAsInstantUse()
				.SetPickupSound(GetGenericYtpAudio(1))
				.SetItemComponent<ITM_MysteryYTP>()
				.SetNameAndDescription("LtsOItems_MysteryYtp_Name", "LtsOItems_MysteryYtp_Desc")
				.BuildAndSetup();

			item.StoreAsNormal(appearsInStore: false, weight: 25, acceptableFloors: ["F2", "F3", "END"]);

			// ------------- EATABLES ---------------

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("chocolatePi")
				.SetGeneratorCost(18)
				.SetShopPrice(314)
				.SetMeta(ItemFlags.Persists, ["food"])
				.SetEnum("ChocolatePi")
				.SetItemComponent<ITM_GenericZestyEatable>()
				.SetNameAndDescription("LtsOItems_ChocolatePi_Name", "LtsOItems_ChocolatePi_Desc")
				.BuildAndSetup<ITM_GenericZestyEatable>(out var genericZesty);
			item.StoreAsNormal(appearsInStore: true, weight: 50, acceptableFloors: ["F1", "F2", "F3", "END"]);

			genericZesty.affectorTime = 31.4f;
			genericZesty.staminaGain = 31.4f;
			genericZesty.speedMultiplier = 1.314f;

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("Pizza")
				.SetGeneratorCost(15)
				.SetShopPrice(175)
				.SetMeta(ItemFlags.None, ["food"])
				.SetEnum("Pizza")
				.SetItemComponent<ITM_GenericZestyEatable>()
				.SetNameAndDescription("LtsOItems_Pizza_Name", "LtsOItems_Pizza_Desc")
				.BuildAndSetup(out genericZesty);
			item.StoreAsNormal(appearsInStore: true, weight: 60, acceptableFloors: ["F1", "F2", "F3", "END"]);

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("pickles")
				.SetGeneratorCost(25)
				.SetShopPrice(575)
				.SetMeta(ItemFlags.MultipleUse | ItemFlags.Persists, ["food"])
				.SetEnum("JarOfPickles")
				.SetItemComponent<ITM_Reusable_GenericZestyEatable>()
				.SetNameAndDescription("LtsOItems_Pickles_Name", "LtsOItems_Pickles_Desc")
				.BuildAndSetup<ITM_Reusable_GenericZestyEatable>(out var genericReusableZesty);
			item.StoreAsNormal(goToFieldTrips: true, appearsInStore: true, weight: 45, acceptableFloors: ["F1", "F2", "F3", "END"]);

			genericReusableZesty.maxStaminaLimit = 300f;
			genericReusableZesty.staminaMaxChanger = 50f;
			genericReusableZesty.affectorTime = 30f;

			genericReusableZesty.CreateNewReusableInstances(item, "LtsOItems_Pickles_Name", 3);

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("HotCrossBun")
				.SetGeneratorCost(35)
				.SetShopPrice(600)
				.SetMeta(ItemFlags.Persists, ["food"])
				.SetEnum("HotCrossBun")
				.SetItemComponent<ITM_HotCrossBun>()
				.SetNameAndDescription("LtsOItems_HotCrossBun_Name", "LtsOItems_HotCrossBun_Desc")
				.BuildAndSetup<ITM_HotCrossBun>(out _);
			item.StoreAsNormal(appearsInStore: true, weight: 30, acceptableFloors: ["F1", "F2", "F3", "END"]);

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("orangeJuice")
				.SetGeneratorCost(30)
				.SetShopPrice(225)
				.SetMeta(ItemFlags.Persists, ["food"])
				.SetEnum("OrangeJuice")
				.SetItemComponent<ITM_GenericZestyEatable>()
				.SetNameAndDescription("LtsOItems_OrangeJuice_Name", "LtsOItems_OrangeJuice_Desc")
				.BuildAndSetup(out genericZesty);
			item.StoreAsNormal(appearsInStore: true, weight: 30, acceptableFloors: ["F1", "F2", "F3", "END"]);

			genericZesty.affectorTime = 15f;
			genericZesty.staminaRiseChanger = 2f;
			genericZesty.staminaDropChanger = 1.25f;

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("chocolateQuarter")
				.SetGeneratorCost(26)              
				.SetShopPrice(350)                 
				.SetMeta(ItemFlags.None, ["food"])
				.SetEnum("ChocolateQuarter")
				.SetItemComponent<ITM_ChocolateQuarter>()
				.SetNameAndDescription("LtsOItems_ChocolateQuarter_Name", "LtsOItems_ChocolateQuarter_Desc")
				.BuildAndSetup();
			item.StoreAsNormal(appearsInStore: true, weight: 35, acceptableFloors: ["F1", "F2", "F3", "END" ]);

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("vanillaZestyBar")
			.SetGeneratorCost(29)
			.SetShopPrice(425)
			.SetMeta(ItemFlags.Persists, ["food"])
			.SetEnum("VanillaZestyBar")
			.SetItemComponent<ITM_VanillaZestyBar>()
			.SetNameAndDescription("LtsOItems_VanillaZestyBar_Name", "LtsOItems_VanillaZestyBar_Desc")
			.BuildAndSetup(out genericZesty);
			item.StoreAsNormal(appearsInStore: true, weight: 50, acceptableFloors: ["F2", "F3", "END"]);
			genericZesty.affectorTime = 10f;
			genericZesty.speedMultiplier = 1.1f;
			genericZesty.maxMultiplier = 3.5f;

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("iceZestyBar")
			.SetGeneratorCost(29)
			.SetShopPrice(500)
			.SetMeta(ItemFlags.Persists, ["food"])
			.SetEnum("IceZestyBar")
			.SetItemComponent<ITM_IceZestyBar>()
			.SetNameAndDescription("LtsOItems_IceZestyBar_Name", "LtsOItems_IceZestyBar_Desc")
			.BuildAndSetup(out genericZesty);
			genericZesty.maxMultiplier = 1.5f;
			item.StoreAsNormal(goToFieldTrips: true,  appearsInStore: true, weight: 50, acceptableFloors: ["F2", "F3", "END"]);

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("chocolateFlavouredZestyBar")
			.SetGeneratorCost(25)
			.SetShopPrice(600)
			.SetMeta(ItemFlags.Persists, ["food"])
			.SetEnum("ChocolateFlavouredZestyBar")
			.SetItemComponent<ITM_ChocolateFlavouredZestyBar>()
			.SetNameAndDescription("LtsOItems_ChocolateFlavouredZestyBar_Name", "LtsOItems_ChocolateFlavouredZestyBar_Desc")
			.BuildAndSetup(out genericZesty);
			genericZesty.staminaSet = 200;

			item.StoreAsNormal(appearsInStore: true, weight: 35, acceptableFloors: ["F1", "F2", "F3", "END"]);

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("dietFlavouredZestyBar")
			.SetGeneratorCost(18)
			.SetShopPrice(175)
			.SetMeta(ItemFlags.None, ["food"])
			.SetEnum("DietFlavouredZestyBar")
			.SetItemComponent<ITM_GenericZestyEatable>()
			.SetNameAndDescription("LtsOItems_DietFlavouredZestyBar_Name", "LtsOItems_DietFlavouredZestyBar_Desc")
			.BuildAndSetup(out genericZesty);
			item.StoreAsNormal(appearsInStore: true, weight: 135, acceptableFloors: ["F1", "F2", "F3", "END"]);

			genericZesty.staminaGain = 50;




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
			item.StoreAsNormal(goToFieldTrips: true, appearsInStore: true, weight: 45, acceptableFloors: ["F2", "F3", "END"]);


			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("SaferTeleporter")
			.SetGeneratorCost(37)
			.SetShopPrice(1000)
			.SetMeta(ItemFlags.Persists, [])
			.SetEnum("SaferTeleporter")
			.SetItemComponent<ITM_SaferTeleporter>()
			.SetNameAndDescription("LtsOItems_SaferTeleporter_Name", "LtsOItems_SaferTeleporter_Desc")
			.BuildAndSetup<ITM_GenericTeleporter>(out var genericTp);
			item.StoreAsNormal(goToFieldTrips: true, appearsInStore: true, weight: 25, acceptableFloors: ["F2", "F3", "END"]);

			genericTp.minTeleports = 1;
			genericTp.maxTeleports = 1;
			genericTp.baseTime = 0.1f;


			// ------------ QUARTERS -------------

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("quarterOnAString")
			.SetGeneratorCost(35)
			.SetShopPrice(675)
			.SetMeta(ItemFlags.MultipleUse | ItemFlags.Persists, [])
			.SetEnum("QuarterOnAString")
			.SetItemComponent<ITM_QuarterOnAString>()
			.SetNameAndDescription("LtsOItems_QuarterOnAString_Name", "LtsOItems_QuarterOnAString_Desc")
			.BuildAndSetup<ITM_QuarterOnAString>(out var quarterOnAString);
			item.StoreAsNormal(goToFieldTrips: true, appearsInStore: true, weight: 45, acceptableFloors: ["F2", "F3", "END"]);

			quarterOnAString.CreateNewReusableInstances(item, "LtsOItems_QuarterOnAString_Name", 2);
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
			var itm = bld.BuildAndSetup();
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
