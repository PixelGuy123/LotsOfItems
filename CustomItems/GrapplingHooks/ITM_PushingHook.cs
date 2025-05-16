using System.Collections.Generic;
using LotsOfItems.ItemPrefabStructures;
using UnityEngine;

namespace LotsOfItems.CustomItems.GrapplingHooks
{
	public class ITM_PushingHook : ITM_GenericGrapplingHook
	{
		[SerializeField]
		internal MovementModifier pushModifier = new(Vector3.zero, 0f, 1);

		readonly List<NPC> pushedNpcs = [];

		protected override void VirtualSetupPrefab(ItemObject itm)
		{
			base.VirtualSetupPrefab(itm);
			var hookRenderer = GetComponentInChildren<SpriteRenderer>();
			hookRenderer.sprite = this.GetSprite("PushingHook_world.png", hookRenderer.sprite.pixelsPerUnit);
			uses = 0;
		}

		public override bool OnWallHitOverride(RaycastHit hit)
		{
			if (layerMask.Contains(hit.collider.gameObject.layer) && !locked)
			{
				ForceStop();
				ForceSnap();
				for (int i = 0; i < pushedNpcs.Count; i++)
				{
					pushedNpcs[i]?.Navigator.Am.moveMods.Remove(pushModifier);
					pushedNpcs.RemoveAt(i--);
				}
			}
			return false;
		}

		public override void VirtualEnd()
		{
			base.VirtualEnd();
			UngrabNPCs();
		}

		protected override void OnDespawn() =>
			UngrabNPCs();


		void UngrabNPCs()
		{
			for (int i = 0; i < pushedNpcs.Count; i++)
			{
				pushedNpcs[i]?.Navigator.Am.moveMods.Remove(pushModifier);
			}
			pushedNpcs.Clear();
		}

		public override void VirtualUpdate()
		{
			for (int i = 0; i < pushedNpcs.Count; i++)
			{
				if (!pushedNpcs[i] || Vector3.Distance(transform.position, pushedNpcs[i].transform.position) > stopDistance)
				{
					pushedNpcs.RemoveAt(i--);
					continue;
				}
				pushedNpcs[i].Navigator.Entity.Teleport(transform.position);
			}
		}

		public override void EntityTriggerEnter(Collider other)
		{
			if (!locked && other.isTrigger && other.CompareTag("NPC"))
			{
				// Try get component exists?? WOW
				if (other.TryGetComponent(out NPC npc) && npc.Navigator.isActiveAndEnabled && !pushedNpcs.Contains(npc))
				{
					npc.Navigator.Am.moveMods.Add(pushModifier);
					pushedNpcs.Add(npc);
					npc.Navigator.Entity.Teleport(transform.position);
				}
			}
		}
	}
}