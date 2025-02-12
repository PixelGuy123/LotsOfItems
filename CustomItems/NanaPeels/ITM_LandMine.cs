using LotsOfItems.CustomItems;
using LotsOfItems.ItemPrefabStructures;
using PixelInternalAPI.Classes;
using System.Collections;
using UnityEngine;

public class ITM_LandMine : ITM_GenericNanaPeel
{
	[SerializeField]
	internal SoundObject activationSound, explosionSound;
	[SerializeField]
	internal Sprite explosionSprite;
	[SerializeField]
	internal float explosionDistance = 5f, explosionForce = 50f, explosionAcceleration = -43.5f;
	[SerializeField]
	internal SpriteRenderer renderer;

	protected override void VirtualSetupPrefab(ItemObject itm)
	{
		base.VirtualSetupPrefab(itm);
		renderer = GetComponentInChildren<SpriteRenderer>();
		var sprs = this.GetSpriteSheet("LandMine_world.png", 2, 1, renderer.sprite.pixelsPerUnit);
		renderer.sprite = sprs[0];
		explosionSprite = sprs[1];

		activationSound = this.GetSound("landMine_Activate.wav", "LtsOItems_Vfx_Activated", SoundType.Effect, Color.white);
		explosionSound = this.GetSound("landMine_explosion.wav", "LtsOItems_Vfx_Explode", SoundType.Effect, Color.white);
	}

	private bool hasExploded = false;
	private bool activated = false;
	Ray ray = new();
	RaycastHit hit;

	// Called when the mine hits the floor
	internal override void OnFloorHit()
	{
		if (!activated)
		{
			audioManager.PlaySingle(activationSound);
			activated = true;
		}
	}

	// Trigger explosion when an entity collides
	internal override bool EntityTriggerStayOverride(Collider other)
	{
		if (!hasExploded && other.isTrigger)
		{
			var entity = other.GetComponent<Entity>();
			if (entity)
				StartCoroutine(Explode());
		}
		return false;
	}

	private IEnumerator Explode()
	{
		hasExploded = true;
		audioManager.PlaySingle(explosionSound);

		// Create explosion sprite indicator
		renderer.sprite = explosionSprite;
		entity.SetFrozen(true);

		// Push away entities within explosionDistance using provided snippet
		Collider[] colliders = Physics.OverlapSphere(transform.position, explosionDistance, LayerStorage.entityCollisionMask, QueryTriggerInteraction.Ignore);
		for (int i = 0; i < colliders.Length; i++)
		{
			Entity entity = colliders[i].GetComponent<Entity>();
			if (entity == null || !entity.InBounds)
				continue;

			Vector3 entityDirection = (entity.transform.position - transform.position).normalized;
			ray.origin = transform.position;
			ray.direction = entityDirection;

			if (Physics.Raycast(ray, out hit, explosionForce, LayerStorage.principalLookerMask, QueryTriggerInteraction.Ignore) && hit.transform == entity.transform)
				entity.AddForce(new Force(entityDirection, explosionForce, explosionAcceleration));
		}

		while (audioManager.AnyAudioIsPlaying)
			yield return null;

		Destroy(gameObject);
	}
}
