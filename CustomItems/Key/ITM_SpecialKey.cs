using UnityEngine;
using LotsOfItems.ItemPrefabStructures;

namespace LotsOfItems.CustomItems.Key
{
	public class ITM_SpecialKey : Item, IItemPrefab
	{
		[SerializeField]
		internal Items acceptableItem;

		public void SetupPrefab(ItemObject itm) =>
			acceptableItem = itm.itemType;
		public void SetupPrefabPost() { }

		public override bool Use(PlayerManager pm)
		{
			if (Physics.Raycast(pm.transform.position,
				Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward,
				out RaycastHit hit, pm.pc.reach))
			{
				IItemAcceptor acceptor = hit.transform.GetComponent<IItemAcceptor>();
				if (acceptor != null && acceptor.ItemFits(acceptableItem))
				{
					acceptor.InsertItem(pm, pm.ec);
					Destroy(gameObject);
					return true;
				}

				var swingingDoor = hit.transform.GetComponent<SwingDoor>();
				if (swingingDoor && swingingDoor.locked)
				{
					swingingDoor.Unlock();
					Destroy(gameObject);
					return true;
				}
			}
			return false;
		}
	}
}
