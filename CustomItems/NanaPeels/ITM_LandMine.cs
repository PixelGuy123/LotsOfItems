using LotsOfItems.CustomItems;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
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
	internal float explosionDistance = 45f, explosionForce = 50f, explosionAcceleration = -43.5f, bullyDelay = 5f;
	[SerializeField]
	internal SpriteRenderer renderer;
	[SerializeField]
	internal LayerMask collisionLayer = LotOfItemsPlugin.onlyNpcPlayerLayers;

	protected override void VirtualSetupPrefab(ItemObject itm)
	{
		base.VirtualSetupPrefab(itm);
		renderer = GetComponentInChildren<SpriteRenderer>();
		var sprs = this.GetSpriteSheet("LandMine_world.png", 2, 1, renderer.sprite.pixelsPerUnit + 15f);
		renderer.sprite = sprs[0];
		explosionSprite = sprs[1];

		activationSound = this.GetSound("landMine_Activate.wav", "LtsOItems_Vfx_Activated", SoundType.Effect, Color.white);
		explosionSound = this.GetSound("landMine_explosion.wav", "LtsOItems_Vfx_Explode", SoundType.Effect, Color.white);

		endHeight = 1.35f;
		throwSpeed *= 1.75f;
	}

	bool canBeBully = true, hasExploded = false, activated = false;

	public override bool Use(PlayerManager pm)
	{
		this.pm = pm;
		return base.Use(pm);
	}

	// Called when the mine hits the floor
	internal override void OnFloorHit()
	{
		if (!activated)
		{
			StartCoroutine(CanBeBully());
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
		if (canBeBully)
			pm?.RuleBreak("Bullying", 1f);
		hasExploded = true;
		audioManager.PlaySingle(explosionSound);


		renderer.sprite = explosionSprite;
		entity.SetFrozen(true);


		Extensions.Explode(this, explosionDistance, collisionLayer, explosionForce, explosionAcceleration);

		while (audioManager.AnyAudioIsPlaying)
			yield return null;

		Destroy(gameObject);
	}

	IEnumerator CanBeBully()
	{
		float timer = bullyDelay;
		while (timer > 0f)
		{
			timer -= pm.ec.EnvironmentTimeScale * Time.deltaTime;
			yield return null;
		}
		canBeBully = false;
	}
}
