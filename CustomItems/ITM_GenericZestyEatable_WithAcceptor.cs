using UnityEngine;

namespace LotsOfItems.CustomItems;

public class ITM_GenericZestyEatable_WithAcceptor : ITM_GenericZestyEatable
{
	[SerializeField]
	internal Items acceptableItem;

	protected override void VirtualSetupPrefab(ItemObject itemObject)
	{
		base.VirtualSetupPrefab(itemObject);
		acceptableItem = itemObject.itemType;
	}

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
		}
		return base.Use(pm);
	}
}

