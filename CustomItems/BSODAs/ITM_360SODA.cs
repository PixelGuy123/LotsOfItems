using LotsOfItems.ItemPrefabStructures;

namespace LotsOfItems.CustomItems.BSODAs;

public class ITM_360SODA : ITM_MultiDirectionalBSODA, IItemPrefab
{
    protected override void VirtualSetupPrefab(ItemObject itm)
    {
        base.VirtualSetupPrefab(itm);
        bsodaPrefab.spriteRenderer.sprite = this.GetSprite("ThreeSixtySODA_Spray.png", bsodaPrefab.spriteRenderer.sprite.pixelsPerUnit);
    }
}