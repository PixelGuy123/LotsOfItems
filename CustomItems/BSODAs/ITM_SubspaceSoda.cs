using System.Collections;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using MTM101BaldAPI;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.BSODAs;

public class ITM_SubspaceSoda : ITM_GenericBSODA, IBsodaShooter
{
	[SerializeField]
	private float activationDelay = 5f;

	[SerializeField]
	private float minSpeed = 35f, maxSpeed = 100f;

	[SerializeField]
	[Range(0f, 1f)]
	private float chanceToFailExplosion = 0.15f, chanceToFailExplosionSteppedOn = 0.75f;

	[SerializeField]
	private SoundObject audFail, audExplode, audActivate;

	[SerializeField]
	private AudioManager audMan;

	[SerializeField]
	private SpriteRenderer canRenderer;

	[SerializeField]
	private Sprite tooWeakSprite;

	private bool isArmed = false, triggered = false;

	public Quaternion PanicKernelRotationOffset { get; set; } = Quaternion.identity;

	Vector3 dirToBlow;
	Coroutine detonationCor;

	protected override void VirtualSetupPrefab(ItemObject itm)
	{
		base.VirtualSetupPrefab(itm);
		audMan = gameObject.CreateAudioManager(75f, 100f);

		spriteRenderer.enabled = false;

		canRenderer = ObjectCreationExtensions.CreateSpriteBillboard(itm.itemSpriteLarge);
		canRenderer.name = "CanRenderer";
		canRenderer.transform.SetParent(transform);
		canRenderer.transform.localPosition = Vector3.down * 4.3f;
		tooWeakSprite = this.GetSprite("SubspaceSoda_Dead.png", itm.itemSpriteLarge.pixelsPerUnit);

		// Load the audios; download them too lol
		spriteRenderer.sprite = this.GetSprite("Rotatoda_Soda.png", spriteRenderer.sprite.pixelsPerUnit); // It's pink, so...
		this.DestroyParticleIfItHasOne();
		audFail = this.GetSound("Subspace_Bruh.wav", "LtsOItems_Vfx_Womp", SoundType.Effect, Color.white);
		audExplode = this.GetSound("Subspace_Explosion.wav", "LtsOItems_Vfx_Explode", SoundType.Effect, Color.white);
		audActivate = this.GetSound("Subspace_Activation.wav", "LtsOItems_Vfx_Activated", SoundType.Effect, Color.white);
	}

	public override bool Use(PlayerManager pm)
	{
		// Store initial direction and place on floor
		ec = pm.ec;
		this.pm = pm;
		dirToBlow = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.rotation * PanicKernelRotationOffset * Vector3.forward;
		transform.position = pm.transform.position;
		entity.Initialize(ec, transform.position);
		audMan.PlaySingle(audActivate);

		detonationCor = StartCoroutine(DetonationCountdown());

		return true;
	}

	private IEnumerator DetonationCountdown()
	{
		yield return new WaitForSecondsEnvironmentTimescale(ec, activationDelay);

		TryExplode(false);
	}

	private IEnumerator FailExplosion()
	{
		audMan.PlaySingle(audFail);
		canRenderer.sprite = tooWeakSprite;
		while (audMan.AnyAudioIsPlaying)
			yield return null;
		Destroy(gameObject);
		yield break;
	}

	void TryExplode(bool steppedOn)
	{
		if (triggered)
			return;

		audMan.FlushQueue(true);
		triggered = true;
		if (Random.value <= (steppedOn ? chanceToFailExplosionSteppedOn : chanceToFailExplosion)) // 15% failure chance
		{
			StartCoroutine(FailExplosion());
			return;
		}
		// Randomize speed and activate BSODA behavior
		speed = Random.Range(minSpeed, maxSpeed);


		spriteRenderer.SetSpriteRotation(Random.Range(0f, 360f));
		spriteRenderer.enabled = true;
		canRenderer.enabled = false;

		moveMod.priority = 1;
		transform.forward = dirToBlow;
		isArmed = true;

		audMan.PlaySingle(audExplode);
	}

	public override bool VirtualEntityTriggerEnter(Collider other, bool validCollision)
	{
		if (isArmed || !validCollision)
			return true;
		if (!triggered && other.isTrigger && (other.CompareTag("Player") || other.CompareTag("NPC")))
		{
			if (detonationCor != null)
				StopCoroutine(detonationCor);
			TryExplode(true);
			return isArmed;
		}
		return false;
	}

	public override void VirtualUpdate()
	{
		if (!isArmed) return;
		base.VirtualUpdate();
	}
}