using LotsOfItems.ItemPrefabStructures;
using MTM101BaldAPI.Components;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.BSODAs;
public class ITM_Plunger : ITM_GenericBSODA
{
	[SerializeField]
	internal AudioManager audMan;

	[SerializeField]
	internal SoundObject audHit;

	[SerializeField]
	internal float hitForce = 5f, hitAcceleration = -3f;

	[SerializeField]
	internal MovementModifier slowMod = new(Vector3.zero, 0.65f);

	private bool isStuck = false;
	private bool stuckToEntity = false;
	private bool stuckToWall = false;
	private Entity stuckTarget = null;
	private Vector3 stuckPosition;
	readonly ValueModifier blindMod = new(0f);

	protected override void VirtualSetupPrefab(ItemObject itm)
	{
		base.VirtualSetupPrefab(itm);

		speed *= 3f;
		time = 20f;

		var sprs = this.GetSpriteSheet("Plunger_Sheet.png", 4, 2, spriteRenderer.sprite.pixelsPerUnit + 35f);
		spriteRenderer.sprite = sprs[0];
		spriteRenderer.CreateAnimatedSpriteRotator(GenericExtensions.CreateRotationMap(8, sprs));
		Destroy(GetComponentInChildren<ParticleSystem>().gameObject);

		audMan = gameObject.CreatePropagatedAudioManager(65f, 75f);
		audHit = this.GetSound("plunger_hit.wav", "LtsOItems_Vfx_Suction", SoundType.Effect, Color.white);
		sound = this.GetSoundNoSub("Plunger_Throw.wav", SoundType.Effect);

		entity.collisionLayerMask = LayerStorage.gumCollisionMask;
	}

	public override bool Use(PlayerManager pm)
	{
		entity.OnEntityMoveInitialCollision += (hit) =>
		{
			if (!isStuck && hit.transform.CompareTag("Wall"))
			{
				isStuck = true;
				stuckToWall = true;
				stuckPosition = transform.position;
				audMan.PlaySingle(audHit);
			}

		};
		return base.Use(pm);
	}

	public override bool VirtualEntityTriggerEnter(Collider other)
	{
		Entity target = other.GetComponent<Entity>();
		if (target != null && !isStuck)
		{
			isStuck = true;
			stuckToEntity = true;
			stuckTarget = target;
			// Apply a small force against the target.
			target.AddForce(new(transform.forward, hitForce, hitAcceleration));
			target.ExternalActivity.moveMods.Add(slowMod);
			target.GetComponent<NPC>()?.GetNPCContainer().AddLookerMod(blindMod);
		}

		return false;
	}

	public override void VirtualUpdate()
	{
		if (!isStuck)
		{
			base.VirtualUpdate();
			return;
		}

		if (stuckToEntity && stuckTarget != null)
		{
			transform.position = stuckTarget.transform.position;
		}
		else if (stuckToWall)
		{
			transform.position = stuckPosition;
		}

		time -= Time.deltaTime * ec.EnvironmentTimeScale;

		if (time <= 0f)
		{
			VirtualEnd();
		}

		entity.UpdateInternalMovement(Vector3.zero);
	}

	protected override void VirtualEnd()
	{
		base.VirtualEnd();
		if (stuckToEntity && stuckTarget)
		{
			stuckTarget.ExternalActivity.moveMods.Remove(slowMod);
			stuckTarget.GetComponent<NPC>()?.GetNPCContainer().RemoveLookerMod(blindMod);
		}
	}
}