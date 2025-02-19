using LotsOfItems.ItemPrefabStructures;
using System.Collections.Generic;
using UnityEngine;
using LotsOfItems.Plugin;

namespace LotsOfItems.CustomItems.BSODAs;
public class ITM_ShrinkRay : ITM_GenericBSODA
{
	[SerializeField]
	private float squishDuration = 20f, pushForce = 65f, pushAcceleration = -35f;

	readonly HashSet<NPC> alreadyTouchedNPCs = [];

	protected override void VirtualSetupPrefab(ItemObject itm)
	{
		base.VirtualSetupPrefab(itm);

		sound = this.GetSoundNoSub("ShrinkRay_shoot.wav", SoundType.Effect);
		speed *= 1.5f;
		spriteRenderer.sprite = this.GetSprite("ShrinkRay_Soda.png", spriteRenderer.sprite.pixelsPerUnit);
		this.DestroyParticleIfItHasOne();
	}

	public override bool VirtualEntityTriggerEnter(Collider other)
	{
		if (other.isTrigger && other.CompareTag("NPC"))
		{
			NPC npc = other.GetComponent<NPC>();
			if (npc != null && !alreadyTouchedNPCs.Contains(npc) && npc.Navigator.isActiveAndEnabled && !npc.Navigator.Entity.Squished)
			{
				alreadyTouchedNPCs.Add(npc);
				npc.Navigator.Entity.Squish(squishDuration);
				npc.Navigator.Entity.AddForce(new(transform.forward, pushForce, pushAcceleration));
				return false; // Skip default push behavior
			}
		}
		return true;
	}
}
