using PixelInternalAPI.Extensions;
using System.Collections.Generic;
using UnityEngine;

namespace LotsOfItems.CustomItems.Teleporters
{
	public class ITM_DetentionTeleporter : ITM_GenericTeleporter
	{
		[SerializeField]
		internal SoundObject audFail;

		[SerializeField]
		internal float minDetentionTime = 10f, maxDetentionTime = 15f;

		protected override void VirtualSetupPrefab(ItemObject itm)
		{
			base.VirtualSetupPrefab(itm);
			audFail = GenericExtensions.FindResourceObjectByName<SoundObject>("ErrorMaybe");
		}

		public override bool Use(PlayerManager pm)
		{
			for (int i = 0; i < pm.ec.rooms.Count; i++)
			{
				if (pm.ec.rooms[i].category == RoomCategory.Office)
					candidateRooms.Add(pm.ec.rooms[i]);
			}

			if (candidateRooms.Count == 0)
			{
				Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audFail);
				return false;
			}

			return base.Use(pm);
		}

		protected override void Teleport()
		{
			int index = Random.Range(0, candidateRooms.Count);
			RoomController targetRoom = candidateRooms[index];

			pm.Teleport(targetRoom.RandomEntitySafeCellNoGarbage().FloorWorldPosition);

			targetRoom.functionObject.GetComponent<DetentionRoomFunction>()
				.Activate(Random.Range(minDetentionTime, maxDetentionTime), pm.ec);

			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audTeleport);
		}

		readonly List<RoomController> candidateRooms = [];
	}
}
