using System.Collections;
using UnityEngine;
using LotsOfItems.Components;

namespace LotsOfItems.CustomItems.Eatables
{
	public class ITM_Chalk : ITM_GenericZestyEatable
	{
		[SerializeField]
		internal RaycastBlocker blocker;

		[SerializeField]
		internal float dustLifeTime = 30f;
		protected override bool CanBeDestroyed() =>
			false;
		protected override void VirtualSetupPrefab(ItemObject itemObject)
		{
			base.VirtualSetupPrefab(itemObject);
			blocker = Plugin.Extensions.GetRawChalkParticleGenerator(true);

		}
		public override bool Use(PlayerManager pm)
		{
			this.pm = pm;
			StartCoroutine(ChalkDustEffect());
			return base.Use(pm);
		}

		private IEnumerator ChalkDustEffect()
		{
			var block = Instantiate(blocker);
			pm.SetInvisible(true);
			float timer = dustLifeTime;
			while (timer > 0f)
			{
				timer -= pm.ec.EnvironmentTimeScale * Time.deltaTime;
				block.transform.position = pm.transform.position;
				yield return null;
			}
			pm.SetInvisible(false);
			Destroy(block.gameObject);
		}
	}
}
