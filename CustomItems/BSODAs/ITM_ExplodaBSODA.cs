using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using UnityEngine;

namespace LotsOfItems.CustomItems.BSODAs;

public class ITM_ExplodaBSODA : ITM_GenericBSODA
{
    [SerializeField]
    private float explosionRadius = 25f, explosionForce = 70f;
    [SerializeField]
    private QuickExplosion explosion;

    protected override void VirtualSetupPrefab(ItemObject itm)
    {
        base.VirtualSetupPrefab(itm);
        explosion = LotOfItemsPlugin.assetMan.Get<QuickExplosion>("genericExplosionPrefab");
        spriteRenderer.sprite = this.GetSprite("ExplodaBSODA_Spray.png", spriteRenderer.sprite.pixelsPerUnit);
        this.DestroyParticleIfItHasOne();
    }

    public override bool Use(PlayerManager pm)
    {
        entity.OnEntityMoveInitialCollision += (_) => Explode();
        return base.Use(pm);
    }

    public override bool VirtualEntityTriggerEnter(Collider other, bool validCollision)
    {
        if (validCollision && other.isTrigger && (other.CompareTag("NPC") || other.CompareTag("Player")))
        {
            Explode();
            return false;
        }
        return true;
    }

    private void Explode()
    {
        if (hasEnded) return;

        hasEnded = true;
        Instantiate(explosion, transform.position, Quaternion.identity);
        ItemExtensions.Explode(this, explosionRadius, LotOfItemsPlugin.onlyNpcPlayerLayers, explosionForce, -explosionForce * 0.5f);
        Destroy(gameObject);
    }
}