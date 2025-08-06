using LotsOfItems.CustomItems;
using UnityEngine;

public class ITM_PocketTeleporter : ITM_GenericTeleporter
{
    [SerializeField]
    public ItemObject nextItem = null;

    protected override void VirtualSetupPrefab(ItemObject itm)
    {
        base.VirtualSetupPrefab(itm);
        minTeleports = 1;
        maxTeleports = 1;
        baseTime = 0f;
    }

    public override bool Use(PlayerManager pm)
    {
        base.Use(pm);
        if (nextItem != null)
        {
            pm.itm.SetItem(nextItem, pm.itm.selectedItem);
            return false;
        }
        return true;
    }
}