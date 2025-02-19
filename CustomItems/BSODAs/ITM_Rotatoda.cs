using LotsOfItems.ItemPrefabStructures;
using PixelInternalAPI.Classes;
using UnityEngine;
using LotsOfItems.Plugin;

namespace LotsOfItems.CustomItems.BSODAs;
public class ITM_Rotatoda : ITM_GenericBSODA
{
	[SerializeField]
	private float minTimeBetweenRotations = 1f, maxTimeBetweenRotations = 3f;

	private float rotationTimer;

	protected override void VirtualSetupPrefab(ItemObject itm)
	{
		base.VirtualSetupPrefab(itm);
		entity.collisionLayerMask = LayerStorage.gumCollisionMask;

		spriteRenderer.sprite = this.GetSprite("Rotatoda_Soda.png", spriteRenderer.sprite.pixelsPerUnit);
		this.DestroyParticleIfItHasOne();

		time = 54f;
	}

	public override bool Use(PlayerManager pm)
	{
		var val = base.Use(pm);
		rotationTimer = Random.Range(minTimeBetweenRotations, maxTimeBetweenRotations);
		entity.OnEntityMoveInitialCollision += (hit) =>
		{
			if (hit.transform.CompareTag("Wall"))
				rotationTimer = 0f; // Change direction quicker
		};
		return val;
	}

	public override void VirtualUpdate()
	{
		rotationTimer -= Time.deltaTime * ec.EnvironmentTimeScale;

		if (rotationTimer <= 0f)
		{
			Vector2 randomCircle = Random.insideUnitCircle;
			transform.forward = new Vector3(randomCircle.x, 0f, randomCircle.y).normalized;
			rotationTimer = Random.Range(minTimeBetweenRotations, maxTimeBetweenRotations);
		}

		base.VirtualUpdate();
	}
}