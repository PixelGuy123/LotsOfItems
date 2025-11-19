using MTM101BaldAPI.Registers;

namespace LotsOfItems.CustomItems.Eatables;

public class ITM_Banana : ITM_GenericZestyEatable
{
    public ItemObject peel;

    protected override void VirtualSetupPrefab(ItemObject itemObject)
    {
        base.VirtualSetupPrefab(itemObject);
        staminaGain = 50f;
        peel = ItemMetaStorage.Instance.FindByEnum(Items.NanaPeel).value;
    }

    public override bool Use(PlayerManager pm)
    {
        // Eat the banana
        bool used = base.Use(pm);

        if (used)
        {
            pm.itm.SetItem(peel, pm.itm.selectedItem);
            return false;
        }
        return used;
    }
}