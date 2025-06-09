using LotsOfItems.ItemPrefabStructures;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.PlusExtensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.Nametags
{
	public class ITM_EnergyNametag : ITM_Nametag, IItemPrefab
	{
		public void SetupPrefab(ItemObject itm) =>
		   //NametagAnimationOverrider.InstallOverrider(this, this.GetSpriteSheet("EnergyTag_world.png", 4, 2, 25f));
		   gaugeSprite = itm.itemSpriteSmall;


		public void SetupPrefabPost() { }

		[SerializeField]
		internal float speedMultiplier = 1.15f;

		[SerializeField]
		[Range(0f, 1f)]
		internal float staminaDropFactor = 0.45f, staminaMaxSet = 100f;

		ValueModifier stamDropMod, speedMod;

		PlayerMovementStatModifier statMod;

		public override bool Use(PlayerManager pm)
		{
			this.pm = pm;

			stamDropMod = new(staminaDropFactor);
			speedMod = new(speedMultiplier);

			statMod = pm.GetMovementStatModifier();
			statMod.AddModifier("staminaDrop", stamDropMod);
			statMod.AddModifier("walkSpeed", speedMod);
			statMod.AddModifier("runSpeed", speedMod);

			if (pm.plm.stamina < staminaMaxSet)
				pm.plm.stamina = staminaMaxSet;

			return base.Use(pm);
		}
		void OnDestroy()
		{
			pm.plm.stamina = 0;
			if (statMod)
			{
				statMod.RemoveModifier(stamDropMod);
				statMod.RemoveModifier(speedMod);
			}
		}

	}
}
