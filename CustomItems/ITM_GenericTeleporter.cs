﻿using UnityEngine;
using System.Collections;

namespace LotsOfItems.CustomItems
{
	public class ITM_GenericTeleporter : Item
	{
		[SerializeField]
		internal SoundObject audTeleport;

		[SerializeField]
		internal int minTeleports = 12, maxTeleports = 16;

		[SerializeField]
		internal float baseTime = 0.2f, increaseFactor = 1.1f;

		public override bool Use(PlayerManager pm)
		{
			this.pm = pm;
			StartCoroutine(Teleporter());
			return true;
		}

		private IEnumerator Teleporter()
		{
			pm.plm.Entity.SetInteractionState(false);
			pm.plm.Entity.SetFrozen(true);
			int teleports = Random.Range(minTeleports, maxTeleports + 1);
			int teleportCount = 0;
			float currentTime = baseTime;
			while (teleportCount < teleports)
			{
				currentTime -= Time.deltaTime * pm.ec.EnvironmentTimeScale;
				if (currentTime <= 0f)
				{
					Teleport();
					teleportCount++;
					baseTime *= increaseFactor;
					currentTime = baseTime;
				}
				yield return null;
			}
			pm.plm.Entity.SetInteractionState(true);
			pm.plm.Entity.SetFrozen(false);
			Destroy(gameObject);
			yield break;
		}

		protected virtual void Teleport()
		{
			pm.Teleport(pm.ec.RandomCell(false, false, true).FloorWorldPosition);
			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audTeleport);
		}
	}
}
