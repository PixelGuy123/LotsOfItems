//using System.Collections.Generic;
//using UnityEngine;

namespace LotsOfItems.Components;

	// Unused since the existence of CoverCloud (0.10.x) which does basically the same thing as here 
	/*
	public class RaycastBlocker : MonoBehaviour
	{
		[SerializeField]
		public ParticleSystem system;

		[SerializeField]
		public Collider[] colliders;

		bool isEnabled = false;


		void Start()
		{
			var e = system.emission;
			isEnabled = e.enabled;
		}
		void OnTriggerEnter(Collider other)
		{
			if (isEnabled && other.isTrigger && other.CompareTag("Player"))
			{
				var pm = other.GetComponent<PlayerManager>();
				if (pm && !touchedPlayers.Contains(pm))
				{
					touchedPlayers.Add(pm);
					pm.Invisible (true);
				}
			}
		}

		void OnTriggerExit(Collider other)
		{
			if (isEnabled && other.isTrigger && other.CompareTag("Player"))
			{
				var pm = other.GetComponent<PlayerManager>();
				if (pm && touchedPlayers.Contains(pm))
				{
					touchedPlayers.Remove(pm);
					pm.SetInvisible(false);
				}
			}
		}

		public void DisablePermanently()
		{
			isEnabled = false;
			ParticleSystem.EmissionModule emission = system.emission;
			emission.enabled = false;

			foreach (var player in touchedPlayers)
				player.SetInvisible(false);

			touchedPlayers.Clear();

			for (int i = 0; i < colliders.Length; i++)
				colliders[i].enabled = false;

		}

		void OnDestroy()
		{
			foreach (var player in touchedPlayers)
				player.SetInvisible(false);
		}


		readonly HashSet<PlayerManager> touchedPlayers = [];
	}
	*/
