using LotsOfItems.ItemPrefabStructures;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.BSODAs;

public class ITM_FrogInACan : ITM_GenericBSODA
{
	private bool isStuck = false;
	private bool isDespawning = false;
	private bool despawnLaunched = false;
	private float shakeTime = 25f;
	private readonly float launchOffSpeed = 5f;
	private readonly float shakeForce = 2f;
	private readonly float despawnYLevel = -5f;
	private readonly float chanceForRibbiting = 0.25f;
	[SerializeField]
	internal SoundObject audLaunchOff, audRibbit;

	[SerializeField]
	internal AudioManager audMan;

	private Entity stuckTarget = null;

	[SerializeField]
	private readonly MovementModifier shakeMod = new(Vector3.zero, 0.15f);

	protected override void VirtualSetupPrefab(ItemObject itm)
	{
		base.VirtualSetupPrefab(itm);
		audMan = gameObject.CreatePropagatedAudioManager(100f, 125f);
		audLaunchOff = GenericExtensions.FindResourceObjectByName<SoundObject>("Nana_Slip");
		audRibbit = this.GetSound("frogInCan_croak.wav", "LtsOItems_Vfx_Ribbit", SoundType.Effect, Color.white);
		sound = audRibbit;

		spriteRenderer.sprite = this.GetSprite("FroginaCan_frog.png", spriteRenderer.sprite.pixelsPerUnit);
		Destroy(GetComponentInChildren<ParticleSystem>().gameObject);
	}


	public override bool VirtualEntityTriggerEnter(Collider other)
	{
		Entity target = other.GetComponent<Entity>();
		if (target != null && !isStuck)
		{
			isStuck = true;
			stuckTarget = target;
			target.ExternalActivity.moveMods.Add(shakeMod);
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
		else if (!isDespawning)
		{

			shakeTime -= Time.deltaTime * ec.EnvironmentTimeScale;
			if (Time.timeScale > 0 && ec.EnvironmentTimeScale > 0)
			{
				shakeMod.movementAddend = Random.insideUnitSphere * shakeForce;
				if (Random.value <= chanceForRibbiting)
					audMan.PlaySingle(audRibbit);
			}

			if (!stuckTarget)
			{
				isDespawning = true;
				return;
			}


			transform.position = stuckTarget.transform.position;


			if (shakeTime <= 0f)
			{
				stuckTarget.ExternalActivity.moveMods.Remove(shakeMod);
				isDespawning = true;
			}
		}
		else
		{
			// Despawn sequence: launch the sprite renderer with gravity and a sound effect.
			if (!despawnLaunched)
			{

				spriteRenderer.transform.parent = null; // Sets to null, so it can be thrown off lol

				Rigidbody rb = spriteRenderer.gameObject.GetComponent<Rigidbody>() ?? spriteRenderer.gameObject.AddComponent<Rigidbody>();
				rb.AddForce(Random.insideUnitSphere * launchOffSpeed, ForceMode.VelocityChange);

				audMan.FlushQueue(true);
				audMan.PlaySingle(audLaunchOff);

				despawnLaunched = true;
			}

			// When the launched sprite falls below y = -10, destroy the entire object.
			if (spriteRenderer.transform.position.y < -despawnYLevel)
			{
				Destroy(spriteRenderer.gameObject);
				Destroy(gameObject);
			}
		}

		entity.UpdateInternalMovement(Vector3.zero);
	}

	protected override void VirtualEnd()
	{
		if (!isStuck)
			base.VirtualEnd();
		hasEnded = true;
	}
}
