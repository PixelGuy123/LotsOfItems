using System.Collections;
using System.Collections.Generic;
using LotsOfItems.Components;
using LotsOfItems.ItemPrefabStructures;
using MTM101BaldAPI;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.Eatables
{
	public class ITM_IceZestyBar : ITM_GenericZestyEatable
	{
		[SerializeField]
		internal SlippingObject slipObjPre;

		[SerializeField]
		internal float lifeTime = 15f;

		readonly HashSet<Cell> visitedCells = []; // To make sure it doesn't spawn ice where it shouldn't twice
		protected override void VirtualSetupPrefab(ItemObject itemObject)
		{
			base.VirtualSetupPrefab(itemObject);

			var slipObj = ObjectCreationExtensions.CreateSpriteBillboard(this.GetSprite("iceZestyBar_ice.png", 25.15f), false).AddSpriteHolder(out var slipRend, 0.06f, LayerStorage.ignoreRaycast); // y offset of 0.06f
			slipObj.name = "IceZestySlippingObject";
			slipRend.gameObject.layer = 0;
			slipRend.name = "SlippingObjectSprite";

			slipRend.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
			slipObj.gameObject.ConvertToPrefab(true);

			slipObjPre = slipObj.gameObject.AddComponent<SlippingObject>();
			slipObjPre.audMan = slipObj.gameObject.CreatePropagatedAudioManager(65f, 75f);
			slipObjPre.audSlip = GenericExtensions.FindResourceObjectByName<SoundObject>("Nana_Slip");

			var collider = slipObjPre.gameObject.AddComponent<BoxCollider>();
			collider.isTrigger = true;
			collider.center = Vector3.up * 5f;
			collider.size = new(4.5f, 5f, 4.5f);
		}

		protected override bool CanBeDestroyed() =>
			false; // Avoid being destroyed for a while

		public override bool Use(PlayerManager pm)
		{
			this.pm = pm;
			gauge = Singleton<CoreGameManager>.Instance.GetHud(pm.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, lifeTime);
			StartCoroutine(ActiveItem());

			return base.Use(pm);
		}

		IEnumerator ActiveItem()
		{
			float time = lifeTime;

			while (time > 0f) // Stops when 
			{
				var cell = pm.ec.CellFromPosition(pm.transform.position);
				if (!cell.Null && !visitedCells.Contains(cell))
				{
					visitedCells.Add(cell);
					var slip = Instantiate(slipObjPre);
					slip.transform.SetParent(transform); // Set parent to the item itself, when destroyed, the "slippers" go too
					slip.transform.position = cell.FloorWorldPosition;
					slip.SetAnOwner(pm.gameObject);
				}
				time -= pm.ec.EnvironmentTimeScale * Time.deltaTime;
				gauge.SetValue(lifeTime, time);
				yield return null;
			}
			gauge.Deactivate();
			Destroy(gameObject); // Should go with the slipObjPre too
		}
	}
}
