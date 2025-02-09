using UnityEngine;
using LotsOfItems.ItemPrefabStructures;

namespace LotsOfItems.CustomItems.SwingingDoorLocks
{
	public class ITM_UniversalLock : Item, IItemPrefab
	{
		[SerializeField]
		internal Items doorLockItem = Items.DoorLock, myItem;

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
				IItemAcceptor acceptor = hit.transform.GetComponent<IItemAcceptor>();
				if (acceptor != null && (acceptor.ItemFits(myItem) || acceptor.ItemFits(doorLockItem)))
				{
					StandardDoor door = hit.transform.GetComponent<StandardDoor>();
					if (door != null)
					{
						door.Lock(false);
						Destroy(gameObject);
						return true;
					}
					else
					{
						acceptor.InsertItem(pm, pm.ec);
						Destroy(gameObject);
						return true;
					}
				}
			}
			return false;
		}
	}
}
