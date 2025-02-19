using HarmonyLib;
using LotsOfItems.Components;
using LotsOfItems.CustomItems;
using LotsOfItems.CustomItems.AlarmClock;
using LotsOfItems.CustomItems.Boots;
using LotsOfItems.CustomItems.BSODAs;
using LotsOfItems.CustomItems.ChalkErasers;
using LotsOfItems.CustomItems.Eatables;
using LotsOfItems.CustomItems.GrapplingHooks;
using LotsOfItems.CustomItems.Key;
using LotsOfItems.CustomItems.Nametags;
using LotsOfItems.CustomItems.NanaPeels;
using LotsOfItems.CustomItems.PortalPosters;
using LotsOfItems.CustomItems.PrincipalWhistles;
using LotsOfItems.CustomItems.Quarters;
using LotsOfItems.CustomItems.Scissors;
using LotsOfItems.CustomItems.SwingingDoorLocks;
using LotsOfItems.CustomItems.Tapes;
using LotsOfItems.CustomItems.Teleporters;
using LotsOfItems.CustomItems.YTPs;
using LotsOfItems.ItemPrefabStructures;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;
using PixelInternalAPI;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

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
				KEYVARTAG = "LtsOfItms_DetentionKey_Variant",
				SCISSORSVARTAG = "LtsOfItms_SafetyScissors_Variant",
				DOORLOCKVARTAG = "LtsOfItms_SwingingDoorLock_Variant",
				BOOTSVARTAG = "LtsOfItms_Boots_Variant",
				ALARMCLOCKVARTAG = "LtsOfItms_AlarmClock_Variant",
				GRAPPLEVARTAG = "LtsOfItms_GrappleHook_Variant",
				TAPEVARTAG = "LtsOfItms_Tape_Variant",
				PORTALVARTAG = "LtsOfItms_Portal_Variant",
				SODAVARTAG = "LtsOfItms_Bsoda_Variant",
				APPLEVARTAG = "LtsOfItms_Apple_Variant",

				PIRATE_CANN_HATE = "cann_hate",
				CRIMINALPACK_CONTRABAND = "crmp_contraband";

			// ---------- YTPS
			new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("squareYtp")
				.SetGeneratorCost(20)
				.SetEnum(Items.Points)
				.SetMeta(ItemFlags.InstantUse, [YTPVARTAG])
				.SetAsInstantUse()
				.SetPickupSound(GetGenericYtpAudio(1))
				.SetItemComponent<ITM_SquareYTP>()
				.SetNameAndDescription("LtsOItems_SquareYtp_Name", "LtsOItems_SquareYtp_Desc")
				.Build()
				.StoreAsNormal(Items.Points, goToFieldTrips: true, appearsInStore: false, weight: 55, acceptableFloors: ["F1", "F2", "F3", "END"]);

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("squareRootYtp")
				.SetGeneratorCost(16)
				.SetEnum(Items.Points)
				.SetMeta(ItemFlags.InstantUse, [YTPVARTAG])
				.SetAsInstantUse()
				.SetPickupSound(GetGenericYtpAudio(0))
				.SetItemComponent<ITM_SquareRootYTP>()
				.SetNameAndDescription("LtsOItems_SquareRootYtp_Name", "LtsOItems_SquareRootYtp_Desc")
				.Build()
				.StoreAsNormal(Items.Points, appearsInStore: false, weight: 65, acceptableFloors: ["F1", "F2", "F3", "END"]);

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("piYtp")
				.SetGeneratorCost(14)
				.SetEnum(Items.Points)
				.SetMeta(ItemFlags.InstantUse, [YTPVARTAG])
				.SetAsInstantUse()
				.SetPickupSound(GetGenericYtpAudio(1))
				.SetItemComponent<ITM_PiYTP>()
				.SetNameAndDescription("LtsOItems_PiYtp_Name", "LtsOItems_PiYtp_Desc")
				.Build()
				.StoreAsNormal(Items.Points, appearsInStore: false, weight: 45, acceptableFloors: ["F2", "F3", "END"]);

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("DancingYTP")
				.SetGeneratorCost(15)
				.SetEnum(Items.Points)
				.SetMeta(ItemFlags.InstantUse, [YTPVARTAG])
				.SetAsInstantUse()
				.SetPickupSound(GetGenericYtpAudio(2))
				.SetItemComponent<ITM_DancingYTP>()
				.SetNameAndDescription("LtsOItems_DancingYtp_Name", "LtsOItems_DancingYtp_Desc")
				.BuildAndSetup<ITM_YTPs>(out var ytp)
				.StoreAsNormal(Items.Points, goToFieldTrips: true, appearsInStore: false, weight: 95, acceptableFloors: ["F1", "F2", "F3", "END"]);
			ytp.value = 125;

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("greenFakeYtp")
				.SetGeneratorCost(20)
				.SetEnum(Items.Points)
				.SetMeta(ItemFlags.InstantUse, [YTPVARTAG])
				.SetAsInstantUse()
				.SetPickupSound(GetYtpAudio("greenFakeYtp.wav"))
				.SetItemComponent<ITM_YTPs>()
				.SetNameAndDescription("LtsOItems_FakeGreenYtp_Name", "LtsOItems_FakeGreenYtp_Desc")
				.BuildAndSetup(out ytp)
				.StoreAsNormal(Items.Points, appearsInStore: false, weight: 155, acceptableFloors: ["F1", "F2", "F3", "END"]);

			ytp.value = -25;

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("silverFakeYtp")
				.SetGeneratorCost(25)
				.SetEnum(Items.Points)
				.SetMeta(ItemFlags.InstantUse, [YTPVARTAG])
				.SetAsInstantUse()
				.SetPickupSound(GetYtpAudio("silverFakeYtp.wav"))
				.SetItemComponent<ITM_YTPs>()
				.SetNameAndDescription("LtsOItems_FakeSilverYtp_Name", "LtsOItems_FakeSilverYtp_Desc")
				.BuildAndSetup(out ytp)
				.StoreAsNormal(Items.Points, appearsInStore: false, weight: 115, acceptableFloors: ["F2", "F3", "END"]);
			ytp.value = -50;

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("goldenFakeYtp")
				.SetGeneratorCost(28)
				.SetEnum(Items.Points)
				.SetMeta(ItemFlags.InstantUse, [YTPVARTAG])
				.SetAsInstantUse()
				.SetPickupSound(GetYtpAudio("goldenFakeYtp.wav"))
				.SetItemComponent<ITM_YTPs>()
				.SetNameAndDescription("LtsOItems_FakeGoldenYtp_Name", "LtsOItems_FakeGoldenYtp_Desc")
				.BuildAndSetup(out ytp)
				.StoreAsNormal(Items.Points, appearsInStore: false, weight: 55, acceptableFloors: ["F2", "F3", "END"]);
			ytp.value = -100;

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("mysteryYtp")
				.SetGeneratorCost(13)
				.SetEnum(Items.Points)
				.SetMeta(ItemFlags.InstantUse, [YTPVARTAG])
				.SetAsInstantUse()
				.SetPickupSound(GetGenericYtpAudio(1))
				.SetItemComponent<ITM_MysteryYTP>()
				.SetNameAndDescription("LtsOItems_MysteryYtp_Name", "LtsOItems_MysteryYtp_Desc")
				.BuildAndSetup()
				.StoreAsNormal(Items.Points, appearsInStore: false, weight: 45, acceptableFloors: ["F2", "F3", "END"]);

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("approximetalyYtp")
				.SetGeneratorCost(13)
				.SetEnum(Items.Points)
				.SetMeta(ItemFlags.InstantUse, [YTPVARTAG])
				.SetAsInstantUse()
				.SetPickupSound(GetGenericYtpAudio(2))
				.SetItemComponent<ITM_MysteryYTP>()
				.SetNameAndDescription("LtsOItems_ApproximetalyYtp_Name", "LtsOItems_ApproximetalyYtp_Desc")
				.BuildAndSetup()
				.StoreAsNormal(Items.Points, appearsInStore: false, weight: 45, acceptableFloors: ["F2", "F3", "END"]);

			// ------------- EATABLES ---------------

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("chocolatePi")
				.SetGeneratorCost(18)
				.SetShopPrice(314)
				.SetMeta(ItemFlags.Persists, ["food", ZESTYVARTAG, PIRATE_CANN_HATE])
				.SetEnum("ChocolatePi")
				.SetItemComponent<ITM_GenericZestyEatable>()
				.SetNameAndDescription("LtsOItems_ChocolatePi_Name", "LtsOItems_ChocolatePi_Desc")
				.BuildAndSetup<ITM_GenericZestyEatable>(out var genericZesty)
				.StoreAsNormal(Items.ZestyBar,  appearsInStore: true, weight: 125, acceptableFloors: ["F1", "F2", "F3", "END"]);

			genericZesty.affectorTime = 31.4f;
			genericZesty.staminaGain = 31.4f;
			genericZesty.speedMultiplier = 1.314f;

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("Pizza")
				.SetGeneratorCost(15)
				.SetShopPrice(175)
				.SetMeta(ItemFlags.None, ["food", ZESTYVARTAG])
				.SetEnum("Pizza")
				.SetItemComponent<ITM_GenericZestyEatable>()
				.SetNameAndDescription("LtsOItems_Pizza_Name", "LtsOItems_Pizza_Desc")
				.BuildAndSetup()
				.StoreAsNormal(Items.ZestyBar,  appearsInStore: true, weight: 115, acceptableFloors: ["F1", "F2", "F3", "END"]);

			var item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("pickles")
				.SetGeneratorCost(25)
				.SetShopPrice(575)
				.SetMeta(ItemFlags.MultipleUse | ItemFlags.Persists, ["food", ZESTYVARTAG])
				.SetEnum("JarOfPickles")
				.SetItemComponent<ITM_Reusable_GenericZestyEatable>()
				.SetNameAndDescription("LtsOItems_Pickles_Name", "LtsOItems_Pickles_Desc")
				.BuildAndSetup<ITM_Reusable_GenericZestyEatable>(out var genericReusableZesty)
				.StoreAsNormal(Items.ZestyBar, goToFieldTrips: true, appearsInStore: true, weight: 45, acceptableFloors: ["F1", "F2", "F3", "END"]);

			genericReusableZesty.maxStaminaLimit = 300f;
			genericReusableZesty.staminaMaxChanger = 50f;
			genericReusableZesty.affectorTime = 30f;

			genericReusableZesty.CreateNewReusableInstances(item, "LtsOItems_Pickles_Name", 3);

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
				.AutoGetSprites("HotCrossBun")
				.SetGeneratorCost(35)
				.SetShopPrice(600)
				.SetMeta(ItemFlags.Persists, ["food", ZESTYVARTAG, PIRATE_CANN_HATE])
				.SetEnum("HotCrossBun")
				.SetItemComponent<ITM_HotCrossBun>()
				.SetNameAndDescription("LtsOItems_HotCrossBun_Name", "LtsOItems_HotCrossBun_Desc")
				.BuildAndSetup<ITM_HotCrossBun>(out _)
				.StoreAsNormal(Items.ZestyBar,  appearsInStore: true, weight: 86, acceptableFloors: ["F1", "F2", "F3", "END"]);

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("orangeJuice")
			.SetGeneratorCost(30)
			.SetShopPrice(225)
			.SetMeta(ItemFlags.Persists, ["food", "drink", ZESTYVARTAG])
			.SetEnum("OrangeJuice")
			.SetItemComponent<ITM_GenericZestyEatable>()
			.SetNameAndDescription("LtsOItems_OrangeJuice_Name", "LtsOItems_OrangeJuice_Desc")
			.BuildAndSetup(out genericZesty)
			.StoreAsNormal(Items.ZestyBar,  appearsInStore: true, weight: 65, acceptableFloors: ["F1", "F2", "F3", "END"]);

			genericZesty.affectorTime = 15f;
			genericZesty.staminaRiseChanger = 2f;
			genericZesty.staminaDropChanger = 1.25f;

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("chocolateQuarter")
			.SetGeneratorCost(26)
			.SetShopPrice(350)
			.SetMeta(ItemFlags.None, ["food", ZESTYVARTAG, QUARTERVARTAG, "currency", PIRATE_CANN_HATE])
			.SetEnum("ChocolateQuarter")
			.SetItemComponent<ITM_ChocolateQuarter>()
			.SetNameAndDescription("LtsOItems_ChocolateQuarter_Name", "LtsOItems_ChocolateQuarter_Desc")
			.BuildAndSetup()
			.StoreAsNormal(Items.ZestyBar,  appearsInStore: true, weight: 75, acceptableFloors: ["F1", "F2", "F3", "END"]);

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("vanillaZestyBar")
			.SetGeneratorCost(29)
			.SetShopPrice(425)
			.SetMeta(ItemFlags.Persists, ["food", ZESTYVARTAG, PIRATE_CANN_HATE])
			.SetEnum("VanillaZestyBar")
			.SetItemComponent<ITM_VanillaZestyBar>()
			.SetNameAndDescription("LtsOItems_VanillaZestyBar_Name", "LtsOItems_VanillaZestyBar_Desc")
			.BuildAndSetup(out genericZesty)
			.StoreAsNormal(Items.ZestyBar,  appearsInStore: true, weight: 86, acceptableFloors: ["F2", "F3", "END"]);
			genericZesty.affectorTime = 10f;
			genericZesty.speedMultiplier = 1.1f;
			genericZesty.maxMultiplier = 3.5f;

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("iceZestyBar")
			.SetGeneratorCost(29)
			.SetShopPrice(500)
			.SetMeta(ItemFlags.Persists, ["food", ZESTYVARTAG, PIRATE_CANN_HATE])
			.SetEnum("IceZestyBar")
			.SetItemComponent<ITM_IceZestyBar>()
			.SetNameAndDescription("LtsOItems_IceZestyBar_Name", "LtsOItems_IceZestyBar_Desc")
			.BuildAndSetup(out genericZesty)
			.StoreAsNormal(Items.ZestyBar, goToFieldTrips: true, appearsInStore: true, weight: 35, acceptableFloors: ["F2", "F3", "END"]);
			genericZesty.maxMultiplier = 1.5f;

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("chocolateFlavouredZestyBar")
			.SetGeneratorCost(25)
			.SetShopPrice(600)
			.SetMeta(ItemFlags.Persists, ["food", ZESTYVARTAG, PIRATE_CANN_HATE])
			.SetEnum("ChocolateFlavouredZestyBar")
			.SetItemComponent<ITM_ChocolateFlavouredZestyBar>()
			.SetNameAndDescription("LtsOItems_ChocolateFlavouredZestyBar_Name", "LtsOItems_ChocolateFlavouredZestyBar_Desc")
			.BuildAndSetup(out genericZesty)
			.StoreAsNormal(Items.ZestyBar,  appearsInStore: true, weight: 75, acceptableFloors: ["F1", "F2", "F3", "END"]);

			genericZesty.staminaSet = 200;

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("dietFlavouredZestyBar")
			.SetGeneratorCost(18)
			.SetShopPrice(175)
			.SetMeta(ItemFlags.None, ["food", ZESTYVARTAG, PIRATE_CANN_HATE])
			.SetEnum("DietFlavouredZestyBar")
			.SetItemComponent<ITM_GenericZestyEatable>()
			.SetNameAndDescription("LtsOItems_DietFlavouredZestyBar_Name", "LtsOItems_DietFlavouredZestyBar_Desc")
			.BuildAndSetup(out genericZesty)
			.StoreAsNormal(Items.ZestyBar,  appearsInStore: true, weight: 175, acceptableFloors: ["F1", "F2", "F3", "END"]);

			genericZesty.staminaGain = 50;

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("Chalk") // expects Chalk_smallIcon.png and Chalk_smallIcon.png
			.SetGeneratorCost(25)
			.SetShopPrice(165)
			.SetMeta(ItemFlags.Persists, ["food", ZESTYVARTAG, CHALKERASERVARTAG])
			.SetEnum("Chalk")
			.SetItemComponent<ITM_Chalk>()
			.SetNameAndDescription("LtsOItems_Chalk_Name", "LtsOItems_Chalk_Desc")
			.BuildAndSetup(out genericZesty)
			.StoreAsNormal(Items.ZestyBar,  appearsInStore: true, weight: 155, acceptableFloors: ["F1", "F2", "F3", "END"]);

			genericZesty.maxMultiplier = 0.5f;

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("PanicKernels")
			.SetGeneratorCost(35)
			.SetShopPrice(950)
			.SetMeta(ItemFlags.Persists, ["food", PIRATE_CANN_HATE, ZESTYVARTAG])
			.SetEnum("PanicKernels")
			.SetItemComponent<ITM_PanicKernels>()
			.SetNameAndDescription("LtsOItems_PanicKernels_Name", "LtsOItems_PanicKernels_Desc")
			.BuildAndSetup(out genericZesty)
			.StoreAsNormal(Items.ZestyBar,  appearsInStore: true, weight: 55, acceptableFloors: ["F2", "F3", "END"]);

			genericZesty.staminaGain = 50f;

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("ThornGlognut")
			.SetGeneratorCost(45)
			.SetShopPrice(750)
			.SetMeta(ItemFlags.Persists, ["food", ZESTYVARTAG, PIRATE_CANN_HATE, CRIMINALPACK_CONTRABAND])
			.SetEnum("ThornGlognut")
			.SetItemComponent<ITM_ThornGlognut>()
			.SetNameAndDescription("LtsOItems_ThornGlognut_Name", "LtsOItems_ThornGlognut_Desc")
			.BuildAndSetup(out genericZesty)
			.StoreAsNormal(Items.ZestyBar,  appearsInStore: true, weight: 85, acceptableFloors: ["F2", "F3", "END"]);

			genericZesty.affectorTime = 0f;
			genericZesty.staminaGain = 300f;


			// ---------------- TELEPORTERS ---------------

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("CalibratedTeleporter")
			.SetGeneratorCost(30)
			.SetShopPrice(750)
			.SetMeta(ItemFlags.None, [DANGERTELEPORTERVARTAG, CRIMINALPACK_CONTRABAND])
			.SetEnum("CalibratedTeleporter")
			.SetItemComponent<ITM_CalibratedTeleporter>()
			.SetNameAndDescription("LtsOItems_CalibratedTp_Name", "LtsOItems_CalibratedTp_Desc")
			.BuildAndSetup()
			.StoreAsNormal(Items.Teleporter, goToFieldTrips: true, appearsInStore: true, weight: 76, acceptableFloors: ["F2", "F3", "END"]);

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("SaferTeleporter")
			.SetGeneratorCost(37)
			.SetShopPrice(850)
			.SetMeta(ItemFlags.Persists, [DANGERTELEPORTERVARTAG, CRIMINALPACK_CONTRABAND])
			.SetEnum("SaferTeleporter")
			.SetItemComponent<ITM_SaferTeleporter>()
			.SetNameAndDescription("LtsOItems_SaferTeleporter_Name", "LtsOItems_SaferTeleporter_Desc")
			.BuildAndSetup<ITM_GenericTeleporter>(out var genericTp)
			.StoreAsNormal(Items.Teleporter, goToFieldTrips: true, appearsInStore: true, weight: 35, acceptableFloors: ["F2", "F3", "END"]);

			genericTp.minTeleports = 1;
			genericTp.maxTeleports = 1;
			genericTp.baseTime = 0.035f;

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("ControlledTeleporter")
			.SetGeneratorCost(40)
			.SetShopPrice(1000)
			.SetMeta(ItemFlags.Persists, [DANGERTELEPORTERVARTAG, CRIMINALPACK_CONTRABAND])
			.SetEnum("ControlledTeleporter")
			.SetItemComponent<ITM_ControlledTeleporter>()
			.SetNameAndDescription("LtsOItems_ControlledTeleporter_Name", "LtsOItems_ControlledTeleporter_Desc")
			.BuildAndSetup(out genericTp)
			.StoreAsNormal(Items.Teleporter, goToFieldTrips: true, appearsInStore: true, weight: 23, acceptableFloors: ["F2", "F3", "END"]);

			genericTp.minTeleports = 1;
			genericTp.maxTeleports = 1;
			genericTp.baseTime = 0.035f;

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("BananaTeleporter")
			.SetGeneratorCost(40)
			.SetShopPrice(1300)
			.SetMeta(ItemFlags.Persists, [DANGERTELEPORTERVARTAG, CRIMINALPACK_CONTRABAND])
			.SetEnum("BananaTeleporter")
			.SetItemComponent<ITM_BananaTeleporter>()
			.SetNameAndDescription("LtsOItems_BananaTeleporter_Name", "LtsOItems_BananaTeleporter_Desc")
			.BuildAndSetup(out genericTp)
			.StoreAsNormal(Items.Teleporter, goToFieldTrips: true, appearsInStore: true, weight: 25, acceptableFloors: ["F2", "F3", "END"]);

			genericTp.minTeleports = 7;
			genericTp.maxTeleports = 12;
			genericTp.baseTime = 0.08f;
			genericTp.increaseFactor = 1.26f;

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("DetentionTeleporter") // expects DetentionTeleporter_smallIcon.png and DetentionTeleporter_largeIcon.png
			.SetGeneratorCost(40)
			.SetShopPrice(1100)
			.SetMeta(ItemFlags.Persists, [DANGERTELEPORTERVARTAG, CRIMINALPACK_CONTRABAND])
			.SetEnum("DetentionTeleporter")
			.SetItemComponent<ITM_DetentionTeleporter>()
			.SetNameAndDescription("LtsOItems_DetentionTeleporter_Name", "LtsOItems_DetentionTeleporter_Desc")
			.BuildAndSetup(out genericTp)
			.StoreAsNormal(Items.Teleporter, appearsInStore: true, weight: 100, acceptableFloors: ["F2", "F3", "END"]);

			genericTp.minTeleports = 1;
			genericTp.maxTeleports = 1;
			genericTp.baseTime = 0.035f;


			// ---------- WHISTLES VARIANTS ----------
			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("PrincipalOEleporter")
			.SetGeneratorCost(16)
			.SetShopPrice(350)
			.SetMeta(ItemFlags.Persists, [PRINCIPALWHISTLEVARTAG, CRIMINALPACK_CONTRABAND])
			.SetEnum("PrincipalOEleporter")
			.SetItemComponent<ITM_PrincipalOEleporter>()
			.SetNameAndDescription("LtsOItems_PrincipalOEleporter_Name", "LtsOItems_PrincipalOEleporter_Desc")
			.BuildAndSetup()
			.StoreAsNormal(Items.PrincipalWhistle, appearsInStore: true, weight: 96, acceptableFloors: ["F2", "F3", "END"]);


			// ---------- Nanapeel VARIANTS ---------

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("TeleporterOWall")
			.SetGeneratorCost(27)
			.SetShopPrice(650)
			.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, [NANAPEELVARTAG, DANGERTELEPORTERVARTAG])
			.SetEnum("TeleporterOWall")
			.SetItemComponent<ITM_TeleporterOWall>(Items.NanaPeel) // Makes a new exact copy of ITM_NanaPeel
			.SetNameAndDescription("LtsOItems_TeleporterOWall_Name", "LtsOItems_TeleporterOWall_Desc")
			.BuildAndSetup()
			.StoreAsNormal(Items.NanaPeel, appearsInStore: true, weight: 35, acceptableFloors: ["F2", "F3", "END"]);

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("LandMine")
			.SetGeneratorCost(25)
			.SetShopPrice(650)
			.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, [NANAPEELVARTAG, CRIMINALPACK_CONTRABAND, PIRATE_CANN_HATE])
			.SetEnum("LandMine")
			.SetItemComponent<ITM_LandMine>(Items.NanaPeel)
			.SetNameAndDescription("LtsOItems_LandMine_Name", "LtsOItems_LandMine_Desc")
			.BuildAndSetup()
			.StoreAsNormal(Items.NanaPeel, goToFieldTrips: true, appearsInStore: true, weight: 95, acceptableFloors: ["F2", "F3", "END"]);



			// ------------ QUARTERS -------------

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("quarterOnAString")
			.SetGeneratorCost(35)
			.SetShopPrice(675)
			.SetMeta(ItemFlags.MultipleUse | ItemFlags.Persists, [QUARTERVARTAG, CRIMINALPACK_CONTRABAND])
			.SetEnum("QuarterOnAString")
			.SetItemComponent<ITM_QuarterOnAString>()
			.SetNameAndDescription("LtsOItems_QuarterOnAString_Name", "LtsOItems_QuarterOnAString_Desc")
			.BuildAndSetup<ITM_QuarterOnAString>(out var quarterOnAString)
			.StoreAsNormal(Items.Quarter, goToFieldTrips: true, appearsInStore: true, weight: 55, acceptableFloors: ["F2", "F3", "END"]);

			quarterOnAString.CreateNewReusableInstances(item, "LtsOItems_QuarterOnAString_Name", 2);

			// ---------- NAME TAGS -----------

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("EnergyNametag")
			.SetGeneratorCost(26)
			.SetShopPrice(600)
			.SetMeta(ItemFlags.Persists, [NAMETAGVARTAG])
			.SetEnum("EnergyNametag")
			.SetItemComponent<ITM_EnergyNametag>(Items.Nametag)
			.SetNameAndDescription("LtsOItems_EnergyNametag_Name", "LtsOItems_EnergyNametag_Desc")
			.BuildAndSetup()
			.StoreAsNormal(Items.Nametag, goToFieldTrips: true, appearsInStore: true, weight: 75, acceptableFloors: ["F1", "F2", "F3"]);

			// --------- Chalk Erasers ---------
			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("FogMachine") // expects FogMachine_smallIcon.png and FogMachine_largeIcon.png
			.SetGeneratorCost(34)
			.SetShopPrice(750)
			.SetMeta(ItemFlags.Persists, [CHALKERASERVARTAG, CRIMINALPACK_CONTRABAND])
			.SetEnum("FogMachine")
			.SetItemComponent<ITM_FogMachine>()
			.SetNameAndDescription("LtsOItems_FogMachine_Name", "LtsOItems_FogMachine_Desc")
			.BuildAndSetup()
			.StoreAsNormal(Items.ChalkEraser, appearsInStore: true, weight: 45, acceptableFloors: ["F2", "F3", "END"]);

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("ChalkBomb")
			.SetGeneratorCost(37)
			.SetShopPrice(850)
			.SetMeta(ItemFlags.CreatesEntity | ItemFlags.Persists, [CHALKERASERVARTAG, CRIMINALPACK_CONTRABAND])
			.SetEnum("ChalkBomb")
			.SetItemComponent<ITM_ChalkBomb>(Items.NanaPeel)
			.SetNameAndDescription("LtsOItems_ChalkBomb_Name", "LtsOItems_ChalkBomb_Desc")
			.BuildAndSetup()
			.StoreAsNormal(Items.ChalkEraser, appearsInStore: true, weight: 95, acceptableFloors: ["F1", "F2", "F3", "END"]);

			// ------ Key ------
			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("KillerKey")
			.SetGeneratorCost(35)
			.SetShopPrice(650)
			.SetMeta(ItemFlags.None, [KEYVARTAG, CRIMINALPACK_CONTRABAND])
			.SetEnum("KillerKey")
			.SetItemComponent<ITM_KillerKey>()
			.SetNameAndDescription("LtsOItems_KillerKey_Name", "LtsOItems_KillerKey_Desc")
			.BuildAndSetup()
			.StoreAsNormal(Items.DetentionKey, appearsInStore: true, weight: 85, acceptableFloors: ["F1", "F2", "F3", "END"]);
			item.AddKeyTypeItem();

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("SpecialKey")
			.SetGeneratorCost(30)
			.SetShopPrice(400)
			.SetMeta(ItemFlags.Persists, [KEYVARTAG, CRIMINALPACK_CONTRABAND])
			.SetEnum("SpecialKey")
			.SetItemComponent<ITM_SpecialKey>()
			.SetNameAndDescription("LtsOItems_SpecialKey_Name", "LtsOItems_SpecialKey_Desc")
			.BuildAndSetup()
			.StoreAsNormal(Items.DetentionKey, goToFieldTrips: true, appearsInStore: true, weight: 75, acceptableFloors: ["F1", "F2", "F3", "END"]);
			item.AddKeyTypeItem();


			// ------ Scissors ------
			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("DangerousScissors")
			.SetGeneratorCost(25)
			.SetShopPrice(450)
			.SetMeta(ItemFlags.Persists, [SCISSORSVARTAG, CRIMINALPACK_CONTRABAND])
			.SetEnum("DangerousScissors")
			.SetItemComponent<ITM_DangerousScissors>(Items.Scissors)
			.SetNameAndDescription("LtsOItems_DangerousScissors_Name", "LtsOItems_DangerousScissors_Desc")
			.BuildAndSetup()
			.StoreAsNormal(Items.Scissors, appearsInStore: true, weight: 95, acceptableFloors: ["F1", "F2", "F3", "END"]);

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("MetalScissors")
			.SetGeneratorCost(22)
			.SetShopPrice(400)
			.SetMeta(ItemFlags.Persists, [SCISSORSVARTAG])
			.SetEnum("MetalScissors")
			.SetItemComponent<ITM_MetalScissors>(Items.Scissors)
			.SetNameAndDescription("LtsOItems_MetalScissors_Name", "LtsOItems_MetalScissors_Desc")
			.BuildAndSetup()
			.StoreAsNormal(Items.Scissors, appearsInStore: true, weight: 95, acceptableFloors: ["F1", "F2", "F3", "END"]);

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("CardboardScissors")
			.SetGeneratorCost(16)
			.SetShopPrice(125)
			.SetMeta(ItemFlags.Persists | ItemFlags.MultipleUse, [SCISSORSVARTAG])
			.SetEnum("CardboardScissors")
			.SetItemComponent<ITM_CardboardScissors>(Items.Scissors)
			.SetNameAndDescription("LtsOItems_CardboardScissors_Name", "LtsOItems_CardboardScissors_Desc")
			.BuildAndSetup<ITM_CardboardScissors>(out var cardboardScissors)
			.StoreAsNormal(Items.Scissors, appearsInStore: true, weight: 135, acceptableFloors: ["F1", "F2", "F3", "END"]);

			var newInstances = cardboardScissors.CreateNewReusableInstances(item, "LtsOItems_CardboardScissors_Name", 2);

			newInstances[1].itemSpriteLarge = AssetLoader.SpriteFromFile(GetItemIcon("CardboardScissors_2", true), Vector2.one * 0.5f, 50f);
			newInstances[1].itemSpriteSmall = AssetLoader.SpriteFromFile(GetItemIcon("CardboardScissors_2", false), Vector2.one * 0.5f, 50f);

			newInstances[2].itemSpriteLarge = AssetLoader.SpriteFromFile(GetItemIcon("CardboardScissors_3", true), Vector2.one * 0.5f, 50f);
			newInstances[2].itemSpriteSmall = AssetLoader.SpriteFromFile(GetItemIcon("CardboardScissors_3", false), Vector2.one * 0.5f, 50f);


			// ----- Swinging door locks -----
			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("UniversalLock")
			.SetGeneratorCost(35)
			.SetShopPrice(400)
			.SetMeta(ItemFlags.None, [DOORLOCKVARTAG])
			.SetEnum("UniversalLock")
			.SetItemComponent<ITM_UniversalLock>()
			.SetNameAndDescription("LtsOItems_UniversalLock_Name", "LtsOItems_UniversalLock_Desc")
			.BuildAndSetup()
			.StoreAsNormal(Items.DoorLock, appearsInStore: true, weight: 100, acceptableFloors: ["F1", "F2", "F3", "END"]);

			// ---- boots ----
			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("ShinyCleanGloves")
			.SetGeneratorCost(30)
			.SetShopPrice(450)
			.SetMeta(ItemFlags.Persists, [BOOTSVARTAG])
			.SetEnum("ShinyCleanGloves")
			.SetItemComponent<ITM_ShinyCleanGloves>(Items.Boots)
			.SetNameAndDescription("LtsOItems_ShinyCleanGloves_Name", "LtsOItems_ShinyCleanGloves_Desc")
			.BuildAndSetup<ITM_Boots>(out var boots)
			.StoreAsNormal(Items.Boots, appearsInStore: true, weight: 85, acceptableFloors: ["F1", "F2", "F3", "END"]);

			boots.setTime = 45f;

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("ReallyTallBoots")
			.SetGeneratorCost(28)
			.SetShopPrice(650)
			.SetMeta(ItemFlags.Persists, [BOOTSVARTAG])
			.SetEnum("ReallyTallBoots")
			.SetItemComponent<ITM_ReallyTallBoots>(Items.Boots)
			.SetNameAndDescription("LtsOItems_ReallyTallBoots_Name", "LtsOItems_ReallyTallBoots_Desc")
			.BuildAndSetup(out boots)
			.StoreAsNormal(Items.Boots, appearsInStore: true, weight: 75, acceptableFloors: ["F2", "F3", "END"]);

			boots.setTime = 30f;

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("RustyOldShoes")
			.SetGeneratorCost(13)
			.SetShopPrice(100)
			.SetMeta(ItemFlags.Persists, [BOOTSVARTAG])
			.SetEnum("RustyOldShoes")
			.SetItemComponent<ITM_Boots>(Items.Boots)
			.SetNameAndDescription("LtsOItems_RustyOldShoes_Name", "LtsOItems_RustyOldShoes_Desc")
			.BuildAndSetup(out boots)
			.StoreAsNormal(Items.Boots, appearsInStore: true, weight: 175, acceptableFloors: ["F1", "F2", "F3", "END"]);

			boots.setTime = 15f;
			boots.GetComponentInChildren<Image>().sprite = AssetLoader.SpriteFromFile(Path.Combine(LotOfItemsPlugin.ModPath, "RustyOldShoes_canvas.png"), Vector2.one * 0.5f);

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("BalletShoes")
			.SetGeneratorCost(22)
			.SetShopPrice(350)
			.SetMeta(ItemFlags.Persists, [BOOTSVARTAG])
			.SetEnum("BalletShoes")
			.SetItemComponent<ITM_BalletShoes>(Items.Boots)
			.SetNameAndDescription("LtsOItems_BalletShoes_Name", "LtsOItems_BalletShoes_Desc")
			.BuildAndSetup()
			.StoreAsNormal(Items.Boots, appearsInStore: true, weight: 90, acceptableFloors: ["F1", "F2", "F3", "END"]);

			// ---- Alarm Clock variant ----

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("TestClock")
			.SetGeneratorCost(25)
			.SetShopPrice(650)
			.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, [ALARMCLOCKVARTAG, CRIMINALPACK_CONTRABAND])
			.SetEnum("TestClock")
			.SetItemComponent<ITM_TestClock>(Items.AlarmClock)
			.SetNameAndDescription("LtsOItems_TestClock_Name", "LtsOItems_TestClock_Desc")
			.BuildAndSetup<ITM_GenericAlarmClock>(out var genericClock)
			.StoreAsNormal(Items.AlarmClock, goToFieldTrips: true, appearsInStore: true, weight: 65, acceptableFloors: ["F2", "F3", "END"]);

			genericClock.setTime[0] = 15f;
			genericClock.initSetTime = 0;

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("WheelClock")
			.SetGeneratorCost(20)
			.SetShopPrice(500)
			.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, [ALARMCLOCKVARTAG])
			.SetEnum("WheelClock")
			.SetItemComponent<ITM_WheelClock>(Items.AlarmClock)
			.SetNameAndDescription("LtsOItems_WheelClock_Name", "LtsOItems_WheelClock_Desc")
			.BuildAndSetup(out genericClock)
			.StoreAsNormal(Items.AlarmClock, goToFieldTrips: true, appearsInStore: true, weight: 85, acceptableFloors: ["F1", "F2", "F3", "END"]);

			genericClock.setTime[0] = 15f;
			genericClock.initSetTime = 0;

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("DynamiteClock")
			.SetGeneratorCost(32)
			.SetShopPrice(750)
			.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, [ALARMCLOCKVARTAG, CRIMINALPACK_CONTRABAND, PIRATE_CANN_HATE])
			.SetEnum("DynamiteClock")
			.SetItemComponent<ITM_DynamiteClock>(Items.AlarmClock)
			.SetNameAndDescription("LtsOItems_DynamiteClock_Name", "LtsOItems_DynamiteClock_Desc")
			.BuildAndSetup(out genericClock)
			.StoreAsNormal(Items.AlarmClock, goToFieldTrips: true, appearsInStore: true, weight: 65, acceptableFloors: ["F2", "F3", "END"]);

			genericClock.setTime[0] = 15f;
			genericClock.initSetTime = 0;
			genericClock.noiseVal = 120;

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("AncientClock")
			.SetGeneratorCost(37)
			.SetShopPrice(800)
			.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, [ALARMCLOCKVARTAG])
			.SetEnum("AncientClock")
			.SetItemComponent<ITM_AncientClock>(Items.AlarmClock)
			.SetNameAndDescription("LtsOItems_AncientClock_Name", "LtsOItems_AncientClock_Desc")
			.BuildAndSetup(out genericClock)
			.StoreAsNormal(Items.AlarmClock, goToFieldTrips: true, appearsInStore: true, weight: 50, acceptableFloors: ["F2", "F3", "END"]);

			genericClock.setTime[0] = 30f;
			genericClock.initSetTime = 0;

			// ---- tape -----
			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("BaldisMostFavoriteTape")
			.SetGeneratorCost(36)
			.SetShopPrice(500)
			.SetMeta(ItemFlags.None, [TAPEVARTAG])
			.SetEnum("BaldisMostFavoriteTape")
			.SetItemComponent<ITM_BaldisMostFavoriteTape>()
			.SetNameAndDescription("LtsOItems_BaldisMostFavoriteTape_Name", "LtsOItems_BaldisMostFavoriteTape_Desc")
			.BuildAndSetup()
			.StoreAsNormal(Items.Tape, goToFieldTrips: true, appearsInStore: true, weight: 75, acceptableFloors: ["F1", "F2", "F3", "END"]);

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("PartyTape")
			.SetGeneratorCost(31)
			.SetShopPrice(450)
			.SetMeta(ItemFlags.None, [TAPEVARTAG])
			.SetEnum("PartyTape")
			.SetItemComponent<ITM_PartyTape>()
			.SetNameAndDescription("LtsOItems_PartyTape_Name", "LtsOItems_PartyTape_Desc")
			.BuildAndSetup()
			.StoreAsNormal(Items.Tape, appearsInStore: true, weight: 60, acceptableFloors: ["F2", "F3", "END"]);

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("LeastFavoriteTape")
			.SetGeneratorCost(35)
			.SetShopPrice(750)
			.SetMeta(ItemFlags.Persists, [TAPEVARTAG, CRIMINALPACK_CONTRABAND])
			.SetEnum("LeastFavoriteTape")
			.SetItemComponent<ITM_LeastFavoriteTape>()
			.SetNameAndDescription("LtsOItems_LeastFavoriteTape_Name", "LtsOItems_LeastFavoriteTape_Desc")
			.BuildAndSetup()
			.StoreAsNormal(Items.Tape, goToFieldTrips: true, appearsInStore: true, weight: 35, acceptableFloors: ["F1", "F2", "F3", "END"]);

			// --- Grapples ---

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("PushingHook")
			.SetGeneratorCost(35)
			.SetShopPrice(850)
			.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, [GRAPPLEVARTAG])
			.SetEnum("PushingHook")
			.SetItemComponent<ITM_PushingHook>(Items.GrapplingHook)
			.SetNameAndDescription("LtsOItems_PushingHook_Name", "LtsOItems_PushingHook_Desc")
			.BuildAndSetup()
			.StoreAsNormal(Items.GrapplingHook, goToFieldTrips: true, appearsInStore: true, weight: 65, acceptableFloors: ["F2", "F3", "END"]);

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("Harpoon")
			.SetGeneratorCost(32)
			.SetShopPrice(800)
			.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, [GRAPPLEVARTAG, CRIMINALPACK_CONTRABAND])
			.SetEnum("Harpoon")
			.SetItemComponent<ITM_Harpoon>(Items.GrapplingHook)
			.SetNameAndDescription("LtsOItems_Harpoon_Name", "LtsOItems_Harpoon_Desc")
			.BuildAndSetup()
			.StoreAsNormal(Items.GrapplingHook, goToFieldTrips: true, appearsInStore: true, weight: 75, acceptableFloors: ["F2", "F3", "END"]);

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("BouncyGrapplingHook")
			.SetGeneratorCost(30)
			.SetShopPrice(800)
			.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, [GRAPPLEVARTAG, CRIMINALPACK_CONTRABAND])
			.SetEnum("BouncyGrapplingHook")
			.SetItemComponent<ITM_BouncyGrapplingHook>(Items.GrapplingHook)
			.SetNameAndDescription("LtsOItems_BouncyGrapplingHook_Name", "LtsOItems_BouncyGrapplingHook_Desc")
			.BuildAndSetup()
			.StoreAsNormal(Items.GrapplingHook, appearsInStore: true, weight: 80, acceptableFloors: ["F2", "F3", "END"]);

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("CheapGrapplingHook")
			.SetGeneratorCost(25)
			.SetShopPrice(350)
			.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, [GRAPPLEVARTAG, CRIMINALPACK_CONTRABAND])
			.SetEnum("CheapGrapplingHook")
			.SetItemComponent<ITM_GrapplingHook>(Items.GrapplingHook)
			.SetNameAndDescription("LtsOItems_CheapGrapplingHook_Name", "LtsOItems_CheapGrapplingHook_Desc")
			.BuildAndSetup(out ITM_GrapplingHook hook)
			.StoreAsNormal(Items.GrapplingHook, appearsInStore: true, weight: 145, acceptableFloors: ["F1", "F2", "F3", "END"]);

			hook.time = 59.6f;
			hook.forceIncrease *= 0.45f;
			hook.initialForce *= 0.85f;
			hook.maxPressure *= 0.95f;

			var hookRenderer = hook.GetComponentInChildren<SpriteRenderer>();
			hookRenderer.sprite = AssetLoader.SpriteFromFile(Path.Combine(LotOfItemsPlugin.ModPath, "CheapGrapplingHook_world.png"), Vector2.one * 0.5f, hookRenderer.sprite.pixelsPerUnit);
			hook.uses = 0;

			// ----- portal poster variant -----
			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("PortalDoor")
			.SetGeneratorCost(45)
			.SetShopPrice(650)
			.SetMeta(ItemFlags.Persists, [PORTALVARTAG])
			.SetEnum("PortalDoor")
			.SetItemComponent<ITM_PortalDoor>()
			.SetNameAndDescription("LtsOItems_PortalDoor_Name", "LtsOItems_PortalDoor_Desc")
			.BuildAndSetup()
			.StoreAsNormal(Items.PortalPoster, appearsInStore: true, weight: 35, acceptableFloors: ["F2", "F3", "END"]);

			// ----- Soda -----
			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("SadSoda")
			.SetGeneratorCost(25)
			.SetShopPrice(500)
			.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, ["drink", SODAVARTAG])
			.SetEnum("SadSoda")
			.SetItemComponent<ITM_SadSoda>(Items.Bsoda)
			.SetNameAndDescription("LtsOItems_SadSoda_Name", "LtsOItems_SadSoda_Desc")
			.BuildAndSetup()
			.StoreAsNormal(Items.Bsoda, appearsInStore: true, weight: 75, acceptableFloors: ["F1", "F2", "F3", "END"]);

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("RootBeer")
			.SetGeneratorCost(30)
			.SetShopPrice(500)
			.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, ["drink", SODAVARTAG])
			.SetEnum("RootBeer")
			.SetItemComponent<ITM_RootBeer>(Items.Bsoda)
			.SetNameAndDescription("LtsOItems_RootBeer_Name", "LtsOItems_RootBeer_Desc")
			.BuildAndSetup<ITM_RootBeer>(out var rootBeer)
			.StoreAsNormal(Items.Bsoda, appearsInStore: true, weight: 85, acceptableFloors: ["F2", "F3", "END"]);

			rootBeer.bsodaPrefab = GetVariantInstance<TemporaryBsoda>(Items.Bsoda);
			rootBeer.bsodaPrefab.time = 7.5f;
			rootBeer.bsodaPrefab.speed *= 1.2f;
			rootBeer.bsodaPrefab.spriteRenderer.sprite = AssetLoader.SpriteFromTexture2D(GenericExtensions.FindResourceObjectByName<Texture2D>("DustCloud"), rootBeer.bsodaPrefab.spriteRenderer.sprite.pixelsPerUnit);
			rootBeer.bsodaPrefab.DestroyParticleIfItHasOne();

			rootBeer.speed = rootBeer.bsodaPrefab.speed;

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("ShakenSoda")
			.SetGeneratorCost(28)
			.SetShopPrice(650)
			.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, [SODAVARTAG, CRIMINALPACK_CONTRABAND, PIRATE_CANN_HATE])
			.SetEnum("ShakenSoda")
			.SetItemComponent<ITM_ShakenSoda>(Items.NanaPeel)
			.SetNameAndDescription("LtsOItems_ShakenSoda_Name", "LtsOItems_ShakenSoda_Desc")
			.BuildAndSetup()
			.StoreAsNormal(Items.NanaPeel, appearsInStore: true, weight: 85, acceptableFloors: ["F2", "F3", "END"]);

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("ShrinkRay")
			.SetGeneratorCost(25)
			.SetShopPrice(450)
			.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, ["drink", SODAVARTAG])
			.SetEnum("ShrinkRay")
			.SetItemComponent<ITM_ShrinkRay>(Items.DietBsoda)
			.SetNameAndDescription("LtsOItems_ShrinkRay_Name", "LtsOItems_ShrinkRay_Desc")
			.BuildAndSetup()
			.StoreAsNormal(Items.DietBsoda, goToFieldTrips: true, appearsInStore: true, weight: 65, acceptableFloors: ["F1", "F2", "F3", "END"]);

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("EnergyFlavoredZestySoda")
			.SetGeneratorCost(25)
			.SetShopPrice(450)
			.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, ["drink", "food", SODAVARTAG, PIRATE_CANN_HATE])
			.SetEnum("EnergyFlavoredZestySoda")
			.SetItemComponent<ITM_EnergyFlavoredZestySoda>(Items.DietBsoda)
			.SetNameAndDescription("LtsOItems_EnergyFlavoredZestySoda_Name", "LtsOItems_EnergyFlavoredZestySoda_Desc")
			.BuildAndSetup()
			.StoreAsNormal(Items.DietBsoda, goToFieldTrips: true, appearsInStore: true, weight: 45, acceptableFloors: ["F1", "F2", "F3", "END"]);

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("OSODA")
			.SetGeneratorCost(30)
			.SetShopPrice(650)
			.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, ["drink", SODAVARTAG])
			.SetEnum("OSODA")
			.SetItemComponent<ITM_OSODA>(Items.Bsoda)
			.SetNameAndDescription("LtsOItems_OSODA_Name", "LtsOItems_OSODA_Desc")
			.BuildAndSetup()
			.StoreAsNormal(Items.Bsoda, goToFieldTrips: true, appearsInStore: true, weight: 75, acceptableFloors: ["F1", "F2", "F3", "END"]);

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("LemonSoda")
			.SetGeneratorCost(25)
			.SetShopPrice(800)
			.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, ["drink", SODAVARTAG])
			.SetEnum("LemonSoda")
			.SetItemComponent<ITM_LemonSoda>(Items.Bsoda)
			.SetNameAndDescription("LtsOItems_LemonSoda_Name", "LtsOItems_LemonSoda_Desc")
			.BuildAndSetup()
			.StoreAsNormal(Items.Bsoda, appearsInStore: true, weight: 65, acceptableFloors: ["F1", "F2", "F3", "END"]);

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("BSODAGun")
			.SetGeneratorCost(23)
			.SetShopPrice(350)
			.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, ["drink", SODAVARTAG])
			.SetEnum("BSODAGun")
			.SetItemComponent<ITM_BSODA>(Items.Bsoda)
			.SetNameAndDescription("LtsOItems_BSODAGun_Name", "LtsOItems_BSODAGun_Desc")
			.BuildAndSetup<ITM_BSODA>(out var normalBsoda)
			.StoreAsNormal(Items.Bsoda, appearsInStore: true, weight: 75, acceptableFloors: ["F1", "F2", "F3", "END"]);

			normalBsoda.speed *= 10f;
			normalBsoda.time = 5f;
			normalBsoda.sound = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(LotOfItemsPlugin.ModPath, "BsodaGun_Shoot.wav")), string.Empty, SoundType.Effect, Color.white);
			normalBsoda.sound.subtitle = false;

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("DietDietSoda")
			.SetGeneratorCost(21)
			.SetShopPrice(450)
			.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, ["drink", SODAVARTAG])
			.SetEnum("DietDietBSODA")
			.SetItemComponent<ITM_BSODA>(Items.DietBsoda)
			.SetNameAndDescription("LtsOItems_DietDietSoda_Name", "LtsOItems_DietDietSoda_Desc")
			.BuildAndSetup(out normalBsoda)
			.StoreAsNormal(Items.DietBsoda, appearsInStore: true, weight: 125, acceptableFloors: ["F1", "F2", "F3", "END"]);

			normalBsoda.speed = -normalBsoda.speed;
			normalBsoda.time = 5f;

			normalBsoda.spriteRenderer.sprite = AssetLoader.SpriteFromFile(Path.Combine(LotOfItemsPlugin.ModPath, "DietDiet_Soda.png"), Vector2.one * 0.5f, normalBsoda.spriteRenderer.sprite.pixelsPerUnit);
			//Object.Destroy(normalBsoda.GetComponentInChildren<ParticleSystem>().gameObject);

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("DietDietDietSoda")
			.SetGeneratorCost(21)
			.SetShopPrice(450)
			.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, ["drink", SODAVARTAG])
			.SetEnum("DietDietDietBSODA")
			.SetItemComponent<ITM_BSODA>(Items.DietBsoda)
			.SetNameAndDescription("LtsOItems_DietDietDietSoda_Name", "LtsOItems_DietDietDietSoda_Desc")
			.BuildAndSetup(out normalBsoda)
			.StoreAsNormal(Items.DietBsoda, appearsInStore: true, weight: 100, acceptableFloors: ["F1", "F2", "F3", "END"]);

			normalBsoda.speed = 0f;
			normalBsoda.time = 15f;

			normalBsoda.spriteRenderer.sprite = AssetLoader.SpriteFromFile(Path.Combine(LotOfItemsPlugin.ModPath, "DietDietDiet_Soda.png"), Vector2.one * 0.5f, normalBsoda.spriteRenderer.sprite.pixelsPerUnit);
			normalBsoda.DestroyParticleIfItHasOne();

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("FroginaCan")
			.SetGeneratorCost(28)
			.SetShopPrice(900)
			.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, ["drink", SODAVARTAG, PIRATE_CANN_HATE])
			.SetEnum("FroginaCan")
			.SetItemComponent<ITM_FrogInACan>(Items.Bsoda)
			.SetNameAndDescription("LtsOItems_FrogInACan_Name", "LtsOItems_FrogInACan_Desc")
			.BuildAndSetup()
			.StoreAsNormal(Items.Bsoda, goToFieldTrips: true, appearsInStore: true, weight: 45, acceptableFloors: ["F1", "F2", "F3", "END"]);

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("Plunger")
			.SetGeneratorCost(25)
			.SetShopPrice(500)
			.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, ["drink", SODAVARTAG, PIRATE_CANN_HATE])
			.SetEnum("Plunger")
			.SetItemComponent<ITM_Plunger>(Items.Bsoda)
			.SetNameAndDescription("LtsOItems_Plunger_Name", "LtsOItems_Plunger_Desc")
			.BuildAndSetup()
			.StoreAsNormal(Items.Bsoda, appearsInStore: true, weight: 75, acceptableFloors: ["F1", "F2", "F3", "END"]);

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("BloxyCola")
			.SetGeneratorCost(32)
			.SetShopPrice(725)
			.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, ["drink", SODAVARTAG])
			.SetEnum("BloxyCola")
			.SetItemComponent<ITM_BloxyCola>(Items.Bsoda)
			.SetNameAndDescription("LtsOItems_BloxyCola_Name", "LtsOItems_BloxyCola_Desc")
			.BuildAndSetup()
			.StoreAsNormal(Items.Bsoda, goToFieldTrips: true, appearsInStore: true, weight: 90, acceptableFloors: ["F1", "F2", "F3", "END"]);

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("Rotatoda")
			.SetGeneratorCost(26)
			.SetShopPrice(650)
			.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, ["drink", SODAVARTAG])
			.SetEnum("Rotatoda")
			.SetItemComponent<ITM_Rotatoda>(Items.Bsoda)
			.SetNameAndDescription("LtsOItems_Rotatoda_Name", "LtsOItems_Rotatoda_Desc")
			.BuildAndSetup()
			.StoreAsNormal(Items.Bsoda, appearsInStore: true, weight: 75, acceptableFloors: ["F1", "F2", "F3", "END"]);

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("SpillingBSODA")
			.SetGeneratorCost(22)
			.SetShopPrice(400)
			.SetMeta(ItemFlags.Persists, ["drink", SODAVARTAG])
			.SetEnum("SpillingBSODA")
			.SetItemComponent<ITM_SpillingBSODA>()
			.SetNameAndDescription("LtsOItems_SpillingBSODA_Name", "LtsOItems_SpillingBSODA_Desc")
			.BuildAndSetup()
			.StoreAsNormal(Items.Bsoda, appearsInStore: true, weight: 90, acceptableFloors: ["F1", "F2", "F3", "END"]);

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("SubspaceSoda")
			.SetGeneratorCost(35)
			.SetShopPrice(850)
			.SetMeta(ItemFlags.CreatesEntity | ItemFlags.Persists, ["drink", SODAVARTAG])
			.SetEnum("SubspaceSoda")
			.SetItemComponent<ITM_SubspaceSoda>(Items.Bsoda)
			.SetNameAndDescription("LtsOItems_SubspaceSoda_Name", "LtsOItems_SubspaceSoda_Desc")
			.BuildAndSetup()
			.StoreAsNormal(Items.Bsoda, appearsInStore: true, weight: 75, acceptableFloors: ["F2", "F3", "END"]);

			// ------ Apple Variants ------

			new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("HairSpray")
			.SetGeneratorCost(35)
			.SetShopPrice(1250)
			.SetMeta(ItemFlags.NoUses, ["food", APPLEVARTAG])
			.SetEnum("HairSpray")
			.SetItemComponent<Item>()
			.SetNameAndDescription("LtsOItems_HairSpray_Name", "LtsOItems_HairSpray_Desc")
			.BuildAndSetup()
			.StoreAsNormal(Items.Apple, appearsInStore: true, weight: 15, acceptableFloors: ["F2", "F3", "END"])
			.AddItemAsApple((baldi) =>
			{
				var nextState = new Baldi_CustomAppleState(baldi,
					baldi.behaviorStateMachine.CurrentState,
					TextureExtensions.LoadSpriteSheet(3, 1, 30.5f, LotOfItemsPlugin.ModPath, "HairSpray_BaldiHaired.png"),
					eatTime: 65f,
					eatSounds: [new() { selection = GenericExtensions.FindResourceObjectByName<SoundObject>("Scissors"), weight = 100 }],
					thanksAudio: GenericExtensions.FindResourceObjectByName<SoundObject>("BAL_Ohh"));

				var mainState = new Baldi_CustomAppleState(baldi,
					nextState,
					TextureExtensions.LoadSpriteSheet(2, 1, 32f, LotOfItemsPlugin.ModPath, "HairSpray_BaldiEatHair.png"),
					eatTime: 15f,
					thanksAudio: ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(LotOfItemsPlugin.ModPath, "HairSpray_BAL_spray.wav")), "LtsOItems_Vfx_BAL_Hair", SoundType.Voice, Color.green));
				return mainState;
			});

			item = new ItemBuilder(LotOfItemsPlugin.plug.Info)
			.AutoGetSprites("GreenApple")
			.SetGeneratorCost(30)
			.SetShopPrice(850)
			.SetMeta(ItemFlags.NoUses, ["food", APPLEVARTAG])
			.SetEnum("GreenApple")
			.SetItemComponent<ITM_Acceptable>()
			.SetNameAndDescription("LtsOItems_GreenApple_Name", "LtsOItems_GreenApple_Desc")
			.BuildAndSetup(out ITM_Acceptable itmAcceptable)
			.StoreAsNormal(Items.Apple, appearsInStore: true, weight: 85, acceptableFloors: ["F2", "F3", "END"])
			.AddItemAsApple((baldi) =>
					new Baldi_CustomAppleState(baldi,
					baldi.behaviorStateMachine.CurrentState,
					TextureExtensions.LoadSpriteSheet(2, 1, 32f, LotOfItemsPlugin.ModPath, "GreenApple_BaldiEat.png"),
					eatTime: 5f));

			GrowItemAcceptor.RegisterExchangingItem(item.itemType, ItemMetaStorage.Instance.FindByEnum(Items.Apple).value, 45f);
			itmAcceptable.item = item.itemType;
			itmAcceptable.audUse = GenericExtensions.FindResourceObjectByName<SoundObject>("ItemPickup");

			GenericExtensions.FindResourceObjects<RendererContainer>().DoIf(x => x.name.StartsWith("Plant"), plant =>
			{
				var acceptor = new GameObject("Plant_GreenAppleAcceptor").AddComponent<GrowItemAcceptor>();
				acceptor.transform.SetParent(plant.transform);
				acceptor.transform.localPosition = Vector3.zero;
				acceptor.gameObject.layer = LayerStorage.iClickableLayer;

				var collider = acceptor.gameObject.AddComponent<BoxCollider>();
				collider.center = Vector3.up * 5f;
				collider.size = new(1.5f, 5f, 1.5f);
				collider.isTrigger = true;

				acceptor.renderer = ObjectCreationExtensions.CreateSpriteBillboard(null);
				acceptor.renderer.name = "GrowRenderer";
				acceptor.renderer.transform.SetParent(acceptor.transform);
				acceptor.renderer.transform.localPosition = Vector3.right * 1.5f;

				acceptor.gameObject.AddComponent<BillboardUpdater>();

				acceptor.audMan = acceptor.gameObject.CreatePropagatedAudioManager(65f, 75f);
				acceptor.audEnd = GenericExtensions.FindResourceObjectByName<SoundObject>("Boink");
			});
		}

		static SoundObject GetYtpAudio(string name)
		{
			var sd = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(LotOfItemsPlugin.ModPath, name)), string.Empty, SoundType.Effect, Color.white);
			sd.subtitle = false;
			return sd;
		}

		static SoundObject GetGenericYtpAudio(int id) =>
			GenericExtensions.FindResourceObjectByName<SoundObject>("YTPPickup_" + id);

		static string GetItemIcon(string itemName, bool big) =>
			Path.Combine(LotOfItemsPlugin.ModPath, $"{itemName}_{(big ? "largeIcon" : "smallIcon")}.png");

		static ItemBuilder AutoGetSprites(this ItemBuilder bld, string itemName)
		{
			string smallPath = GetItemIcon(itemName, false),
				largePath = GetItemIcon(itemName, true);

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

		static ItemObject StoreAsNormal(this ItemObject itm, Items replacingItem, bool goToFieldTrips = false, bool appearsInStore = true, int weight = 100, params string[] acceptableFloors)
		{
			itm.item.name = "ITM_" + Singleton<LocalizationManager>.Instance.GetLocalizedText(itm.nameKey);
			LotOfItemsPlugin.plug.availableItems.Add(new(itm, replacingItem, goToFieldTrips, appearsInStore, weight, acceptableFloors));
			return itm;
		}

		static ItemBuilder SetItemComponent<T>(this ItemBuilder bld, Items item) where T : Item
		{
			bld.SetItemComponent(GetVariantInstance<T>(item)); // To make sure it doesn't create a new one lol

			return bld;
		}

		static T GetVariantInstance<T>(Items item) where T : Item
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

			return newItm;
		}
	}

	internal readonly struct ItemData(ItemObject itm, Items itemItIsReplacing, int weight = 100, params string[] acceptableFloors)
	{
		internal ItemData(ItemObject itm, Items itemItIsReplacing, bool acceptFieldTrips, bool appearsInStore, int weight = 100, params string[] acceptableFloors) : this(itm, itemItIsReplacing, weight, acceptableFloors)
		{
			this.acceptFieldTrips = acceptFieldTrips;
			this.appearsInStore = appearsInStore;
		}

		readonly public Items replacingItem = itemItIsReplacing;
		readonly public ItemObject itm = itm;
		readonly public int weight = Mathf.Max(1, weight);
		readonly public IItemPrefab Prefab => itm.item is IItemPrefab pre ? pre : null;
		readonly public HashSet<string> acceptableFloors = [.. acceptableFloors];
		readonly public bool acceptFieldTrips = false, appearsInStore = true;
	}


}
