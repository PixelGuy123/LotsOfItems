using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;

namespace LotsOfItems.CustomItems.BSODAs;

public class ITM_LaboratoryBSODA : ITM_MultiDirectionalBSODA, IItemPrefab
{
    protected override void VirtualSetupPrefab(ItemObject itm)
    {
        bsodaPrefab = ItemExtensions.GetVariantInstance<ITM_BSODA>(Items.DietBsoda);
        audUse = bsodaPrefab.sound;
        numOfSodas = 4;
    }
}