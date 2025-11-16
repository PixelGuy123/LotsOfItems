using System.Collections;
using System.Collections.Generic;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using MTM101BaldAPI;
using UnityEngine;

namespace LotsOfItems.CustomItems.BSODAs;

public class ITM_LemonSoda : ITM_GenericBSODA
{
	readonly HashSet<Entity> touchedEntities = [];

	[SerializeField]
	MovementModifier stunMod = new(Vector3.zero, 0.2f);

	[SerializeField]
	internal float stunTimer = 10f;

	protected override void VirtualSetupPrefab(ItemObject itm)
	{
		base.VirtualSetupPrefab(itm);
		time = 1f;
		speed *= 4f;

		spriteRenderer.sprite = this.GetSprite("LemonSoda_soda.png", spriteRenderer.sprite.pixelsPerUnit);
		this.DestroyParticleIfItHasOne();
	}
	public override bool VirtualEntityTriggerEnter(Collider other, bool validCollision)
	{
		if (validCollision && other.isTrigger && !hasEnded)
		{
			Entity target = other.GetComponent<Entity>();
			if (target != null && !touchedEntities.Contains(target))
			{
				target.ExternalActivity.moveMods.Add(stunMod);
				StartCoroutine(RemoveStunAfterDelay(target));
				touchedEntities.Add(target);
			}
		}
		return true;
	}

	private IEnumerator RemoveStunAfterDelay(Entity target)
	{
		yield return new WaitForSecondsEnvironmentTimescale(ec, stunTimer);
		if (target)
		{
			target.ExternalActivity.moveMods.Remove(stunMod);
			touchedEntities.Remove(target);
		}
		else
			touchedEntities.RemoveWhere(x => !x); // If null, remove from the hashset too

	}

	protected override void VirtualEnd()
	{
		hasEnded = true;
		foreach (ActivityModifier activityMod in activityMods)
			activityMod.moveMods.Remove(moveMod);

		StartCoroutine(WaitToDie());
	}

	IEnumerator WaitToDie()
	{
		while (touchedEntities.Count != 0)
			yield return null;

		Destroy(gameObject);
	}
}
