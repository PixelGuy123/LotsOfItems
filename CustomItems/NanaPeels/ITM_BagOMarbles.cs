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
        endHeight = 0.2f;
        startHeight = 1f;
        throwSpeed = 10f;
        gravity = 10f;

        var renderer = GetComponentInChildren<SpriteRenderer>();
        renderer.sprite = this.GetSprite("BagOMarble_World.png", 25f);

        audSplat = this.GetSoundNoSub("BagOMarble_Splat.wav", SoundType.Effect);
    }

    internal override void VirtualUpdate()
    {
        base.VirtualUpdate();
        if (time <= 0f && slipping)
            End();
    }
}