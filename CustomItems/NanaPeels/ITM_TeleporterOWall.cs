using LotsOfItems.ItemPrefabStructures;
using PixelInternalAPI.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LotsOfItems.CustomItems.NanaPeels
{
	public class ITM_TeleporterOWall : ITM_GenericNanaPeel, IItemPrefab
	{
		public void SetupPrefab(ItemObject itm)
		{
			audTeleport = GenericExtensions.FindResourceObjectByName<SoundObject>("Teleport");
			var rend = GetComponentInChildren<SpriteRenderer>();
			rend.sprite = this.GetSprite("TeleporterOWall_world.png", rend.sprite.pixelsPerUnit);
			endHeight = 1.35f;
		}

		public void SetupPrefabPost() { }

		[SerializeField]
		internal int maxHits = 5;

		[SerializeField]
		internal float teleportWallOffset = 5.5f;

		[SerializeField]
		internal SoundObject audTeleport;

		private int hitsLeft;

		readonly private List<Vector3> candidatePositions = [];

		bool waitingHitDelay = false;

		internal override void AdditionalSpawnContribute()
		{
			base.AdditionalSpawnContribute();
			hitsLeft = maxHits;

			GetPositions(ec);

			entity.OnTeleport += OnTeleport;
		}

		void OnTeleport(Vector3 pos) =>
			slippingEntitity?.Teleport(pos);

		private void GetPositions(EnvironmentController ec) // Spots that the teleporter can go to
		{
			candidatePositions.Clear();
			foreach (var cell in ec.AllTilesNoGarbage(false, false))
			{
				List<Direction> closedDirs = Directions.ClosedDirectionsFromBin(cell.ConstBin);

				if (closedDirs.Count == 0)
					continue;


				foreach (Direction dir in closedDirs)
				{
					Direction exitDir = dir.GetOpposite();
					Vector3 exitPosition = cell.FloorWorldPosition + (exitDir.ToVector3() * teleportWallOffset);

					candidatePositions.Add(exitPosition);
				}

			}
		}

		internal override bool OnCollisionOverride(RaycastHit hit)
		{
			if (!waitingHitDelay && slipping && !dying)
			{
				if (candidatePositions.Count == 0)
				{
					// If somehow no candidate exists, die :)
					End();
					return false;
				}
				waitingHitDelay = true;
				StartCoroutine(TeleportDelay(candidatePositions[Random.Range(0, candidatePositions.Count)])); // For some reason, entity.Teleport doesn't work in a collision call, so I'm using a delay for it

				direction = Vector3.Reflect(direction, hit.normal);
			}
			return false;
		}

		IEnumerator TeleportDelay(Vector3 posToGo)
		{
			yield return null;

			entity.Teleport(posToGo);
			audioManager.PlaySingle(audTeleport);

			if (--hitsLeft <= 0)
				End();
			
			waitingHitDelay = false;

		}
	}
}
