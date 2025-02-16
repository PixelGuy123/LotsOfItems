using LotsOfItems.Components;
using LotsOfItems.ItemPrefabStructures;
using MTM101BaldAPI;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.BSODAs;
public class ITM_OSODA : ITM_GenericBSODA
{
	[SerializeField] 
	private float puddleLifeTime = 30f;

	[SerializeField] 
	private SodaPuddle puddlePrefab;

	protected override void VirtualSetupPrefab(ItemObject itm)
	{
		base.VirtualSetupPrefab(itm);
		time = 10f;
		speed *= 1.15f;
		moveMod.movementMultiplier = 0.1f;
		AddendMultiplier = 0.75f;

		spriteRenderer.sprite = this.GetSprite("OSODA_Spray.png", spriteRenderer.sprite.pixelsPerUnit);
		Destroy(GetComponentInChildren<ParticleSystem>().gameObject);
		entity.collisionLayerMask = LayerStorage.gumCollisionMask;

		var puddleObject = ObjectCreationExtensions.CreateSpriteBillboard(
				spriteRenderer.sprite, // temp sprite
				false
			).AddSpriteHolder(out var puddleSprite, 0.05f, LayerStorage.ignoreRaycast);
		puddleSprite.name = "Sprite";
		puddleSprite.gameObject.layer = 0;
		puddleSprite.transform.Rotate(90, 0, 0); // Face downward
		puddlePrefab = puddleObject.gameObject.AddComponent<SodaPuddle>();
		puddlePrefab.gameObject.ConvertToPrefab(true);
		puddlePrefab.name = "SodaPuddle";
		puddlePrefab.speedDebuff.movementMultiplier = moveMod.movementMultiplier;

		var collider = puddlePrefab.gameObject.AddComponent<BoxCollider>();
		collider.isTrigger = true;
		collider.size = new Vector3(4.5f, 1f, 4.5f);
		collider.center = Vector3.up * 2f;
	}

	public override bool Use(PlayerManager pm)
	{
		entity.OnEntityMoveInitialCollision += (hit) =>
		{
			if (!hasEnded && hit.transform.CompareTag("Wall"))
				VirtualEnd();
		};
		return base.Use(pm);
	}

	protected override void VirtualEnd()
	{
		// Check if the current cell is inside the school (placeholder check)
		if (ec.ContainsCoordinates(transform.position) && !ec.CellFromPosition(transform.position).Null)
		{
			Instantiate(puddlePrefab, ec.CellFromPosition(transform.position).FloorWorldPosition, Quaternion.identity)
				.Initialize(puddleLifeTime, ec);
		}
		base.VirtualEnd();
	}
}
