using HarmonyLib;
using LotsOfItems.ItemPrefabStructures;
using UnityEngine;

namespace LotsOfItems.CustomItems
{
	public class ITM_GenericGrapplingHook : ITM_GrapplingHook, IEntityTrigger, IItemPrefab
	{
		public void SetupPrefab(ItemObject itm) =>
			VirtualSetupPrefab(itm);
		public void SetupPrefabPost() { }
		protected virtual void VirtualSetupPrefab(ItemObject itm) { }
		public virtual void VirtualUpdate() { }
		public virtual bool VirtualPreUpdate() => true;
		public virtual bool OnWallHitOverride(RaycastHit hit) => true;
		public virtual void VirtualEnd() { }

		public void ForceStop()
		{
			if (locked) return;

			locked = true;
			entity.SetFrozen(true);
			force = initialForce;
			initialDistance = (transform.position - pm.transform.position).magnitude;
			
		}
		public void ForceStop(Vector3 crackRotation, bool motorAudio)
		{
			ForceStop();
			audMan.PlaySingle(audClang);
			if (motorAudio)
				this.motorAudio.Play();
			cracks.rotation = Quaternion.Euler(crackRotation);
			cracks.gameObject.SetActive(true);
		}
		public void ForceSnap()
		{
			if (!locked) return;
			snapped = true;
			audMan.FlushQueue(true);
			audMan.QueueAudio(audSnap);
			motorAudio.Stop();
			lineRenderer.enabled = false;
			pm.Am.moveMods.Remove(moveMod);
			StartCoroutine(WaitForAudio());
		}
		public virtual void EntityTriggerEnter(Collider other) { }
		public virtual void EntityTriggerStay(Collider other) { }
		public virtual void EntityTriggerExit(Collider other) { }
	}

	[HarmonyPatch(typeof(ITM_GrapplingHook))]
	public class ITM_GenericGrapplingHook_Patches
	{

		[HarmonyPatch("OnEntityMoveCollision")]
		[HarmonyPrefix]
		static bool GrapWallOverride(ITM_GrapplingHook __instance, RaycastHit hit, bool ___locked)
		{
			if (!___locked && __instance is ITM_GenericGrapplingHook genericHook)
			{
				return genericHook.OnWallHitOverride(hit);
			}
			return true;
		}

		[HarmonyPatch("Update")]
		[HarmonyPostfix]
		static void VirtualUpdateCall(ITM_GrapplingHook __instance)
		{
			if (__instance is ITM_GenericGrapplingHook genericHook)
				genericHook.VirtualUpdate();
		}

		[HarmonyPatch("Update")]
		[HarmonyPrefix]
		static bool VirtualPreUpdateCall(ITM_GrapplingHook __instance)
		{
			if (__instance is ITM_GenericGrapplingHook genericHook)
				return genericHook.VirtualPreUpdate();
			return true;
		}

		[HarmonyPatch("End")]
		[HarmonyPrefix]
		static void VirtualEndCall(ITM_GrapplingHook __instance)
		{
			if (__instance is ITM_GenericGrapplingHook genericHook)
				genericHook.VirtualEnd();
		}
	}
}
