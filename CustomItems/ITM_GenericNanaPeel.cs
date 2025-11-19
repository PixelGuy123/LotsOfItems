using HarmonyLib;
using LotsOfItems.ItemPrefabStructures;
using UnityEngine;

namespace LotsOfItems.CustomItems
{
	public abstract class ITM_GenericNanaPeel : ITM_NanaPeel, IItemPrefab
	{
		public void SetupPrefab(ItemObject itm) =>
			VirtualSetupPrefab(itm);

		public void SetupPrefabPost() { }
		protected virtual void VirtualSetupPrefab(ItemObject itm) { }
		internal virtual void AdditionalSpawnContribute() { }
		internal virtual bool OnCollisionOverride(RaycastHit hit) => true;
		internal virtual void OnFloorHit() { }
		internal virtual bool EntityTriggerStayOverride(Collider other, bool validCollision) => true;
		internal virtual void EntityTriggerEnterOverride(Collider other, bool validCollision) { }
		internal virtual bool EntityTriggerExitOverride(Collider other, bool validCollision) => true;
		internal virtual void VirtualUpdate() { }
		internal virtual bool VirtualEnd() => true;
		internal virtual bool DisableUpdate => false;
	}

	[HarmonyPatch(typeof(ITM_NanaPeel))]
	internal static class NanaPeelPatch
	{
		[HarmonyPatch("Spawn")]
		[HarmonyPostfix]
		static void OverrideSpawn(ITM_NanaPeel __instance)
		{
			if (__instance is ITM_GenericNanaPeel gen)
				gen.AdditionalSpawnContribute();
		}

		[HarmonyPatch("OnEntityMoveCollision")]
		[HarmonyPrefix]
		static bool CollisionEnterOverride(ITM_NanaPeel __instance, RaycastHit hit)
		{
			if (__instance is ITM_GenericNanaPeel gen)
				return gen.OnCollisionOverride(hit);
			return true;
		}

		[HarmonyPatch("End")]
		[HarmonyPrefix]
		static bool EndOverride(ITM_NanaPeel __instance)
		{
			if (__instance is ITM_GenericNanaPeel gen)
				return gen.VirtualEnd();
			return true;
		}

		[HarmonyPatch("EntityTriggerStay")]
		[HarmonyPrefix]
		static bool ActuallyWorksAsNanaPeel(ITM_NanaPeel __instance, Collider other, bool ___ready, bool ___slipping, bool validCollision)
		{
			if (__instance is ITM_GenericNanaPeel gen && ___ready && !___slipping)
				return gen.EntityTriggerStayOverride(other, validCollision);
			return true;
		}

		[HarmonyPatch("EntityTriggerEnter")]
		[HarmonyPrefix]
		static void TriggerEnterExtra(ITM_NanaPeel __instance, Collider other, bool ___ready, bool ___slipping, bool validCollision)
		{
			if (__instance is ITM_GenericNanaPeel gen && ___ready && !___slipping)
				gen.EntityTriggerEnterOverride(other, validCollision);
		}

		[HarmonyPatch("EntityTriggerExit")]
		[HarmonyPrefix]
		static bool TriggerExitExtra(ITM_NanaPeel __instance, Collider other, bool ___ready, bool ___slipping, bool validCollision)
		{
			if (__instance is ITM_GenericNanaPeel gen && ___ready && ___slipping)
				return gen.EntityTriggerExitOverride(other, validCollision);
			return true;
		}

		[HarmonyPatch("Update")]
		[HarmonyPrefix]
		static bool MakeSureFloorHitIsCorrect(ITM_NanaPeel __instance, out bool __state, bool ___ready)
		{
			__state = ___ready;
			if (__instance is ITM_GenericNanaPeel gen)
			{
				gen.VirtualUpdate();
				if (gen.DisableUpdate)
					return false;
			}
			return true;
		}

		[HarmonyPatch("Update")]
		[HarmonyPostfix]
		static void FloorHitCallIsCorrect(ITM_NanaPeel __instance, bool __state, bool ___ready)
		{
			if (__instance is ITM_GenericNanaPeel gen && ___ready && __state != ___ready)
				gen.OnFloorHit();
		}
	}
}
