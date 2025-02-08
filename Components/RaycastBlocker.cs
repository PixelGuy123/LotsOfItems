using System.Collections.Generic;
using UnityEngine;

namespace LotsOfItems.Components
{
	public class RaycastBlocker : MonoBehaviour
	{
		[SerializeField]
		public ParticleSystem system;

		bool isEnabled = false;


		void Start()
		{
			var e = system.emission;
			isEnabled = e.enabled;
		}
		void OnTriggerEnter(Collider other)
		{
			if (isEnabled && other.isTrigger)
			{
				var pm = other.GetComponent<PlayerManager>();
				if (pm && !touchedPlayers.Contains(pm))
				{
					touchedPlayers.Add(pm);
					pm.SetInvisible(true);
				}
				else
				{
					var entity = other.GetComponent<Entity>();
					if (entity && !touchedEntities.Contains(entity))
					{
						touchedEntities.Add(entity);
						entity.SetVisible(false);
					}
				}
			}
		}

		void OnTriggerExit(Collider other)
		{
			if (isEnabled && other.isTrigger)
			{
				var pm = other.GetComponent<PlayerManager>();
				if (pm && touchedPlayers.Contains(pm))
				{
					touchedPlayers.Remove(pm);
					pm.SetInvisible(false);
				}
				else
				{
					var entity = other.GetComponent<Entity>();
					if (entity && touchedEntities.Contains(entity))
					{
						touchedEntities.Remove(entity);
						entity.SetVisible(true);
					}
				}
			}
		}

		public void DisablePermanently()
		{
			isEnabled = false;
			ParticleSystem.EmissionModule emission = system.emission;
			emission.enabled = false;

			foreach (var e in touchedEntities)
				e.SetVisible(true);
			foreach (var player in touchedPlayers)
				player.SetInvisible(false);

			touchedEntities.Clear();
			touchedPlayers.Clear();
		}


		readonly HashSet<Entity> touchedEntities = [];
		readonly HashSet<PlayerManager> touchedPlayers = [];
	}
}
