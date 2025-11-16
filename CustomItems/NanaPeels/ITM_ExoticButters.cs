using LotsOfItems.ItemPrefabStructures;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.NanaPeels;

public class ITM_ExoticButters : ITM_GenericNanaPeel, IItemPrefab
{
    [SerializeField]
    float staminaGain = 50f;

    [SerializeField]
    internal ItemObject nextItem = null;

    [SerializeField]
    SoundObject audEat;

    protected override void VirtualSetupPrefab(ItemObject itemObject)
    {
        base.VirtualSetupPrefab(itemObject);
        audEat = GenericExtensions.FindResourceObjectByName<SoundObject>("ChipCrunch");
        GetComponentInChildren<SpriteRenderer>().sprite = this.GetSprite("ExoticButters_peel.png", 25f);
    }

    public override bool Use(PlayerManager pm)
    {
        bool used = base.Use(pm);
        if (nextItem != null)
        {
            pm.itm.SetItem(nextItem, pm.itm.selectedItem);
            return false;
        }
        Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audEat);
        pm.plm.stamina += staminaGain;
        return used;
    }
}