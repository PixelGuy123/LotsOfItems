using UnityEngine;
using HarmonyLib;
using LotsOfItems.ItemPrefabStructures;
using UnityEngine.UI;

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
		internal virtual bool EntityTriggerStayOverride(Collider other) => true;
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

		[HarmonyPatch("EntityTriggerStay")]
		[HarmonyPrefix]
		static bool ActuallyWorksAsNanaPeel(ITM_NanaPeel __instance, Collider other, bool ___ready, bool ___slipping)
		{
			if (__instance is ITM_GenericNanaPeel gen && ___ready && !___slipping)
				return gen.EntityTriggerStayOverride(other);
			return true;
		}

		[HarmonyPatch("Update")]
		[HarmonyPrefix]
		static void MakeSureFloorHitIsCorrect(out bool __state, bool ___ready) =>
			__state = ___ready;

		[HarmonyPatch("Update")]
		[HarmonyPostfix]
		static void FloorHitCallIsCorrect(ITM_NanaPeel __instance, bool __state, bool ___ready)
		{
			if (__instance is ITM_GenericNanaPeel gen && ___ready && __state != ___ready)
				gen.OnFloorHit();
		}
	}
}
