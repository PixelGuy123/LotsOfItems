using System.Collections;
using System.Collections.Generic;
using LotsOfItems.Components;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.ChalkErasers
{
	public class ITM_FogMachine : Item, IItemPrefab
	{
		[SerializeField]
		internal CoverCloud fogParticlesPre;
		[SerializeField]
		internal Sprite[] explosionSheet;
		[SerializeField]
		internal float minDuration = 28f, maxDuration = 31f, smallDelayBeforeDespawn = 5f, explosionSpeed = 12f;
		[SerializeField]
		internal int distance = 5, minBreakParticles = 12, maxBreakParticles = 16; // Distance for DijkstraMap (10x10 area)
		[SerializeField]
		internal Color initialFogColor = Color.white, finalFogColor = Color.red; // Final malfunction red color
		[SerializeField]
		internal SpriteRenderer renderer;
		[SerializeField]
		internal PropagatedAudioManager audMan;
		[SerializeField]
		internal SoundObject audWorking, audExplode;

		private readonly List<CoverCloud> activeFogs = [];

		public void SetupPrefab(ItemObject itm)
		{
			audMan = gameObject.CreatePropagatedAudioManager(45f, 125f);
			audWorking = this.GetSound("fogMachine_loop.wav", "LtsOItems_Vfx_FogMachine_Working", SoundType.Effect, Color.white);
			audExplode = LotOfItemsPlugin.assetMan.Get<SoundObject>("aud_explode");

			explosionSheet = this.GetSpriteSheet("FogMachine_world.png", 3, 2, itm.itemSpriteLarge.pixelsPerUnit);

			renderer = ObjectCreationExtensions.CreateSpriteBillboard(explosionSheet[0]);
			renderer.transform.SetParent(transform);
			renderer.transform.localPosition = Vector3.up * 1.32f;
			//renderer.transform.localScale = Vector3.one * 2f;
			renderer.name = "FogMachineSprite";

			fogParticlesPre = ParticleExtensions.GetRawChalkParticleGenerator();
		}
		public void SetupPrefabPost() { }

		public override bool Use(PlayerManager pm)
		{
			this.pm = pm;
			transform.position = pm.ec.CellFromPosition(pm.transform.position).FloorWorldPosition;
			StartCoroutine(ActivateFog());
			return true;
		}

		private IEnumerator ActivateFog()
		{
			audMan.maintainLoop = true;
			audMan.SetLoop(true);
			audMan.QueueAudio(audWorking);


			EnvironmentController ec = pm.ec;
			IntVector2 centerCellPos = IntVector2.GetGridPosition(transform.position);

			DijkstraMap dMap = new(ec, PathType.Nav, distance, []);
			dMap.Calculate(distance + 1, true, [centerCellPos]);

			foreach (Cell cell in dMap.FoundCells())
				InstantiateFog(cell);
			InstantiateFog(ec.CellFromPosition(centerCellPos));

			float duration = Random.Range(minDuration, maxDuration),
				timer = duration;
			foreach (var fog in activeFogs)
				fog.StartEndTimer(timer - 1f);

			while (timer > 0f)
			{
				renderer.color = Color.Lerp(initialFogColor,
					finalFogColor,
					1f - (timer / duration));

				timer -= Time.deltaTime * ec.EnvironmentTimeScale;
				yield return null;
			}

			audMan.FlushQueue(true);
			audMan.QueueAudio(audExplode);
			float frame = 0f;
			while (true)
			{
				frame += ec.EnvironmentTimeScale * Time.deltaTime * explosionSpeed;
				if (frame >= explosionSheet.Length)
					break;

				renderer.sprite = explosionSheet[Mathf.FloorToInt(frame)];
				yield return null;
			}
			renderer.enabled = false;

			timer = smallDelayBeforeDespawn;
			while (timer > 0f || audMan.QueuedAudioIsPlaying)
			{
				timer -= Time.deltaTime * ec.EnvironmentTimeScale;
				yield return null;
			}

			Destroy(gameObject);
		}

		private void InstantiateFog(Cell cell)
		{
			var fog = Instantiate(fogParticlesPre);
			fog.transform.SetParent(transform);
			fog.transform.position = cell.CenterWorldPosition;
			fog.Ec = pm.ec;
			activeFogs.Add(fog);
		}
	}
}
