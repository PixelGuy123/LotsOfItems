using LotsOfItems.ItemPrefabStructures;
using UnityEngine;

namespace LotsOfItems.CustomItems.BSODAs;
public class ITM_EnergyFlavoredZestySoda : ITM_GenericBSODA
{
	[SerializeField]
	private float energyStamina = 200f; // Stamina given when used

	protected override void VirtualSetupPrefab(ItemObject itm)
	{
		base.VirtualSetupPrefab(itm);
		speed *= 2.25f;
		spriteRenderer.sprite = this.GetSprite("ShrinkRay_Soda.png", spriteRenderer.sprite.pixelsPerUnit);
		Destroy(GetComponentInChildren<ParticleSystem>().gameObject);
	}

	public override bool Use(PlayerManager pm)
	{
		pm.plm.stamina = energyStamina;
		return base.Use(pm);
	}
}
