using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using UnityEngine;

namespace LotsOfItems.CustomItems.BSODAs;

public class ITM_PlasticSoda : ITM_GenericBSODA
{
    [SerializeField]
    private int hits = 3;

    [SerializeField]
    private Sprite[] damageSprites;

    protected override void VirtualSetupPrefab(ItemObject itm)
    {
        base.VirtualSetupPrefab(itm);
        time = 25f;
        damageSprites = this.GetSpriteSheet("PlasticSoda_Spray.png", 3, 1, spriteRenderer.sprite.pixelsPerUnit);
        spriteRenderer.sprite = damageSprites[0];
        this.DestroyParticleIfItHasOne();
    }

    public override bool VirtualEntityTriggerEnter(Collider other, bool validCollision)
    {
        if (validCollision && other.isTrigger && (other.CompareTag("Player") || other.CompareTag("NPC")))
        {
            if (--hits <= 0)
            {
                VirtualEnd();
                return false;
            }
            else
                spriteRenderer.sprite = damageSprites[damageSprites.Length - hits]; // Will never throw an exception here, since hit = 0 ends the soda
        }
        return base.VirtualEntityTriggerEnter(other, validCollision);
    }
}
