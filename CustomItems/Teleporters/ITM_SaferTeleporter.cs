using UnityEngine;
using System.Collections;
using LotsOfItems.ItemPrefabStructures;
using MTM101BaldAPI;

namespace LotsOfItems.CustomItems.Teleporters
{
	public class ITM_SaferTeleporter : ITM_GenericTeleporter
	{
		[SerializeField]
		internal float minDistanceToAvoidBaldi = 15f;

		public override bool Use(PlayerManager pm)
		{
			this.pm = pm;
			// TODO: Retrieve the saved position from 10 seconds ago via a Harmony patch.
			Vector3 savedPosition = Vector3.zero; // Placeholder

			var baldi = pm.ec.GetBaldi();
			if (baldi != null && Vector3.Distance(baldi.transform.position, savedPosition) < minDistanceToAvoidBaldi)
				return false;
			
			return base.Use(pm);
		}

		protected override void Teleport()
		{
			// TODO: Retrieve the saved position from 10 seconds ago via a Harmony patch.
			Vector3 savedPosition = Vector3.zero; // Placeholder
			pm.Teleport(savedPosition);
			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audTeleport);
		}
	}
}
