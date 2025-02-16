using UnityEngine;
using HarmonyLib;

namespace LotsOfItems.CustomItems.Eatables
{
	public class ITM_PanicKernels : ITM_GenericZestyEatable
	{
		protected override void VirtualSetupPrefab(ItemObject itemObject)
		{
			base.VirtualSetupPrefab(itemObject);
			PanicKernelsPatches.itemEnum = itemObject.itemType;
		}
	}

	[HarmonyPatch]
	static class PanicKernelsPatches
	{
		public static Items itemEnum;
		static bool canInstantiateSodas = false;

		[HarmonyPostfix]
		[HarmonyPatch(typeof(ITM_BSODA), nameof(ITM_BSODA.Use))]
		static void BSodaPostfix(bool __result, PlayerManager pm)
		{
			int idx = pm.itm.FindKernel();
			if (!canInstantiateSodas || !__result || idx == -1)
			{
				canInstantiateSodas = false;
				return;
			}
			pm.itm.RemoveItem(idx);

			canInstantiateSodas = false;
			Vector3 spawnPos = pm.transform.position;
			Quaternion baseRot = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.rotation;

			// Create angled variants
			CreateAngledProjectile(pm, (ITM_BSODA)pm.itm.items[pm.itm.selectedItem].item, spawnPos, baseRot * Quaternion.Euler(0, 45, 0));
			CreateAngledProjectile(pm, (ITM_BSODA)pm.itm.items[pm.itm.selectedItem].item, spawnPos, baseRot * Quaternion.Euler(0, -45, 0));

			
		}

		static int FindKernel(this ItemManager itm)
		{
			for (int i = 0; i <=itm. maxItem; i++)
				if (itm.items[i].itemType == itemEnum && !itm. slotLocked[i])
					return i;
			
			return -1;
		}

		static void CreateAngledProjectile(PlayerManager pm, ITM_BSODA original, Vector3 position, Quaternion rotation)
		{
			// Duplicate BSODA entity
			ITM_BSODA newProjectile = Object.Instantiate(original, position, rotation);

			newProjectile.Use(pm);
			newProjectile.transform.rotation = rotation;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(ItemManager), nameof(ItemManager.UseItem))]
		static void AllowSodaInstantiation() =>
			canInstantiateSodas = true;

		[HarmonyPostfix]
		[HarmonyPatch(typeof(ItemManager), nameof(ItemManager.UseItem))]
		static void DisllowSodaInstantiation() =>
			canInstantiateSodas = false;
	}
}