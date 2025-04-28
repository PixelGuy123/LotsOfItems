using LotsOfItems.ItemPrefabStructures;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.PlusExtensions;
using PixelInternalAPI.Extensions;
using System.Collections;
using UnityEngine;

namespace LotsOfItems.CustomItems
{
	public class ITM_GenericZestyEatable : Item, IItemPrefab
	{
		public void SetupPrefab(ItemObject itm)
		{
			audEat = GenericExtensions.FindResourceObjectByName<SoundObject>("ChipCrunch");
			gaugeSprite = itm.itemSpriteLarge;
			VirtualSetupPrefab(itm);
		}
		public void SetupPrefabPost() { }
		protected virtual void VirtualSetupPrefab(ItemObject itemObject) { }
		public override bool Use(PlayerManager pm)
		{
			if (!CanBeUsed())
			{
				Destroy(gameObject);
				return false;
			}
			this.pm = pm;

			var statModifier = pm.GetMovementStatModifier();

			if (affectorTime > 0f)
			{
				gauge = Singleton<CoreGameManager>.Instance.GetHud(pm.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, affectorTime);
				StartCoroutine(SpeedAffector(statModifier));
			}
			else if (CanBeDestroyed())
				Destroy(gameObject);

			float staminaValue = staminaSet != 0f ? (pm.plm.stamina < staminaSet ? staminaSet : pm.plm.stamina) :
				staminaGain != 0f ? pm.plm.stamina + staminaGain :
				statModifier.baseStats["staminaMax"] * maxMultiplier;

			pm.plm.stamina = pm.plm.stamina > pm.plm.staminaMax ? Mathf.Abs(pm.plm.stamina - staminaValue) + pm.plm.stamina : staminaValue;

			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audEat);
			if (audSecondEatNoise)
				Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audSecondEatNoise);



			return true;
		}

		protected virtual bool CanBeUsed() => true; // To make sure some eatables just don't accumulate too much stats over time

		protected virtual bool CanBeDestroyed() => true;

		IEnumerator SpeedAffector(PlayerMovementStatModifier stat)
		{
			ValueModifier mod = new(speedMultiplier, speedAddition),
				staminaRiseMod = new(multiplier: staminaRiseChanger),
				staminaDropMod = new(multiplier: staminaDropChanger),
				staminaMaxMod = new(addend: staminaMaxChanger);


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
				gauge.SetValue(affectorTime, timer);
				timer -= pm.ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}
			gauge.Deactivate();
			stat.RemoveModifier(mod);
			stat.RemoveModifier(staminaRiseMod);
			stat.RemoveModifier(staminaDropMod);
			stat.RemoveModifier(staminaMaxMod);

			if (CanBeDestroyed())
				Destroy(gameObject);
		}

		[SerializeField]
		internal float staminaSet = 0f, staminaGain = 0f, maxMultiplier = 1f, speedMultiplier = 1f, speedAddition = 0f, affectorTime = 0f,
			staminaDropChanger = 1f, staminaRiseChanger = 1f, staminaMaxChanger = 0f;

		[SerializeField]
		internal bool speedAffectorAffectsRunSpeed = true, speedAffectorAffectsWalkSpeed = true;

		[SerializeField]
		internal SoundObject audEat, audSecondEatNoise;
		[SerializeField]
		internal Sprite gaugeSprite;
		protected HudGauge gauge;
	}
}
