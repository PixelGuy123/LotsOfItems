using LotsOfItems.ItemPrefabStructures;
using UnityEngine;
using LotsOfItems.Plugin;

namespace LotsOfItems.CustomItems.BSODAs;
public class ITM_EnergyFlavoredZestySoda : ITM_GenericBSODA
{
	[SerializeField]
	private float energyStamina = 200f; // Stamina given when used

	protected override void VirtualSetupPrefab(ItemObject itm)
	{
		base.VirtualSetupPrefab(itm);
		
		speed *= 2.25f;
		spriteRenderer.sprite = this.GetSprite("EnergyFlavoredZestySoda_spray.png", spriteRenderer.sprite.pixelsPerUnit);
		this.DestroyParticleIfItHasOne();
	}

	public override bool Use(PlayerManager pm)
	{
		pm.plm.stamina = energyStamina;
		return base.Use(pm);
	}
}
