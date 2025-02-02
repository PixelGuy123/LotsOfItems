using MTM101BaldAPI.Components;
using MTM101BaldAPI.PlusExtensions;
using System.Collections;
using UnityEngine;

namespace LotsOfItems.CustomItems
{
	public class ITM_GenericZestyEatable : Item
	{

		public override bool Use(PlayerManager pm)
		{
			if (!CanBeUsed())
			{
				Destroy(gameObject);
				return false;
			}
			this.pm = pm;

			if (affectorTime > 0f)
				StartCoroutine(SpeedAffector());
			else
				Destroy(gameObject);

			pm.plm.stamina = staminaSet != 0f ? staminaSet :
				staminaGain != 0f ? pm.plm.stamina + staminaGain :
				pm.plm.staminaMax * maxMultiplier;
			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audEat);

			

			return true;
		}

		protected virtual bool CanBeUsed() => true; // To make sure some eatables just don't accumulate too much stats over time

		IEnumerator SpeedAffector()
		{
			ValueModifier mod = new(speedMultiplier, speedAddition),
				staminaRiseMod = new(addend:staminaRiseChanger),
				staminaDropMod = new(addend: staminaDropChanger),
				staminaMaxMod = new(addend: staminaMaxChanger);


			var stat = pm.GetMovementStatModifier();
			if (speedAffectorAffectsRunSpeed)
				stat.AddModifier("runSpeed", mod);
			if (speedAffectorAffectsWalkSpeed)
				stat.AddModifier("walkSpeed", mod);

			stat.AddModifier("staminaRise", staminaRiseMod);
			stat.AddModifier("staminaDrop", staminaDropMod);
			stat.AddModifier("staminaMax", staminaMaxMod);

			float timer = affectorTime;
			while (timer > 0f)
			{
				timer -= pm.ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}

			stat.RemoveModifier(mod);
			stat.RemoveModifier(staminaRiseMod);
			stat.RemoveModifier(staminaDropMod);
			stat.RemoveModifier(staminaMaxMod);

			Destroy(gameObject);
		}

		[SerializeField]
		internal float staminaSet = 0f, staminaGain = 0f, maxMultiplier = 1f, speedMultiplier = 1f, speedAddition = 0f, affectorTime = 0f,
			staminaDropChanger = 0f, staminaRiseChanger = 0f, staminaMaxChanger = 0f;

		[SerializeField]
		internal bool speedAffectorAffectsRunSpeed = true, speedAffectorAffectsWalkSpeed = true;

		[SerializeField]
		internal SoundObject audEat;
	}
}
