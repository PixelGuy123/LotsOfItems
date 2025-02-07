﻿using System.Collections;
using UnityEngine;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.PlusExtensions;

namespace LotsOfItems.CustomItems.Eatables
{
	public class ITM_ChocolateFlavouredZestyBar : ITM_GenericZestyEatable
	{
		[SerializeField]
		internal float speedBoostVal = 1.15f, speedPenaltyValue = 0.85f, boostDuration = 30f;

		protected override bool CanBeDestroyed() =>
			false;

		public override bool Use(PlayerManager pm)
		{
			this.pm = pm;
			pm.plm.stamina = 200;
			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audEat);
			StartCoroutine(DynamicSpeedEffect());
			return base.Use(pm);
		}

		IEnumerator DynamicSpeedEffect()
		{
			var statModifier = pm.GetMovementStatModifier();

			ValueModifier boostSpeedMod = new(speedBoostVal, 0f);
			ValueModifier noStaminaDropMod = new(0f, 0f);
			statModifier.AddModifier("runSpeed", boostSpeedMod);
			statModifier.AddModifier("walkSpeed", boostSpeedMod);
			statModifier.AddModifier("staminaDrop", noStaminaDropMod);

			float timer = boostDuration;
			while (timer > 0f)
			{
				timer -= pm.ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}

			statModifier.RemoveModifier(boostSpeedMod);
			statModifier.RemoveModifier(noStaminaDropMod);

			ValueModifier penaltySpeedMod = new(speedPenaltyValue, 0f);
			statModifier.AddModifier("runSpeed", penaltySpeedMod);
			statModifier.AddModifier("walkSpeed", penaltySpeedMod);

			while (pm.plm.stamina > statModifier.baseStats["staminaMax"])
			{
				yield return null;
			}

			statModifier.RemoveModifier(penaltySpeedMod);
			Destroy(gameObject);
		}
	}
}
