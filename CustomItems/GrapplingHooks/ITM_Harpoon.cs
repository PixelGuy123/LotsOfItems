using LotsOfItems.ItemPrefabStructures;
using System.Collections.Generic;
using UnityEngine;

namespace LotsOfItems.CustomItems.GrapplingHooks
{
	public class ITM_Harpoon : ITM_GenericGrapplingHook
	{
		[SerializeField]
		internal MovementModifier pushModifier = new(Vector3.zero, 0f, 1);

		readonly private List<KeyValuePair<NPC, float>> pushedNpcs = [];

		bool touchedNPCs = false;
		float limitDistance = -1f;

		public override bool OnWallHitOverride(RaycastHit hit)
		{
			if (!touchedNPCs)
				return true;

			if (layerMask.Contains(hit.collider.gameObject.layer) && !locked)
			{
				ForceStop(-hit.normal, false);
				limitDistance = Vector3.Distance(transform.position, pm.transform.position) * 1.75f;
			}
			return false;
		}

		protected override void VirtualSetupPrefab(ItemObject itm)
		{
			base.VirtualSetupPrefab(itm);
			var hookRenderer = GetComponentInChildren<SpriteRenderer>();
			hookRenderer.sprite = this.GetSprite("Harpoon_world.png", hookRenderer.sprite.pixelsPerUnit);
			uses = 0;
		}

		public override bool VirtualPreUpdate() =>
				!locked || !touchedNPCs;
		

		public override void VirtualUpdate()
		{
			base.VirtualUpdate();
			pushModifier.movementAddend = (pm.transform.position - transform.position).normalized * speed * ec.EnvironmentTimeScale;
			if (touchedNPCs && !snapped)
			{
				float distance;
				for (int i = 0; i < pushedNpcs.Count; i++)
				{
					distance = Vector3.Distance(pushedNpcs[i].Key.transform.position, pm.transform.position);
					if (distance >= pushedNpcs[i].Value || distance <= stopDistance)
					{
						pushedNpcs[i].Key?.Navigator.Entity.ExternalActivity.moveMods.Remove(pushModifier);
						pushedNpcs.RemoveAt(i--);
					}
				}
				distance = (transform.position - pm.transform.position).magnitude;
				bool outOfLimit = limitDistance != -1f && (distance > limitDistance || pushedNpcs.Count == 0);
				if (distance <= stopDistance || outOfLimit)
				{
					if (outOfLimit)
						ForceSnap();
					else
						End();
				}
			}
		}

		public override void VirtualEnd()
		{
			base.VirtualEnd();
			foreach (var npc in pushedNpcs)
				npc.Key?.Navigator.Entity.ExternalActivity.moveMods.Remove(pushModifier);
		}

		public override void EntityTriggerEnter(Collider other)
		{
			if (!locked && other.isTrigger && other.CompareTag("NPC"))
			{
				// Try get component exists?? WOW
				if (other.TryGetComponent(out NPC npc) && npc.Navigator.isActiveAndEnabled && !pushedNpcs.Exists(x => x.Key == npc))
				{
					if (!touchedNPCs)
						touchedNPCs = true;
					
					npc.Navigator.Entity.ExternalActivity.moveMods.Add(pushModifier);
					pushedNpcs.Add(new(npc, Vector3.Distance(transform.position, pm.transform.position) * 1.15f));
				}
			}
		}
	}
}