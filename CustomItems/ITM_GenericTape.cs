﻿using UnityEngine;
using LotsOfItems.Patches;
using System.Collections;
using LotsOfItems.ItemPrefabStructures;

namespace LotsOfItems.CustomItems
{
	public class ITM_GenericTape : Item, IItemPrefab
	{
		public void SetupPrefab(ItemObject itm) =>
			VirtualSetupPrefab(itm);
		public void SetupPrefabPost() { }
		protected virtual void VirtualSetupPrefab(ItemObject itm) { }

		[SerializeField]
		internal SoundObject[] audioToOverride = null;
		protected virtual IEnumerator NewCooldown(TapePlayer tapePlayer) => null;

		public override bool Use(PlayerManager pm)
		{
			if (Physics.Raycast(pm.transform.position, Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, out hit, pm.pc.reach, pm.pc.ClickLayers))
			{
				var component = hit.transform.GetComponent<TapePlayer>();
				if (component != null && component.ItemFits(Items.Tape))
				{	
					component.OverridenInsertItem(pm, pm.ec, NewCooldown(component), audioToOverride);
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
