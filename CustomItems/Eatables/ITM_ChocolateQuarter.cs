using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.Eatables
{
	public class ITM_ChocolateQuarter : ITM_GenericZestyEatable
	{
		protected override void VirtualSetupPrefab(ItemObject itemObject)
		{
			base.VirtualSetupPrefab(itemObject);
			audUse = GenericExtensions.FindResourceObjectByName<SoundObject>("CoinDrop");
		}
		public override bool Use(PlayerManager pm)
		{
			if (Physics.Raycast(pm.transform.position, Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, out hit, pm.pc.reach, pm.pc.ClickLayers))
			{
				IItemAcceptor component = hit.transform.GetComponent<IItemAcceptor>();
				if (component != null && component.ItemFits(quarterType))
				{
					Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audUse);
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

		[SerializeField]
		internal SoundObject audUse;
	}
}
