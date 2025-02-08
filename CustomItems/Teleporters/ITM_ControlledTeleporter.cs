using UnityEngine;
using System.Collections.Generic;
using PixelInternalAPI.Extensions;

namespace LotsOfItems.CustomItems.Teleporters
{
	public class ITM_ControlledTeleporter : ITM_GenericTeleporter
	{
		protected override void VirtualSetupPrefab(ItemObject itm)
		{
			base.VirtualSetupPrefab(itm);
			audFailUse = GenericExtensions.FindResourceObjectByName<SoundObject>("ErrorMaybe");
		}

		[SerializeField]
		internal SoundObject audFailUse;
		public override bool Use(PlayerManager pm)
		{
			if (pm.ec.CellFromPosition(pm.transform.position).room.type == RoomType.Hall)
			{
				Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audFailUse);
				Destroy(gameObject);
				return false;
			}

			this.pm = pm;
			return base.Use(pm);
		}

		protected override void Teleport()
		{
			RoomController currentRoom = pm.ec.CellFromPosition(pm.transform.position).room;
			if (!currentRoom) // If this even happens anyhow
			{
				base.Teleport();
				return;
			}
			List<IntVector2> safeDestinations = [];

			foreach (var room in pm.ec.rooms)
			{
				if (room.category == currentRoom.category && room != currentRoom && room.entitySafeCells.Count != 0)
					safeDestinations.AddRange(room.entitySafeCells);
			}

			if (safeDestinations.Count == 0)
				safeDestinations.AddRange(currentRoom.entitySafeCells);

			if (safeDestinations.Count > 0)
			{
				pm.Teleport(
					pm.ec.CellFromPosition(
						safeDestinations[Random.Range(0, safeDestinations.Count)]
						).FloorWorldPosition
					);

				Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audTeleport);
			}
			else
				base.Teleport(); // Worst case scenario: just teleport randomly
		}
	}
}
