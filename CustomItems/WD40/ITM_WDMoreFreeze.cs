using System.Collections.Generic;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using PixelInternalAPI.Classes;
using UnityEngine;

namespace LotsOfItems.CustomItems.WD40;

public class ITM_WDMoreFreeze : ITM_NoSquee, IItemPrefab // Inherits NoSquee to reuse its particles
{
    public void SetupPrefab(ItemObject itm)
    {
        sparkleParticlesPre = sparkleParticlesPre.SafeDuplicatePrefab(true);
        sparkleParticlesPre.name = "FreezingSparkles";
        sparkleParticlesPre.gameObject.AddComponent<WDMoreFreezeEffect>();
        var sparkleCollider = sparkleParticlesPre.gameObject.AddComponent<BoxCollider>();
        sparkleCollider.center = Vector3.up * 5f;
        sparkleCollider.size = Vector3.one * 9.9f;
        sparkleCollider.isTrigger = true;
        sparkleCollider.gameObject.layer = LayerStorage.ignoreRaycast;

        var renderer = sparkleParticlesPre.GetComponentInChildren<ParticleSystemRenderer>();
        var newMat = new Material(renderer.material)
        {
            name = "FreezingSparkle",
            mainTexture = this.GetTexture("WDMoreFreeze_Sparkle.png")
        };
        renderer.material = newMat;

        distance = 3;
    }
    public void SetupPrefabPost() { }
}

public class WDMoreFreezeEffect : MonoBehaviour
{
    readonly List<Entity> affectedEntities = [];
    public MovementModifier moveMod = new(Vector3.zero, 0.5f);
    void OnTriggerEnter(Collider other)
    {
        Entity entity = other.GetComponent<Entity>();
        if (entity != null && !affectedEntities.Contains(entity))
        {
            entity.ExternalActivity.moveMods.Add(moveMod);
            affectedEntities.Add(entity);
        }
    }

    void OnTriggerExit(Collider other)
    {
        Entity entity = other.GetComponent<Entity>();
        if (entity != null && affectedEntities.Contains(entity))
        {
            entity.ExternalActivity.moveMods.Remove(moveMod);
            affectedEntities.Remove(entity);
        }
    }

    private void OnDestroy()
    {
        foreach (var entity in affectedEntities)
            entity?.ExternalActivity.moveMods.Remove(moveMod);
    }
}