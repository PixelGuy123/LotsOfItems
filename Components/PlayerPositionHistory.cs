using System.Collections.Generic;
using UnityEngine;

namespace LotsOfItems.Components
{
	public class PlayerPositionHistory : MonoBehaviour
	{
		[SerializeField]
		internal PlayerManager pm;

		[SerializeField]
		internal float cutoffInterval = 10f;

		private struct PositionRecord
		{
			public float time;
			public Vector3 position;
		}

		private readonly List<PositionRecord> positionHistory = [];

		private void Update()
		{
			float currentTime = pm.ec.SurpassedGameTime;
			positionHistory.Add(new PositionRecord { time = currentTime, position = transform.position });

			float cutoff = currentTime - cutoffInterval; // Basically an implicit range check

			while (positionHistory.Count > 0 && positionHistory[0].time < cutoff) // While loop to make sure it remove all the items that are out of range
				positionHistory.RemoveAt(0);
		}

		public Vector3 GetPositionFromSecondsAgo(float seconds)
		{
			float currentTime = pm.ec.SurpassedGameTime;
			float targetTime = currentTime - seconds;
			if (positionHistory.Count == 0)
				return transform.position;

			for (int i = 0; i < positionHistory.Count; i++)
			{
				if (positionHistory[i].time >= targetTime)
					return positionHistory[i].position;
			}
			return positionHistory[positionHistory.Count - 1].position;
		}
	}
}
