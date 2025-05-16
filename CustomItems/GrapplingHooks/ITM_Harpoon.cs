using LotsOfItems.ItemPrefabStructures;
using UnityEngine;

namespace LotsOfItems.CustomItems.GrapplingHooks
{
	public class ITM_Harpoon : ITM_GenericGrapplingHook
	{
		bool touchedNPCs = false;
		NPC npc = null;

		protected override void VirtualSetupPrefab(ItemObject itm)
		{
			base.VirtualSetupPrefab(itm);
			var hookRenderer = GetComponentInChildren<SpriteRenderer>();
			hookRenderer.sprite = this.GetSprite("Harpoon_world.png", hookRenderer.sprite.pixelsPerUnit);
			uses = 0;
		}

		public override bool VirtualPreUpdate()
		{
			if (touchedNPCs && locked)
			{
				if (!npc && !snapped)
				{
					ForceSnap();
					return false;
				}

				entity.UpdateInternalMovement(Vector3.zero);
				if ((transform.position - npc.transform.position).magnitude <= stopDistance)
				{
					StartCoroutine(EndDelay());
				}
				moveMod.movementAddend = (transform.position - npc.transform.position).normalized * force;
				if (!snapped)
				{
					motorAudio.pitch = (force - initialForce) / 100f + 1f;
				}
				force += forceIncrease * Time.deltaTime;
				pressure = (transform.position - npc.transform.position).magnitude - (initialDistance - force);
				if (pressure > maxPressure && !snapped)
					ForceSnap();

				return false;
			}
			return true;
		}

		public override bool VirtualPreLateUpdate()
		{
			if (touchedNPCs && npc)
			{
				positions[0] = transform.position;
				positions[1] = npc.transform.position + Vector3.down * 1f;
				lineRenderer.SetPositions(positions);
				return false;
			}
			return true;
		}

		public override void VirtualEnd()
		{
			base.VirtualEnd();
			UntouchNPC();
		}

		protected override void OnDespawn() =>
			UntouchNPC();


		void UntouchNPC()
		{
			if (touchedNPCs)
				npc?.Navigator.Am.moveMods.Remove(moveMod);
		}

		public override void EntityTriggerEnter(Collider other)
		{
			if (!touchedNPCs && !locked && other.isTrigger && other.CompareTag("NPC"))
			{
				// Try get component exists?? WOW
				if (other.TryGetComponent(out NPC npc) && npc.Navigator.isActiveAndEnabled)
				{
					touchedNPCs = true;
					npc.Navigator.Am.moveMods.Add(moveMod);
					pm.Am.moveMods.Remove(moveMod);
					this.npc = npc;
				}
			}
		}
	}
}