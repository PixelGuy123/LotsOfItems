using LotsOfItems.Components;
using LotsOfItems.ItemPrefabStructures;
using UnityEngine;

public class ITM_WDNoSqueeX : ITM_NoSquee, IItemPrefab
{
    [SerializeField]
    InvisibleCoverCloud coverPre;
    [SerializeField]
    internal ItemObject nextItem = null;
    public void SetupPrefab(ItemObject itm)
    {
        coverPre = InvisibleCoverCloud.GetPrefab();
        time += 30f;
        distance *= 2; // Double the distance
    }

    public void SetupPrefabPost() { }

    public override bool Use(PlayerManager pm)
    {
        bool flag = base.Use(pm);

        foreach (var cell in silencedCells)
        {
            var coverCloud = Instantiate(coverPre, cell.CenterWorldPosition, Quaternion.identity);
            coverCloud.Ec = pm.ec;
            coverCloud.StartEndTimer(time);
        }


        if (flag && nextItem)
        {
            pm.itm.SetItem(nextItem, pm.itm.selectedItem);
            return false;
        }

        return flag;
    }
}