﻿using MTM101BaldAPI;
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
using LotsOfItems.CustomItems.PrincipalWhistles;
using LotsOfItems.CustomItems.NanaPeels;
using LotsOfItems.CustomItems.Nametags;
using LotsOfItems.CustomItems.ChalkErasers;

namespace LotsOfItems.Plugin
{
	internal static class TheItemBuilder
	{
		public static void StartBuilding()
		{
			const string 
				ZESTYVARTAG = "LtsOfItms_ZestyBar_Variant",
				YTPVARTAG = "LtsOfItms_YTP_Variant",
				DANGERTELEPORTERVARTAG = "LtsOfItms_DangerousTeleporter_Variant",
				PRINCIPALWHISTLEVARTAG = "LtsOfItms_PrincipalWhistle_Variant",
				QUARTERVARTAG = "LtsOfItms_Quarter_Variant",
				NANAPEELVARTAG = "LtsOfItms_NanaPeel_Variant",
				NAMETAGVARTAG = "LtsOfItms_Nametag_Variant",
				CHALKERASERVARTAG = "LtsOfItms_ChalkEraser_Variant",
				
				PIRATE_CANN_HATE = "cann_hate",
				CRIMINALPACK_CONTRABAND = "crmp_contraband";

			// ---------- YTPS
			var item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("squareYtp")
				.SetGeneratorCost(20)
				.SetEnum(Items.Points)
				.SetMeta(ItemFlags.InstantUse, [YTPVARTAG])
				.SetAsInstantUse()
				.SetPickupSound(GetGenericYtpAudio(1))
				.SetItemComponent<ITM_SquareYTP>()
				.SetNameAndDescription("LtsOItems_SquareYtp_Name", "LtsOItems_SquareYtp_Desc")
				.Build();
			item.StoreAsNormal(goToFieldTrips: true, appearsInStore: false, weight: 55, acceptableFloors: ["F1", "F2", "F3", "END"]);

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("squareRootYtp")
				.SetGeneratorCost(16)
				.SetEnum(Items.Points)
				.SetMeta(ItemFlags.InstantUse, [YTPVARTAG])
				.SetAsInstantUse()
				.SetPickupSound(GetGenericYtpAudio(0))
				.SetItemComponent<ITM_SquareRootYTP>()
				.SetNameAndDescription("LtsOItems_SquareRootYtp_Name", "LtsOItems_SquareRootYtp_Desc")
				.Build();
			item.StoreAsNormal(appearsInStore: false, weight: 65, acceptableFloors: ["F1", "F2", "F3", "END"]);

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("piYtp")
				.SetGeneratorCost(14)
				.SetEnum(Items.Points)
				.SetMeta(ItemFlags.InstantUse, [YTPVARTAG])
				.SetAsInstantUse()
				.SetPickupSound(GetGenericYtpAudio(1))
				.SetItemComponent<ITM_PiYTP>()
				.SetNameAndDescription("LtsOItems_PiYtp_Name", "LtsOItems_PiYtp_Desc")
				.Build();
			item.StoreAsNormal(appearsInStore: false, weight: 45, acceptableFloors: ["F2", "F3", "END"]);

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("DancingYTP")
				.SetGeneratorCost(15)
				.SetEnum(Items.Points)
				.SetMeta(ItemFlags.InstantUse, [YTPVARTAG])
				.SetAsInstantUse()
				.SetPickupSound(GetGenericYtpAudio(2))
				.SetItemComponent<ITM_DancingYTP>()
				.SetNameAndDescription("LtsOItems_DancingYtp_Name", "LtsOItems_DancingYtp_Desc")
				.BuildAndSetup<ITM_YTPs>(out var ytp);
			item.StoreAsNormal(appearsInStore: false, weight: 95, acceptableFloors: ["F1", "F2", "F3", "END"]);
			ytp.value = 65;

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("greenFakeYtp")
				.SetGeneratorCost(20)
				.SetEnum(Items.Points)
				.SetMeta(ItemFlags.InstantUse, [YTPVARTAG])
				.SetAsInstantUse()
				.SetPickupSound(GetYtpAudio("greenFakeYtp.wav"))
				.SetItemComponent<ITM_YTPs>()
				.SetNameAndDescription("LtsOItems_FakeGreenYtp_Name", "LtsOItems_FakeGreenYtp_Desc")
				.BuildAndSetup(out ytp);

			item.StoreAsNormal(appearsInStore: false, weight: 155, acceptableFloors: ["F1", "F2", "F3", "END"]);
			ytp.value = -25;

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("silverFakeYtp")
				.SetGeneratorCost(25)
				.SetEnum(Items.Points)
				.SetMeta(ItemFlags.InstantUse, [YTPVARTAG])
				.SetAsInstantUse()
				.SetPickupSound(GetYtpAudio("silverFakeYtp.wav"))
				.SetItemComponent<ITM_YTPs>()
				.SetNameAndDescription("LtsOItems_FakeSilverYtp_Name", "LtsOItems_FakeSilverYtp_Desc")
				.BuildAndSetup(out ytp);

			item.StoreAsNormal(appearsInStore: false, weight: 115, acceptableFloors: ["F2", "F3", "END"]);
			ytp.value = -50;

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("goldenFakeYtp")
				.SetGeneratorCost(28)
				.SetEnum(Items.Points)
				.SetMeta(ItemFlags.InstantUse, [YTPVARTAG])
				.SetAsInstantUse()
				.SetPickupSound(GetYtpAudio("goldenFakeYtp.wav"))
				.SetItemComponent<ITM_YTPs>()
				.SetNameAndDescription("LtsOItems_FakeGoldenYtp_Name", "LtsOItems_FakeGoldenYtp_Desc")
				.BuildAndSetup(out ytp);

			item.StoreAsNormal(appearsInStore: false, weight: 55, acceptableFloors: ["F2", "F3", "END"]);
			ytp.value = -100;

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("mysteryYtp")
				.SetGeneratorCost(13)
				.SetEnum(Items.Points)
				.SetMeta(ItemFlags.InstantUse, [YTPVARTAG])
				.SetAsInstantUse()
				.SetPickupSound(GetGenericYtpAudio(1))
				.SetItemComponent<ITM_MysteryYTP>()
				.SetNameAndDescription("LtsOItems_MysteryYtp_Name", "LtsOItems_MysteryYtp_Desc")
				.BuildAndSetup();

			item.StoreAsNormal(appearsInStore: false, weight: 45, acceptableFloors: ["F2", "F3", "END"]);

			// ------------- EATABLES ---------------

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("chocolatePi")
				.SetGeneratorCost(18)
				.SetShopPrice(314)
				.SetMeta(ItemFlags.Persists, ["food", ZESTYVARTAG, PIRATE_CANN_HATE])
				.SetEnum("ChocolatePi")
				.SetItemComponent<ITM_GenericZestyEatable>()
				.SetNameAndDescription("LtsOItems_ChocolatePi_Name", "LtsOItems_ChocolatePi_Desc")
				.BuildAndSetup<ITM_GenericZestyEatable>(out var genericZesty);
			item.StoreAsNormal(appearsInStore: true, weight: 125, acceptableFloors: ["F1", "F2", "F3", "END"]);

			genericZesty.affectorTime = 31.4f;
			genericZesty.staminaGain = 31.4f;
			genericZesty.speedMultiplier = 1.314f;

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("Pizza")
				.SetGeneratorCost(15)
				.SetShopPrice(175)
				.SetMeta(ItemFlags.None, ["food", ZESTYVARTAG])
				.SetEnum("Pizza")
				.SetItemComponent<ITM_GenericZestyEatable>()
				.SetNameAndDescription("LtsOItems_Pizza_Name", "LtsOItems_Pizza_Desc")
				.BuildAndSetup(out genericZesty);
			item.StoreAsNormal(appearsInStore: true, weight: 115, acceptableFloors: ["F1", "F2", "F3", "END"]);

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("pickles")
				.SetGeneratorCost(25)
				.SetShopPrice(575)
				.SetMeta(ItemFlags.MultipleUse | ItemFlags.Persists, ["food", ZESTYVARTAG, CRIMINALPACK_CONTRABAND])
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
				.SetMeta(ItemFlags.Persists, ["food", ZESTYVARTAG, PIRATE_CANN_HATE, CRIMINALPACK_CONTRABAND])
				.SetEnum("HotCrossBun")
				.SetItemComponent<ITM_HotCrossBun>()
				.SetNameAndDescription("LtsOItems_HotCrossBun_Name", "LtsOItems_HotCrossBun_Desc")
				.BuildAndSetup<ITM_HotCrossBun>(out _);
			item.StoreAsNormal(appearsInStore: true, weight: 86, acceptableFloors: ["F1", "F2", "F3", "END"]);

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("orangeJuice")
				.SetGeneratorCost(30)
				.SetShopPrice(225)
				.SetMeta(ItemFlags.Persists, ["food", "drink", ZESTYVARTAG])
				.SetEnum("OrangeJuice")
				.SetItemComponent<ITM_GenericZestyEatable>()
				.SetNameAndDescription("LtsOItems_OrangeJuice_Name", "LtsOItems_OrangeJuice_Desc")
				.BuildAndSetup(out genericZesty);
			item.StoreAsNormal(appearsInStore: true, weight: 65, acceptableFloors: ["F1", "F2", "F3", "END"]);

			genericZesty.affectorTime = 15f;
			genericZesty.staminaRiseChanger = 2f;
			genericZesty.staminaDropChanger = 1.25f;

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("chocolateQuarter")
				.SetGeneratorCost(26)              
				.SetShopPrice(350)                 
				.SetMeta(ItemFlags.None, ["food", ZESTYVARTAG, QUARTERVARTAG, "currency", PIRATE_CANN_HATE])
				.SetEnum("ChocolateQuarter")
				.SetItemComponent<ITM_ChocolateQuarter>()
				.SetNameAndDescription("LtsOItems_ChocolateQuarter_Name", "LtsOItems_ChocolateQuarter_Desc")
				.BuildAndSetup();
			item.StoreAsNormal(appearsInStore: true, weight: 75, acceptableFloors: ["F1", "F2", "F3", "END" ]);

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("vanillaZestyBar")
			.SetGeneratorCost(29)
			.SetShopPrice(425)
			.SetMeta(ItemFlags.Persists, ["food", ZESTYVARTAG, PIRATE_CANN_HATE])
			.SetEnum("VanillaZestyBar")
			.SetItemComponent<ITM_VanillaZestyBar>()
			.SetNameAndDescription("LtsOItems_VanillaZestyBar_Name", "LtsOItems_VanillaZestyBar_Desc")
			.BuildAndSetup(out genericZesty);
			item.StoreAsNormal(appearsInStore: true, weight: 86, acceptableFloors: ["F2", "F3", "END"]);
			genericZesty.affectorTime = 10f;
			genericZesty.speedMultiplier = 1.1f;
			genericZesty.maxMultiplier = 3.5f;

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("iceZestyBar")
			.SetGeneratorCost(29)
			.SetShopPrice(500)
			.SetMeta(ItemFlags.Persists, ["food", ZESTYVARTAG, PIRATE_CANN_HATE, CRIMINALPACK_CONTRABAND])
			.SetEnum("IceZestyBar")
			.SetItemComponent<ITM_IceZestyBar>()
			.SetNameAndDescription("LtsOItems_IceZestyBar_Name", "LtsOItems_IceZestyBar_Desc")
			.BuildAndSetup(out genericZesty);
			genericZesty.maxMultiplier = 1.5f;
			item.StoreAsNormal(goToFieldTrips: true,  appearsInStore: true, weight: 35, acceptableFloors: ["F2", "F3", "END"]);

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("chocolateFlavouredZestyBar")
			.SetGeneratorCost(25)
			.SetShopPrice(600)
			.SetMeta(ItemFlags.Persists, ["food", ZESTYVARTAG, PIRATE_CANN_HATE])
			.SetEnum("ChocolateFlavouredZestyBar")
			.SetItemComponent<ITM_ChocolateFlavouredZestyBar>()
			.SetNameAndDescription("LtsOItems_ChocolateFlavouredZestyBar_Name", "LtsOItems_ChocolateFlavouredZestyBar_Desc")
			.BuildAndSetup(out genericZesty);
			genericZesty.staminaSet = 200;

			item.StoreAsNormal(appearsInStore: true, weight: 75, acceptableFloors: ["F1", "F2", "F3", "END"]);

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("dietFlavouredZestyBar")
			.SetGeneratorCost(18)
			.SetShopPrice(175)
			.SetMeta(ItemFlags.None, ["food", ZESTYVARTAG, PIRATE_CANN_HATE])
			.SetEnum("DietFlavouredZestyBar")
			.SetItemComponent<ITM_GenericZestyEatable>()
			.SetNameAndDescription("LtsOItems_DietFlavouredZestyBar_Name", "LtsOItems_DietFlavouredZestyBar_Desc")
			.BuildAndSetup(out genericZesty);
			item.StoreAsNormal(appearsInStore: true, weight: 175, acceptableFloors: ["F1", "F2", "F3", "END"]);

			genericZesty.staminaGain = 50;

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("Chalk") // expects Chalk_smallIcon.png and Chalk_smallIcon.png
			.SetGeneratorCost(25)
			.SetShopPrice(165)
			.SetMeta(ItemFlags.Persists, ["food", ZESTYVARTAG, CHALKERASERVARTAG])
			.SetEnum("Chalk")
			.SetItemComponent<ITM_Chalk>()
			.SetNameAndDescription("LtsOItems_Chalk_Name", "LtsOItems_Chalk_Desc")
			.BuildAndSetup(out genericZesty);
			item.StoreAsNormal(appearsInStore: true, weight: 155, acceptableFloors: ["F1", "F2", "F3", "END"]);

			genericZesty.maxMultiplier = 0.5f;





			// ---------------- TELEPORTERS ---------------

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("CalibratedTeleporter")
				.SetGeneratorCost(30)
				.SetShopPrice(750)
				.SetMeta(ItemFlags.None, [DANGERTELEPORTERVARTAG, CRIMINALPACK_CONTRABAND])
				.SetEnum("CalibratedTeleporter")
				.SetItemComponent<ITM_CalibratedTeleporter>()
				.SetNameAndDescription("LtsOItems_CalibratedTp_Name", "LtsOItems_CalibratedTp_Desc")
				.BuildAndSetup();
			item.StoreAsNormal(goToFieldTrips: true, appearsInStore: true, weight: 76, acceptableFloors: ["F2", "F3", "END"]);


			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("SaferTeleporter")
			.SetGeneratorCost(37)
			.SetShopPrice(850)
			.SetMeta(ItemFlags.Persists, [DANGERTELEPORTERVARTAG])
			.SetEnum("SaferTeleporter")
			.SetItemComponent<ITM_SaferTeleporter>()
			.SetNameAndDescription("LtsOItems_SaferTeleporter_Name", "LtsOItems_SaferTeleporter_Desc")
			.BuildAndSetup<ITM_GenericTeleporter>(out var genericTp);
			item.StoreAsNormal(goToFieldTrips: true, appearsInStore: true, weight: 35, acceptableFloors: ["F2", "F3", "END"]);

			genericTp.minTeleports = 1;
			genericTp.maxTeleports = 1;
			genericTp.baseTime = 0.035f;

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("ControlledTeleporter")
			.SetGeneratorCost(40)
			.SetShopPrice(1000)
			.SetMeta(ItemFlags.Persists, [DANGERTELEPORTERVARTAG, CRIMINALPACK_CONTRABAND])
			.SetEnum("ControlledTeleporter")
			.SetItemComponent<ITM_ControlledTeleporter>()
			.SetNameAndDescription("LtsOItems_ControlledTeleporter_Name", "LtsOItems_ControlledTeleporter_Desc")
			.BuildAndSetup(out genericTp);
			item.StoreAsNormal(goToFieldTrips: true, appearsInStore: true, weight: 23, acceptableFloors: ["F2", "F3", "END"]);

			genericTp.minTeleports = 1;
			genericTp.maxTeleports = 1;
			genericTp.baseTime = 0.035f;

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("BananaTeleporter")
			.SetGeneratorCost(40)
			.SetShopPrice(1300)
			.SetMeta(ItemFlags.Persists, [DANGERTELEPORTERVARTAG, CRIMINALPACK_CONTRABAND])
			.SetEnum("BananaTeleporter")
			.SetItemComponent<ITM_BananaTeleporter>()
			.SetNameAndDescription("LtsOItems_BananaTeleporter_Name", "LtsOItems_BananaTeleporter_Desc")
			.BuildAndSetup(out genericTp);
			item.StoreAsNormal(goToFieldTrips: true, appearsInStore: true, weight: 25, acceptableFloors: ["F2", "F3", "END"]);

			genericTp.minTeleports = 7;
			genericTp.maxTeleports = 12;
			genericTp.baseTime = 0.08f;
			genericTp.increaseFactor = 1.26f;

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("DetentionTeleporter") // expects DetentionTeleporter_smallIcon.png and DetentionTeleporter_largeIcon.png
			.SetGeneratorCost(40)
			.SetShopPrice(1100)
			.SetMeta(ItemFlags.Persists, [DANGERTELEPORTERVARTAG, CRIMINALPACK_CONTRABAND])
			.SetEnum("DetentionTeleporter")
			.SetItemComponent<ITM_DetentionTeleporter>()
			.SetNameAndDescription("LtsOItems_DetentionTeleporter_Name", "LtsOItems_DetentionTeleporter_Desc")
			.BuildAndSetup(out genericTp);
			item.StoreAsNormal(appearsInStore: true, weight: 100, acceptableFloors: ["F2", "F3", "END"]);

			genericTp.minTeleports = 1;
			genericTp.maxTeleports = 1;
			genericTp.baseTime = 0.035f;


			// ---------- WHISTLES VARIANTS ----------
			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("PrincipalOEleporter")
			.SetGeneratorCost(16)
			.SetShopPrice(350)
			.SetMeta(ItemFlags.Persists, [PRINCIPALWHISTLEVARTAG, CRIMINALPACK_CONTRABAND])
			.SetEnum("PrincipalOEleporter")
			.SetItemComponent<ITM_PrincipalOEleporter>()
			.SetNameAndDescription("LtsOItems_PrincipalOEleporter_Name", "LtsOItems_PrincipalOEleporter_Desc")
			.BuildAndSetup();
			item.StoreAsNormal(appearsInStore: true, weight: 96, acceptableFloors: ["F2", "F3", "END"]);


			// ---------- Nanapeel VARIANTS ---------

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("TeleporterOWall")
			.SetGeneratorCost(27)
			.SetShopPrice(650)
			.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, [NANAPEELVARTAG, DANGERTELEPORTERVARTAG])
			.SetEnum("TeleporterOWall")
			.SetItemComponent<ITM_TeleporterOWall>(Items.NanaPeel) // Makes a new exact copy of ITM_NanaPeel
			.SetNameAndDescription("LtsOItems_TeleporterOWall_Name", "LtsOItems_TeleporterOWall_Desc")
			.BuildAndSetup();
			item.StoreAsNormal(appearsInStore: true, weight: 35, acceptableFloors: ["F2", "F3", "END"]);


			// ------------ QUARTERS -------------

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("quarterOnAString")
			.SetGeneratorCost(35)
			.SetShopPrice(675)
			.SetMeta(ItemFlags.MultipleUse | ItemFlags.Persists, [QUARTERVARTAG, CRIMINALPACK_CONTRABAND])
			.SetEnum("QuarterOnAString")
			.SetItemComponent<ITM_QuarterOnAString>()
			.SetNameAndDescription("LtsOItems_QuarterOnAString_Name", "LtsOItems_QuarterOnAString_Desc")
			.BuildAndSetup<ITM_QuarterOnAString>(out var quarterOnAString);
			item.StoreAsNormal(goToFieldTrips: true, appearsInStore: true, weight: 55, acceptableFloors: ["F2", "F3", "END"]);

			quarterOnAString.CreateNewReusableInstances(item, "LtsOItems_QuarterOnAString_Name", 2);

			// ---------- NAME TAGS -----------

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("EnergyNametag")
			.SetGeneratorCost(26)
			.SetShopPrice(600)
			.SetMeta(ItemFlags.Persists, [NAMETAGVARTAG])
			.SetEnum("EnergyNametag")
			.SetItemComponent<ITM_EnergyNametag>(Items.Nametag)
			.SetNameAndDescription("LtsOItems_EnergyNametag_Name", "LtsOItems_EnergyNametag_Desc")
			.BuildAndSetup();
			item.StoreAsNormal(goToFieldTrips: true, appearsInStore: true, weight: 75, acceptableFloors: ["F1", "F2", "F3"]);

			// --------- Chalk Erasers ---------
			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("FogMachine") // expects FogMachine_smallIcon.png and FogMachine_largeIcon.png
			.SetGeneratorCost(34)
			.SetShopPrice(750)
			.SetMeta(ItemFlags.Persists, [CHALKERASERVARTAG, CRIMINALPACK_CONTRABAND])
			.SetEnum("FogMachine")
			.SetItemComponent<ITM_FogMachine>()
			.SetNameAndDescription("LtsOItems_FogMachine_Name", "LtsOItems_FogMachine_Desc")
			.BuildAndSetup();
			item.StoreAsNormal(appearsInStore: true, weight: 65, acceptableFloors: ["F2", "F3", "END"]);

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("ChalkBomb")
			.SetGeneratorCost(37)
			.SetShopPrice(850)
			.SetMeta(ItemFlags.CreatesEntity | ItemFlags.Persists, [CHALKERASERVARTAG, CRIMINALPACK_CONTRABAND])
			.SetEnum("ChalkBomb")
			.SetItemComponent<ITM_ChalkBomb>(Items.NanaPeel)
			.SetNameAndDescription("LtsOItems_ChalkBomb_Name", "LtsOItems_ChalkBomb_Desc")
			.BuildAndSetup();
			item.StoreAsNormal(appearsInStore: true, weight: 35, acceptableFloors: ["F1", "F2", "F3", "END"]);



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

		static ItemBuilder SetItemComponent<T>(this ItemBuilder bld, Items item) where T : Item
		{
			var ogItem = ItemMetaStorage.Instance.FindByEnum(item).value.item;

			ogItem.gameObject.SetActive(false); // To make sure the prefab is disabled and no Awake() is called
			var itm = Object.Instantiate(ogItem);
			itm.name = typeof(T).Name;

			ogItem.gameObject.SetActive(true); // Forgot about this lol

			var newItm = itm.gameObject.AddComponent<T>()
				.GetACopyFromFields(itm);
			Object.Destroy(itm);

			newItm.gameObject.ConvertToPrefab(true);

			bld.SetItemComponent(newItm); // To make sure it doesn't create a new one lol

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
