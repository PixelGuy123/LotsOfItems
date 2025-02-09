using UnityEngine;

namespace LotsOfItems.CustomItems.Scissors
{
	public class ITM_MetalScissors : ITM_Scissors
	{
		[SerializeField]
		internal Items keyEnum = Items.DetentionKey;
		public override bool Use(PlayerManager pm)
		{
			this.pm = pm;

			if (Physics.Raycast(pm.transform.position,
				Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward,
				out RaycastHit hit, pm.pc.reach))
			{
				IItemAcceptor acceptor = hit.transform.GetComponent<IItemAcceptor>();
				if (acceptor != null && acceptor.ItemFits(keyEnum))
				{
					acceptor.InsertItem(pm, pm.ec);
					Destroy(gameObject);
					return true;
				}
			}

			return base.Use(pm);
		}
	}
}
