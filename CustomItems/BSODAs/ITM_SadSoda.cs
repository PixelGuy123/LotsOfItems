using UnityEngine;
using LotsOfItems.Components;
using System.Collections;
using PixelInternalAPI.Classes;
using LotsOfItems.ItemPrefabStructures;

namespace LotsOfItems.CustomItems.BSODAs
{
	public class ITM_SadSoda : ITM_GenericBSODA
	{
		[SerializeField]
		private NPC targetNpc;

		[SerializeField]
		private MomentumNavigator momentumNav;

		[SerializeField]
		private float detectionDistance = 9999f, nearbyNpcDistance = 5f;

		private bool hasDetectedTarget;

		protected override void VirtualSetupPrefab(ItemObject itm)
		{
			base.VirtualSetupPrefab(itm);
			momentumNav = gameObject.AddComponent<MomentumNavigator>();
			momentumNav.maxSpeed = speed;

			spriteRenderer.sprite = this.GetSprite("SadSoda_drink.png", spriteRenderer.sprite.pixelsPerUnit);
		}

		public override bool Use(PlayerManager pm)
		{
			base.Use(pm);

			// NPC detection logic
			if (Physics.Raycast(pm.transform.position, pm.transform.forward, out var hit, detectionDistance, LayerStorage.principalLookerMask))
			{
				targetNpc = hit.collider.GetComponent<NPC>();
				if (targetNpc != null)
				{
					hasDetectedTarget = true;
					momentumNav.Initialize(ec, false);
					StartCoroutine(UpdatePath());
				}
			}
			return true;
		}

		private IEnumerator UpdatePath()
		{
			while (targetNpc && targetNpc.gameObject.activeSelf)
			{
				momentumNav.FindPath(targetNpc.transform.position);
				yield return null;
			}

			hasDetectedTarget = false;
			momentumNav.ClearDestination();
		}

		public override void VirtualUpdate()
		{
			if (!hasDetectedTarget)
			{
				// Fallback to normal BSODA behavior
				base.VirtualUpdate();
				return;
			}

			time -= Time.deltaTime * ec.EnvironmentTimeScale;

			if (time <= 0f)
				VirtualEnd();

			if (targetNpc && Vector3.Distance(targetNpc.transform.position, transform.position) < nearbyNpcDistance)
			{
				hasDetectedTarget = false;
				momentumNav.ClearDestination();
				targetNpc = null;
			}
		}

		protected override void VirtualEnd()
		{
			hasDetectedTarget = false;
			targetNpc = null;
			base.VirtualEnd();
		}
	}
}