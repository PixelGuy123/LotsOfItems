using System.Collections;
using LotsOfItems.ItemPrefabStructures;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.PlusExtensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.Eatables
{
	public class ITM_ThornGlognut : ITM_GenericZestyEatable
	{
		[SerializeField]
		internal float speedBoostVal = 1.1f, staminaDrain = 1.2f;

		[SerializeField]
		internal int noiseValue = 100;
		protected override bool CanBeDestroyed() =>
			false;

		protected override void VirtualSetupPrefab(ItemObject itemObject)
		{
			base.VirtualSetupPrefab(itemObject);
			audEat = this.GetSound("Glognut_CrunchGlognut.wav", audEat.soundKey, SoundType.Effect, Color.white);
			audEat.additionalKeys = [new() { key = "LtsOItems_Vfx_Scream", time = 0.295f }];
		}

		public override bool Use(PlayerManager pm)
		{
			pm.ec.MakeNoise(pm.transform.position, noiseValue);

			// Set initial stamina and start effects
			pm.plm.stamina = 300f;
			StartCoroutine(ThornEffect(pm));
			return base.Use(pm);
		}

		IEnumerator ThornEffect(PlayerManager pm)
		{
			var stats = pm.GetMovementStatModifier();
			float baseStaminaMax = stats.baseStats["staminaMax"];

			// Apply modifiers
			ValueModifier speedMod = new(speedBoostVal, 0f); // 10% speed boost
			ValueModifier staminaDropMod = new(staminaDrain, 0f); // 20% faster drain

			stats.AddModifier("runSpeed", speedMod);
			stats.AddModifier("walkSpeed", speedMod);
			stats.AddModifier("staminaDrop", staminaDropMod);

			// Wait until stamina drops below base maximum
			while (pm.plm.stamina >= baseStaminaMax)
			{
				yield return null;
			}

			// Remove modifiers and destroy
			stats.RemoveModifier(speedMod);
			stats.RemoveModifier(staminaDropMod);
			Destroy(gameObject);
		}
	}
}