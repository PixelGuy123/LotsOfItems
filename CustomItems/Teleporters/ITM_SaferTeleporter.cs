using UnityEngine;
using LotsOfItems.Components;
using PixelInternalAPI.Extensions;

namespace LotsOfItems.CustomItems.Teleporters
{
	public class ITM_SaferTeleporter : ITM_GenericTeleporter
	{
		protected override void VirtualSetupPrefab(ItemObject itm)
		{
			base.VirtualSetupPrefab(itm);
			audFailUse = GenericExtensions.FindResourceObjectByName<SoundObject>("ErrorMaybe");
		}

		[SerializeField]
		internal float minDistanceToAvoidBaldi = 15f, posFromSeconds = 10f;

		[SerializeField]
		internal SoundObject audFailUse;

		PlayerPositionHistory histPm;
		Vector3 posFromSecondsAgo;

		public override bool Use(PlayerManager pm)
		{
			this.pm = pm;
			histPm = pm.gameObject.GetComponent<PlayerPositionHistory>();

			if (histPm)
			{
				posFromSecondsAgo = histPm.GetPositionFromSecondsAgo(posFromSeconds);
				var cell = pm.ec.CellFromPosition(posFromSecondsAgo);
				var baldi = pm.ec.GetBaldi();
				if (cell.Null || cell.offLimits || (baldi != null && Vector3.Distance(baldi.transform.position, posFromSecondsAgo) < minDistanceToAvoidBaldi))
				{
					Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audFailUse);
					Destroy(gameObject);
					return false;
				}
			}

			return base.Use(pm);
		}

		protected override void Teleport()
		{
			if (!histPm)
			{
				base.Teleport(); // Just normal teleport if no history found
				return;
			}

			pm.Teleport(posFromSecondsAgo);
			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audTeleport);
		}
	}
}
