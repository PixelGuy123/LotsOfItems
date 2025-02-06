using UnityEngine;

namespace LotsOfItems.CustomItems.Eatables
{
	public class ITM_VanillaZestyBar : ITM_GenericZestyEatable
	{
		public override bool Use(PlayerManager pm)
		{
			this.pm = pm;
			pm.RuleBreak("Eating", 2f, 0.5f);
			return base.Use(pm);
		}

		void Update()
		{
			currentNoiseTimer -= pm.ec.EnvironmentTimeScale * Time.deltaTime;

			if (currentNoiseTimer <= 0f)
			{
				pm.ec.MakeNoise(pm.transform.position, noiseLevel);
				currentNoiseTimer += noiseDelay;
			}
		}

		float currentNoiseTimer = 0f;

		[SerializeField]
		internal float noiseDelay = 2.5f;

		[SerializeField]
		internal int noiseLevel = 16;
	}
}
