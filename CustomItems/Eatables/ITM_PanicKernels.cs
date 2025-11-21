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
		readonly static List<MethodInfo> useItemCache = [];
		readonly static Dictionary<System.Type, PropertyInfo> _shooterPropertyCache = [];

		[HarmonyPrepare]
		// Prepare the method beforehand, to be sure anything was found first
		static bool PrepareGettingMethods()
		{
			var interfaceType = typeof(IBsodaShooter);
			var implementingTypes = AccessTools.GetTypesFromAssembly(interfaceType.Assembly).Where(type => interfaceType.IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);

			if (!implementingTypes.Any())
			{
				// Debug.Log("No types implementing IBsodaShooter found.");
				return false; // Prevents the patch from being applied if no targets are found
			}

			foreach (var type in implementingTypes)
			{
				if (_shooterPropertyCache.ContainsKey(type)) continue;

				var method = AccessTools.Method(type, nameof(Item.Use)); // Get item use to instantiate it
				if (method != null)
				{
					useItemCache.Add(method);
					var property = AccessTools.Property(type, nameof(IBsodaShooter.PanicKernelRotationOffset));
					_shooterPropertyCache.Add(type, property);

					// Debug.Log($"{type.FullName}::{method.Name}\n{type.FullName}::{property.Name}");
				}
				else
				{
					// Debug.LogWarning($"Type {type.FullName} implements IBsodaShooter but no ShootBsoda method was found.");
				}
			}

			return _shooterPropertyCache.Any(); // Only proceed with patching if at least one target method was found
		}

		[HarmonyTargetMethods]
		static IEnumerable<MethodBase> GetShooterMethods() => useItemCache;

		[HarmonyPostfix]
		static void ForceBSODAShoot(bool __result, Item __instance, PlayerManager pm)
		{
			if (_executingPatch) return;

			var instType = __instance.GetType();

			int idx = pm.itm.FindKernel();
			if (!__result || !PanicKernelsPatches_ITM_BSODA.canInstantiateSodas || idx == -1) // If there are any kernels
			{
				PanicKernelsPatches_ITM_BSODA.canInstantiateSodas = false;
				return;
			}
			pm.itm.RemoveItem(idx);

			PanicKernelsPatches_ITM_BSODA.canInstantiateSodas = false;

			// Get ItemObject to be used again
			var usedItemObject = PanicKernelsPatches_ITM_BSODA.lastUsedItem;
			try
			{
				_executingPatch = true;
				var setMethod = _shooterPropertyCache[instType].SetMethod;

				// Add offsets for these items
				MakeItem(45f);
				MakeItem(-45f);

				void MakeItem(float angle)
				{
					// Do proper instantiation here
					var newItem = Object.Instantiate(usedItemObject.item);
					// Set a new rotation offset for this item
					setMethod.Invoke(newItem, [Quaternion.Euler(0f, angle, 0f)]);
					// Use item
					newItem.Use(pm);
				}
			}
			finally
			{
				_executingPatch = false; // Safely sets this to false again
			}

		}

		[System.ThreadStatic] // Makes this field not share the same value between threads, differentiating uniquely to each other. https://learn.microsoft.com/en-us/dotnet/api/system.threadstaticattribute?view=net-9.0
		static bool _executingPatch = false;
	}

	[HarmonyPatch]
	static class PanicKernelsPatches_ITM_BSODA
	{
		public static Items itemEnum;
		internal static bool canInstantiateSodas = false;

		[HarmonyPostfix]
		[HarmonyPatch(typeof(ITM_BSODA), nameof(ITM_BSODA.Use))]
		[HarmonyPatch(typeof(ITM_GenericBSODA), nameof(ITM_GenericBSODA.Use))]
		static void BSODAPostfix(object __instance, bool __result, PlayerManager pm)
		{
			if (__instance is IBsodaShooter) // Failsafe, if the bsoda shooter is implemented directly into the item
				return;

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
			CreateAngledProjectile(pm, (ITM_BSODA)lastUsedItem.item, spawnPos, baseRot * Quaternion.Euler(0, 45, 0));
			CreateAngledProjectile(pm, (ITM_BSODA)lastUsedItem.item, spawnPos, baseRot * Quaternion.Euler(0, -45, 0));
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
		static void AllowSodaInstantiation(ItemManager __instance)
		{
			lastUsedItem = __instance.items[__instance.selectedItem];
			canInstantiateSodas = true;
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(ItemManager), nameof(ItemManager.UseItem))]
		static void DisllowSodaInstantiation() =>
			canInstantiateSodas = false;

		public static ItemObject lastUsedItem;
	}
}