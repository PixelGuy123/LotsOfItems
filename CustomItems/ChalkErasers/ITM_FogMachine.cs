using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LotsOfItems.ItemPrefabStructures;
using PixelInternalAPI.Extensions;
using LotsOfItems.Components;

namespace LotsOfItems.CustomItems.ChalkErasers
{
	public class ITM_FogMachine : Item, IItemPrefab
	{
		[SerializeField]
		internal RaycastBlocker fogParticlesPre;
		[SerializeField] 
		internal float minDuration = 28f, maxDuration = 31f, smallDelayBeforeDespawn = 5f;
		[SerializeField] 
		internal int distance = 5; // Distance for DijkstraMap (10x10 area)
		[SerializeField] 
		internal Color initialFogColor = Color.white, finalFogColor = Color.red; // Final malfunction red color
		[SerializeField]
		internal SpriteRenderer renderer;
		[SerializeField]
		internal PropagatedAudioManager audMan;
		[SerializeField]
		internal SoundObject audWorking, audStop;

		private readonly List<RaycastBlocker> activeFogs = [];

		public void SetupPrefab(ItemObject itm)
		{
			audMan = gameObject.CreatePropagatedAudioManager(45f, 125f);
			audWorking = this.GetSound("fogMachine_loop.wav", "LtsOItems_Vfx_FogMachine_Working", SoundType.Effect, Color.white);
			audStop = this.GetSound("fogMachine_end.wav", "LtsOItems_Vfx_FogMachine_Stops", SoundType.Effect, Color.white);

			renderer = ObjectCreationExtensions.CreateSpriteBillboard(itm.itemSpriteLarge);
			renderer.transform.SetParent(transform);
			renderer.transform.localPosition = Vector3.up * 1.32f;
			renderer.transform.localScale = Vector3.one * 2f;
			renderer.name = "FogMachineSprite";

			fogParticlesPre = Plugin.Extensions.GetRawChalkParticleGenerator();

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

			DijkstraMap dMap = new(ec, PathType.Nav, []);
			dMap.Calculate(distance + 1, true, [centerCellPos]);

			foreach (Cell cell in dMap.FoundCells())
				InstantiateFog(cell);
			InstantiateFog(ec.CellFromPosition(centerCellPos));

			float duration = Random.Range(minDuration, maxDuration),
				timer = duration;

			while (timer > 0f)
			{
				renderer.color = Color.Lerp(initialFogColor,
					finalFogColor, 
					1f - (timer / duration));

				timer -= Time.deltaTime * ec.EnvironmentTimeScale;
				yield return null;
			}


			foreach (var fog in activeFogs)
				fog.DisablePermanently(); // Should disable emission too

			audMan.FlushQueue(true);
			audMan.QueueAudio(audStop);

			timer = smallDelayBeforeDespawn;
			while (timer > 0f)
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
			activeFogs.Add(fog);
		}
	}
}
