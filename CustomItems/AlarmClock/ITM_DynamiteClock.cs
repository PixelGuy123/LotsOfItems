using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using UnityEngine;
using PixelInternalAPI.Classes;

namespace LotsOfItems.CustomItems.AlarmClock;
public class ITM_DynamiteClock : ITM_GenericAlarmClock
{
	[SerializeField] 
	private Sprite explosionSprite; // sprite to display when exploding

	[SerializeField] 
	private SoundObject audExplode; // sound to play on explosion

	[SerializeField]
	internal float explosionDistance = 45f, explosionForce = 86f, explosionAcceleration = -43.5f;

	[SerializeField]
	internal LayerMask collisionLayer = LotOfItemsPlugin.onlyNpcPlayerLayers;

	protected override void VirtualSetupPrefab(ItemObject itm)
	{
		base.VirtualSetupPrefab(itm);
		var sprs = this.GetSpriteSheet("DynamiteClock_world.png", 2, 1, spriteRenderer.sprite.pixelsPerUnit);
		spriteRenderer.sprite = sprs[0];
		explosionSprite = sprs[1];
		audExplode = LotOfItemsPlugin.assetMan.Get<SoundObject>("aud_explode");
	}

	public override bool AllowClickable() => false;

	protected override void OnClockRing()
	{
		// Change to explosion sprite and play explosion sound when the timer ends
		if (explosionSprite != null)
			spriteRenderer.sprite = explosionSprite;
		if (audExplode != null)
			audMan.PlaySingle(audExplode);


		Collider[] colliders = Physics.OverlapSphere(transform.position, explosionDistance, collisionLayer, QueryTriggerInteraction.Ignore);

		Ray ray = new();
		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders[i].transform == transform)
				continue;

			Entity entity = colliders[i].GetComponent<Entity>();
			if (entity == null || !entity.InBounds)
				continue;

			Vector3 entityDirection = (entity.transform.position - transform.position).normalized;
			ray.origin = transform.position;
			ray.direction = entityDirection;

			if (Physics.Raycast(ray, out var hit, 9999f, LayerStorage.principalLookerMask) && hit.transform == colliders[i].transform)
				entity.AddForce(new Force(entityDirection, explosionForce, explosionAcceleration));
		}
	}
}
