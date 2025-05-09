﻿using UnityEngine;
using System.Collections;
using LotsOfItems.ItemPrefabStructures;
using PixelInternalAPI.Extensions;

namespace LotsOfItems.CustomItems.Quarters
{
	public class ITM_QuarterOnAString : Item, IItemPrefab
	{
		[SerializeField]
		internal Items quarterType = Items.Quarter;

		[SerializeField]
		internal ItemObject nextItem = null;

		[SerializeField]
		internal SoundObject audUse;

		public void SetupPrefab(ItemObject itm) =>
			audUse = GenericExtensions.FindResourceObjectByName<SoundObject>("CoinDrop");

		public void SetupPrefabPost() { }

		public override bool Use(PlayerManager pm)
		{
			if (pm.itm.InventoryFull())
				return false;

			if (Physics.Raycast(pm.transform.position,
			Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward,
			out RaycastHit hit, pm.pc.reach, pm.pc.ClickLayers))
			{
				IItemAcceptor component = hit.transform.GetComponent<IItemAcceptor>();
				if (component != null && component.ItemFits(quarterType))
				{
					Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audUse);
					pm.RuleBreak("Bullying", 3.5f, 1f);
					component.InsertItem(pm, pm.ec);

					if (nextItem) // If the item field is not null, there are still uses left
					{
						this.pm = pm;
						StartCoroutine(Delay()); // use a Delay() to wait for the next frame, to re-add the item back into the inventory
						return false;
					}
					return true;
				}
			}


			Destroy(gameObject);
			return false;
		}

		IEnumerator Delay()
		{
			yield return null; // Waits a frame/screen refresh

			pm.itm.SetItem(nextItem, pm.itm.selectedItem); // Set item back
			Destroy(gameObject);
		}

		
	}
}
