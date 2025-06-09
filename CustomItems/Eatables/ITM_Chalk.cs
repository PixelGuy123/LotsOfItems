using System.Collections;
using UnityEngine;

namespace LotsOfItems.CustomItems.Eatables
{
	public class ITM_Chalk : ITM_GenericZestyEatable
	{
		[SerializeField]
		internal CoverCloud blocker;

		[SerializeField]
		internal float dustLifeTime = 30f;

		protected override bool CanBeDestroyed() =>
			false;
		protected override void VirtualSetupPrefab(ItemObject itemObject)
		{
			base.VirtualSetupPrefab(itemObject);
			blocker = Plugin.ParticleExtensions.GetRawChalkParticleGenerator(true);
		}
		public override bool Use(PlayerManager pm)
		{
			this.pm = pm;
			gauge = Singleton<CoreGameManager>.Instance.GetHud(pm.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, dustLifeTime);
			StartCoroutine(ChalkDustEffect());
			return base.Use(pm);
		}

		private IEnumerator ChalkDustEffect()
		{
			var block = Instantiate(blocker, transform);
			block.Ec = pm.ec;
			block.StartEndTimer(dustLifeTime);

			pm.SetHidden(true);
			float timer = dustLifeTime;
			while (timer > 0f)
			{
				gauge.SetValue(dustLifeTime, timer);
				timer -= pm.ec.EnvironmentTimeScale * Time.deltaTime;
				block.transform.position = pm.transform.position;
				yield return null;
			}
			gauge.Deactivate();
			pm.SetHidden(false);
			Destroy(gameObject);
		}
	}
}
