using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using UnityEngine;

namespace LotsOfItems.CustomItems.BSODAs
{
    public class ITM_RSODA : ITM_GenericBSODA
    {
        protected override void VirtualSetupPrefab(ItemObject itm)
        {
            base.VirtualSetupPrefab(itm);
            spriteRenderer.sprite = this.GetSprite("RSODA_Soda.png", spriteRenderer.sprite.pixelsPerUnit);
            this.DestroyParticleIfItHasOne();
        }
        [SerializeField]
        float baseSpeed = 15f;

        public override void VirtualUpdate()
        {
            speed = baseSpeed * (1 + activityMods.Count); // literally this
            base.VirtualUpdate();
        }
    }
}