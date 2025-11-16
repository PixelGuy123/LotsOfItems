using System.Collections;
using LotsOfItems.Components;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using MTM101BaldAPI;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.BSODAs
{
	public class ITM_ShakenSoda : ITM_GenericNanaPeel
	{
		[SerializeField]
		private SoundObject audPressureSound, audExplode;

		[SerializeField]
		internal float explosionRadius = 65f, timerBeforeExploding = 7.5f, explosionForce = 85f, explosionAcceleration = -45f, shakingAnimationSpeed = 10f,
			puddleLifeTime = 30f;

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		private SodaPuddle puddlePrefab;

		[SerializeField]
		internal Sprite[] animation;

		[SerializeField]
		internal LayerMask collisionLayer = LotOfItemsPlugin.onlyNpcPlayerLayers;

		Coroutine animationCor;

		protected override void VirtualSetupPrefab(ItemObject itm)
		{
			// Create puddle prefab with floor-facing billboard
			var puddleObject = ObjectCreationExtensions.CreateSpriteBillboard(
				this.GetSprite("ShakenSoda_Soda.png", 8f), // temp sprite
				false
			).AddSpriteHolder(out var puddleSprite, 0.05f, LayerStorage.ignoreRaycast);
			puddleSprite.name = "Sprite";
			puddleSprite.gameObject.layer = 0;
			puddleSprite.transform.Rotate(90, 0, 0); // Face downward
			puddlePrefab = puddleObject.gameObject.AddComponent<SodaPuddle>();
			puddlePrefab.gameObject.ConvertToPrefab(true);
			puddlePrefab.name = "SodaPuddle";

			var collider = puddlePrefab.gameObject.AddComponent<BoxCollider>();
			collider.isTrigger = true;
			collider.size = new Vector3(4.5f, 1f, 4.5f);
			collider.center = Vector3.up * 2f;

			renderer = GetComponentInChildren<SpriteRenderer>();
			animation = this.GetSpriteSheet("ShakenSoda_world.png", 4, 3, renderer.sprite.pixelsPerUnit + 25f);

			renderer.sprite = animation[0];

			audExplode = LotOfItemsPlugin.assetMan.Get<SoundObject>("aud_explode");
			audPressureSound = this.GetSound("ShakenSoda_PitchNoise.wav", "LtsOItems_Vfx_HighPitchNoise", SoundType.Effect, Color.white);

			endHeight = 1.27f;
		}

		internal override bool EntityTriggerStayOverride(Collider other, bool validCollision) =>
			false;
		internal override void OnFloorHit()
		{
			StartCoroutine(DetonationSequence());
			animationCor = StartCoroutine(ShakingAnimation());
		}

		private IEnumerator ShakingAnimation()
		{
			float frame = 0f;
			while (true)
			{
				frame += ec.EnvironmentTimeScale * Time.deltaTime * shakingAnimationSpeed;
				frame %= animation.Length - 1;
				renderer.sprite = animation[Mathf.FloorToInt(frame)];
				yield return null;
			}
		}


		private IEnumerator DetonationSequence()
		{
			// Play escalating pitch sound
			audioManager.maintainLoop = true;
			audioManager.SetLoop(true);
			audioManager.QueueAudio(audPressureSound);

			float timer = timerBeforeExploding;
			while (timer > 0f)
			{
				audioManager.audioDevice.pitch = 1f + (1f - (timer / timerBeforeExploding));
				timer -= Time.deltaTime * ec.EnvironmentTimeScale;
				yield return null;
			}

			// Explosion force
			this.Explode(explosionRadius, collisionLayer, explosionForce, explosionAcceleration);

			// Create puddle
			Instantiate(puddlePrefab, transform.position.ZeroOutY(), Quaternion.identity)
				.Initialize(puddleLifeTime, ec);

			StopCoroutine(animationCor);
			audioManager.FlushQueue(true);
			audioManager.PlaySingle(audExplode);
			renderer.enabled = false;
			while (audioManager.AnyAudioIsPlaying)
				yield return null;

			Destroy(gameObject);
		}
	}
}