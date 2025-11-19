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
				_current = hit.transform.GetComponent<IItemAcceptor>();
				if (_current != null && _current.ItemFits(quarterType))
				{
					return true;
				}
			}

			return base.Use(pm);
		}

		public override void PostUse(PlayerManager pm)
		{
			base.PostUse(pm);
			if (_current == null) return;

			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audUse);
			_current.InsertItem(pm, pm.ec);
			Destroy(gameObject);
		}


		private RaycastHit hit;
		private IItemAcceptor _current;

		[SerializeField]
		internal Items quarterType = Items.Quarter;

		[SerializeField]
		internal SoundObject audUse;
	}
}
