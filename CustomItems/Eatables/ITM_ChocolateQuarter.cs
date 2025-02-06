using UnityEngine;

namespace LotsOfItems.CustomItems.Eatables
{
	public class ITM_ChocolateQuarter : ITM_GenericZestyEatable
	{
		public override bool Use(PlayerManager pm)
		{
			if (Physics.Raycast(pm.transform.position, Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, out hit, pm.pc.reach, pm.pc.ClickLayers))
			{
				IItemAcceptor component = hit.transform.GetComponent<IItemAcceptor>();
				if (component != null && component.ItemFits(quarterType))
				{
					component.InsertItem(pm, pm.ec);
					Destroy(gameObject);
					return true;
				}
			}
			
			return base.Use(pm);
		}

		RaycastHit hit;

		[SerializeField]
		internal Items quarterType = Items.Quarter;
	}
}
