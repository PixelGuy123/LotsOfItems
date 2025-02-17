using UnityEngine;
using System.Collections;
using PixelInternalAPI.Extensions;
using MTM101BaldAPI;
using LotsOfItems.ItemPrefabStructures;

namespace LotsOfItems.CustomItems.BSODAs;

public class ITM_SubspaceSoda : ITM_GenericBSODA
{
	[SerializeField]
	private float activationDelay = 5f;

	[SerializeField]
	private float minSpeed = 35f, maxSpeed = 100f;

	[SerializeField]
	[Range(0f, 1f)]
	private float chanceToFailExplosion = 0.15f;

	[SerializeField] 
	private SoundObject audFail, audExplode, audActivate;

	[SerializeField]
	private AudioManager audMan;

	[SerializeField]
	private SpriteRenderer canRenderer;

	[SerializeField]
	private Sprite tooWeakSprite;

	private bool isArmed = false, triggered = false;
	Vector3 dirToBlow;
	Coroutine detonationCor;

	protected override void VirtualSetupPrefab(ItemObject itm)
	{
		base.VirtualSetupPrefab(itm);
		audMan = gameObject.CreatePropagatedAudioManager(75f, 100f);

		spriteRenderer.enabled = false;

		canRenderer = ObjectCreationExtensions.CreateSpriteBillboard(itm.itemSpriteLarge);
		canRenderer.name = "CanRenderer";
		canRenderer.transform.SetParent(transform);
		canRenderer.transform.localPosition = Vector3.down * 4.3f;
		tooWeakSprite = this.GetSprite("SubspaceSoda_Dead.png", itm.itemSpriteLarge.pixelsPerUnit);

		// Load the audios; download them too lol
		Destroy(GetComponentInChildren<ParticleSystem>().gameObject);
		audFail = this.GetSound("Subspace_Bruh.wav", "LtsOItems_Vfx_Womp", SoundType.Effect, Color.white);
		audExplode = this.GetSound("Subspace_Explosion.wav", "LtsOItems_Vfx_Explode", SoundType.Effect, Color.white);
		audActivate = this.GetSound("Subspace_Activation.wav", "LtsOItems_Vfx_Activated", SoundType.Effect, Color.white);
	}

	public override bool Use(PlayerManager pm)
	{
		// Store initial direction and place on floor
		ec = pm.ec;
		this.pm = pm;
		dirToBlow = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward;
		transform.position = pm.transform.position;
		entity.Initialize(ec, transform.position);
		audMan.PlaySingle(audActivate);

		detonationCor = StartCoroutine(DetonationCountdown());

		return true;
	}

	private IEnumerator DetonationCountdown()
	{
		yield return new WaitForSecondsEnvironmentTimescale(ec, activationDelay);

		TryExplode();
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

	void TryExplode()
	{
		if (triggered)
			return;

		audMan.FlushQueue(true);
		triggered = true;
		if (Random.value <= chanceToFailExplosion) // 15% failure chance
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

	public override bool VirtualEntityTriggerEnter(Collider other)
	{
		if (isArmed)
			return true;
		if (!triggered && other.isTrigger && other.GetComponent<Entity>())
		{
			StopCoroutine(detonationCor);
			TryExplode();
			return true;
		}
		return false;
	}

	public override void VirtualUpdate()
	{
		if (!isArmed) return;
		base.VirtualUpdate();
	}
}