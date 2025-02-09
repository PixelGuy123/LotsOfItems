using UnityEngine;
using LotsOfItems.ItemPrefabStructures;
using HarmonyLib;

namespace LotsOfItems.CustomItems.Key
{
	public class ITM_KillerKey : Item, IItemPrefab
	{
		[SerializeField]
		private Items item;

		// SetupPrefab assigns the key use sound.
		public void SetupPrefab(ItemObject itm)
		{
			item = itm.itemType;
		}
		public void SetupPrefabPost() { }

		public override bool Use(PlayerManager pm)
		{
			this.pm = pm;
			// Perform a raycast from the player's position in the camera's forward direction.
			if (Physics.Raycast(pm.transform.position,
				Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward,
				out RaycastHit hit, pm.pc.reach, pm.pc.ClickLayers))
			{
				IItemAcceptor acceptor = hit.transform.GetComponent<IItemAcceptor>();
				if (acceptor != null && acceptor.ItemFits(item))
				{
					StandardDoor door = hit.transform.GetComponent<StandardDoor>();
					if (door != null && door.locked)
					{
						door.InsertItem(pm, pm.ec);
						if (!door.GetComponent<StandardDoor_NeverLockMarker>())
							door.gameObject.AddComponent<StandardDoor_NeverLockMarker>();

						Destroy(gameObject);
						return true;
					}

					acceptor.InsertItem(pm, pm.ec);
					Destroy(gameObject);
					return true;
				}
			}
			return false;
		}
	}

	public class StandardDoor_NeverLockMarker : MonoBehaviour { }

	[HarmonyPatch(typeof(StandardDoor))]
	internal static class DoorPermanentlyUnlockedPatch
	{
		[HarmonyPatch("Lock", [typeof(bool)])]
		[HarmonyPrefix]
		static bool MakeSureToNeverLock(StandardDoor __instance, bool ___locked)
		{
			if (!___locked && __instance.gameObject.GetComponent<StandardDoor_NeverLockMarker>())
			{
				return false; // Skip the original Lock() call.
			}
			return true; // Continue with the original method.
		}
	}
}
