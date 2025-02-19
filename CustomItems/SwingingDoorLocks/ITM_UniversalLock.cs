using LotsOfItems.ItemPrefabStructures;
using UnityEngine;
using HarmonyLib;

namespace LotsOfItems.CustomItems.SwingingDoorLocks
{
	public class ITM_UniversalLock : Item, IItemPrefab
	{
		[SerializeField]
		internal Items doorLockItem = Items.DoorLock, myItem;

		[SerializeField]
		internal float lockTimer = 20f;

		private RaycastHit hit;

		public void SetupPrefab(ItemObject itm) =>
			myItem = itm.itemType;
		public void SetupPrefabPost() { }

		public override bool Use(PlayerManager pm)
		{
			this.pm = pm;
			if (Physics.Raycast(pm.transform.position,
				Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward,
				out hit, pm.pc.reach, pm.pc.ClickLayers))
			{
				StandardDoor door = hit.transform.GetComponent<StandardDoor>();
				if (door != null && !door.locked)
				{
					if (!door.GetComponent<DoorActuallyBlockedMarker>())
						door.gameObject.AddComponent<DoorActuallyBlockedMarker>();
					door.Shut();
					door.LockTimed(lockTimer);
					Destroy(gameObject);
					return true;
				}

				IItemAcceptor acceptor = hit.transform.GetComponent<IItemAcceptor>();
				if (acceptor != null && (acceptor.ItemFits(myItem) || acceptor.ItemFits(doorLockItem)))
				{
					acceptor.InsertItem(pm, pm.ec);
					Destroy(gameObject);
					return true;
				}
			}
			return false;
		}
	}

	internal class DoorActuallyBlockedMarker : MonoBehaviour { }

	[HarmonyPatch(typeof(Door))]
	internal static class DoorActualLockPatch
	{
		[HarmonyPrefix]
		[HarmonyPatch("Lock")]
		static void ActualLock(Door __instance, ref bool ___lockBlocks)
		{
			if (!___lockBlocks)
				___lockBlocks = __instance.GetComponent<DoorActuallyBlockedMarker>();
		}

		[HarmonyPrefix]
		[HarmonyPatch("Unlock")]
		static void ActualUnlockPre(out bool __state, bool ___lockBlocks) =>
			__state = ___lockBlocks;
		[HarmonyPostfix]
		[HarmonyPatch("Unlock")]
		static void ActualUnlock(Door __instance, bool __state, ref bool ___lockBlocks)
		{
			var marker = __instance.GetComponent<DoorActuallyBlockedMarker>();
			if (marker) // If closeBlocks was previously false (__state), it'll destroy the marker since it actually opens
			{
				Object.Destroy(marker);
				___lockBlocks = __state;
			}

			
		}
	}
}
