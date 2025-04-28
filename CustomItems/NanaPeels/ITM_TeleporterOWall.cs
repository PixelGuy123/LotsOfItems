using LotsOfItems.ItemPrefabStructures;
using PixelInternalAPI.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LotsOfItems.CustomItems.NanaPeels
{
	public class ITM_TeleporterOWall : ITM_GenericNanaPeel
	{

		protected override void VirtualSetupPrefab(ItemObject itm)
		{
			audTeleport = GenericExtensions.FindResourceObjectByName<SoundObject>("Teleport");
			var rend = GetComponentInChildren<SpriteRenderer>();
			rend.sprite = this.GetSprite("TeleporterOWall_world.png", rend.sprite.pixelsPerUnit);
			endHeight = 1.35f;
		}

		[SerializeField]
		internal int maxHits = 5;

		[SerializeField]
		internal float teleportWallOffset = 4.5f;

		[SerializeField]
		internal SoundObject audTeleport;

		private int hitsLeft;

		readonly private List<Cell> candidatePositions = [], filteredPositions = [];

		bool waitingHitDelay = false;

		internal override void AdditionalSpawnContribute()
		{
			base.AdditionalSpawnContribute();
			hitsLeft = maxHits;

			GetPositions(ec);
		}

		private void GetPositions(EnvironmentController ec) // Spots that the teleporter can go to
		{
			candidatePositions.Clear();
			foreach (var cell in ec.AllTilesNoGarbage(false, false))
			{
				if (cell.shape == TileShapeMask.Open)
					continue;

				candidatePositions.Add(cell);
			}
		}

		internal override bool OnCollisionOverride(RaycastHit hit)
		{
			if (!waitingHitDelay && slipping && !dying)
			{
				direction = Vector3.Reflect(direction, hit.normal);
				if (candidatePositions.Count == 0)
				{
					// If somehow no candidate exists, die :)
					End();
					return false;
				}

				filteredPositions.Clear();
				var dirToGo = Directions.DirFromVector3(direction, 45f).GetOpposite();


				for (int i = 0; i < candidatePositions.Count; i++)
				{
					if (candidatePositions[i].HasWallInDirection(dirToGo))
						filteredPositions.Add(candidatePositions[i]);
				}

				if (filteredPositions.Count == 0)
				{
					End();
					return false;
				}

				waitingHitDelay = true;
				StartCoroutine(TeleportDelay(filteredPositions[Random.Range(0, filteredPositions.Count)].FloorWorldPosition + (dirToGo.ToVector3() * teleportWallOffset))); // For some reason, entity.Teleport doesn't work in a collision call, so I'm using a delay for it
			}
			return false;
		}

		IEnumerator TeleportDelay(Vector3 posToGo)
		{
			yield return null;

			entity.Teleport(posToGo);
			slippingEntity?.Teleport(posToGo);
			audioManager.PlaySingle(audTeleport);

			if (--hitsLeft <= 0)
				End();

			waitingHitDelay = false;

		}
	}
}
