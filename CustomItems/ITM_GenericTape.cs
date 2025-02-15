using UnityEngine;
using LotsOfItems.Patches;
using System.Collections;
using LotsOfItems.ItemPrefabStructures;

namespace LotsOfItems.CustomItems
{
	public abstract class ITM_GenericTape : Item, IItemPrefab
	{
		public void SetupPrefab(ItemObject itm) =>
			VirtualSetupPrefab(itm);
		public void SetupPrefabPost() { }
		protected virtual void VirtualSetupPrefab(ItemObject itm) { }

		[SerializeField]
		internal SoundObject[] audioToOverride = null;

		[SerializeField]
		internal bool useOriginalTapePlayerFunction = false;
		protected virtual IEnumerator NewCooldown(TapePlayer tapePlayer) => null;

		public override bool Use(PlayerManager pm)
		{
			if (Physics.Raycast(pm.transform.position, Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, out hit, pm.pc.reach, pm.pc.ClickLayers))
			{
				var component = hit.transform.GetComponent<TapePlayer>();
				if (component != null && component.ItemFits(Items.Tape))
				{	
					component.CustomInsertItem(pm, pm.ec, NewCooldown(component), audioToOverride, useOriginalTapePlayerFunction);
					Destroy(gameObject);
					return true;
				}
			}
			Destroy(gameObject);
			return false;
		}

		private RaycastHit hit;
	}
}
