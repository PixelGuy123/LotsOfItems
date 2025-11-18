using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace LotsOfItems.CustomItems.Eatables
{
	public class ITM_PanicKernels : ITM_GenericZestyEatable
	{
		protected override void VirtualSetupPrefab(ItemObject itemObject)
		{
			base.VirtualSetupPrefab(itemObject);
			PanicKernelsPatches_ITM_BSODA.itemEnum = itemObject.itemType;
		}
	}

	[HarmonyPatch]
	static class PanicKernelsPatch_IBsodaShooter
	{
		readonly static Dictionary<System.Type, MethodInfo> _shooterCache = [];

		[HarmonyPrepare]
		// Prepare the method beforehand, to be sure anything was found first
		static bool PrepareGettingMethods()
		{
			var interfaceType = typeof(IBsodaShooter);
			var implementingTypes = AccessTools.AllTypes().Where(type => interfaceType.IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);

			if (!implementingTypes.Any())
			{
				Debug.Log("No types implementing IBsodaShooter found.");
				return false; // Prevents the patch from being applied if no targets are found
			}

			foreach (var type in implementingTypes)
			{
				if (_shooterCache.ContainsKey(type)) continue;

				var method = AccessTools.Method(type, "ShootBsoda");
				if (method != null)
				{
					_shooterCache.Add(type, method);
					Debug.Log($"{type.FullName}::{method.Name}");
				}
				else
				{
					Debug.LogWarning($"Type {type.FullName} implements IBsodaShooter but no ShootBsoda method was found.");
				}
			}

			return _shooterCache.Any(); // Only proceed with patching if at least one target method was found
		}

		[HarmonyTargetMethods]
		static IEnumerable<MethodBase> GetShooterMethods() => _shooterCache.Values;

		[HarmonyPostfix]
		static void ForceBSODAShoot(object __instance, ITM_BSODA bsoda, PlayerManager pm, Vector3 position, Quaternion rotation)
		{
			int idx = pm.itm.FindKernel();
			if (!PanicKernelsPatches_ITM_BSODA.canInstantiateSodas || idx == -1)
			{
				PanicKernelsPatches_ITM_BSODA.canInstantiateSodas = false;
				return;
			}
			pm.itm.RemoveItem(idx);

			PanicKernelsPatches_ITM_BSODA.canInstantiateSodas = false;
			Vector3 spawnPos = position;
			Quaternion baseRot = rotation;

			var instType = __instance.GetType();

			// calls this patched method twice
			_shooterCache[instType].Invoke(__instance, [bsoda, pm, spawnPos, baseRot * Quaternion.Euler(0, 45, 0)]);
			_shooterCache[instType].Invoke(__instance, [bsoda, pm, spawnPos, baseRot * Quaternion.Euler(0, -45, 0)]);
		}
	}

	[HarmonyPatch]
	static class PanicKernelsPatches_ITM_BSODA
	{
		public static Items itemEnum;
		internal static bool canInstantiateSodas = false;

		[HarmonyPostfix]
		[HarmonyPatch(typeof(ITM_BSODA), nameof(ITM_BSODA.Use))]
		static void BSODAPostfix(bool __result, PlayerManager pm)
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

		internal static int FindKernel(this ItemManager itm)
		{
			for (int i = 0; i <= itm.maxItem; i++)
				if (itm.items[i].itemType == itemEnum && !itm.slotLocked[i])
					return i;

			return -1;
		}

		static void CreateAngledProjectile(PlayerManager pm, ITM_BSODA original, Vector3 position, Quaternion rotation)
		{
			// Duplicate BSODA entity
			ITM_BSODA newProjectile = Object.Instantiate(original, position, rotation);

			if (newProjectile is ITM_GenericBSODA generic)
				generic.SetOriginalRotation(rotation);

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