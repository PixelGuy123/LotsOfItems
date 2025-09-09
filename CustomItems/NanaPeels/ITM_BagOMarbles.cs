using LotsOfItems.ItemPrefabStructures;
using UnityEngine;

namespace LotsOfItems.CustomItems.NanaPeels;

public class ITM_BagOMarbles : ITM_GenericNanaPeel
{
    [SerializeField]
    internal ItemObject nextItem = null;

    protected override void VirtualSetupPrefab(ItemObject itm)
    {
        base.VirtualSetupPrefab(itm);
        speed = 25f;
        maxTime = 15f;
        endHeight = 0.5f;
        startHeight = 1f;
        throwSpeed = 45f;
        gravity = 10f;

        var renderer = GetComponentInChildren<SpriteRenderer>();
        renderer.sprite = this.GetSprite("BagOMarbles_World.png", 75f);

        audSplat = this.GetSoundNoSub("BagOMarbles_Splat.wav", SoundType.Effect);
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

    internal override void VirtualUpdate()
    {
        base.VirtualUpdate();
        if (time <= 0f && slipping)
            End();
    }
}