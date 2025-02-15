using LotsOfItems.ItemPrefabStructures;
using System.Collections.Generic;
using UnityEngine;

namespace LotsOfItems.CustomItems.GrapplingHooks
{
	public class ITM_Harpoon : ITM_GenericGrapplingHook
	{
		[SerializeField]
		internal MovementModifier pushModifier = new(Vector3.zero, 0f, 1);

		readonly private List<NPC> pushedNpcs = [];

		bool touchedNPCs = false;

		protected override void VirtualSetupPrefab(ItemObject itm)
		{
			base.VirtualSetupPrefab(itm);
			var hookRenderer = GetComponentInChildren<SpriteRenderer>();
			hookRenderer.sprite = this.GetSprite("Harpoon_world.png", hookRenderer.sprite.pixelsPerUnit);
			uses = 0;
		}

		public override void VirtualUpdate()
		{
			base.VirtualUpdate();
			pushModifier.movementAddend = transform.forward * speed * ec.EnvironmentTimeScale;
			if (touchedNPCs)
			{
				if ((transform.position - pm.transform.position).magnitude <= stopDistance)
					End();
			}
		}

		public override void VirtualEnd()
		{
			base.VirtualEnd();
			foreach (var npc in pushedNpcs)
				npc?.Navigator.Entity.ExternalActivity.moveMods.Remove(pushModifier);
		}

		public override void EntityTriggerEnter(Collider other)
		{
			if (!locked && other.isTrigger && other.CompareTag("NPC"))
			{
				// Try get component exists?? WOW
				if (other.TryGetComponent(out NPC npc) && npc.Navigator.isActiveAndEnabled && !pushedNpcs.Contains(npc))
				{
					if (!touchedNPCs)
					{
						speed = -speed;
						touchedNPCs = true;
					}
					npc.Navigator.Entity.ExternalActivity.moveMods.Add(pushModifier);
					pushedNpcs.Add(npc);
				}
			}
		}

		public override void EntityTriggerExit(Collider other)
		{
			if (other.CompareTag("NPC") && other.TryGetComponent(out NPC npc) && pushedNpcs.Contains(npc))
			{
				npc.Navigator.Entity.ExternalActivity.moveMods.Remove(pushModifier);
				pushedNpcs.Remove(npc);
			}
		}
	}
}