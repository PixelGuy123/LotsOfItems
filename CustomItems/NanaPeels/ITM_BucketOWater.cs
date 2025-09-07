using LotsOfItems.Plugin;
using UnityEngine;

namespace LotsOfItems.CustomItems.NanaPeels;

public class ITM_BucketOWater : ITM_GenericNanaPeel
{
    [SerializeField]
    [Range(0f, 1f)]
    internal float negateDirectionChance = 0.5f;
    protected override void VirtualSetupPrefab(ItemObject itm)
    {
        base.VirtualSetupPrefab(itm);
        var renderer = GetComponentInChildren<SpriteRenderer>();
        renderer.sprite = itm.itemSpriteLarge.DuplicateItself(renderer.sprite.pixelsPerUnit);
        renderer.sprite.name = $"{itm.itemSpriteLarge.name}_World";
    }

    internal override bool EntityTriggerStayOverride(Collider other)
    {
        Entity component = other.GetComponent<Entity>();
        if (component != null && component.Grounded && component.Velocity.magnitude > 0f)
        {
            entity.Teleport(component.transform.position);
            component.ExternalActivity.moveMods.Add(moveMod);
            slippingEntity = component;
            slipping = true;
            ready = false;
            entity.ExternalActivity.ignoreFrictionForce = true;

            direction = component.Velocity.normalized;
            if (Random.value <= negateDirectionChance)
            {
                direction = -direction; // 50% chance to go backwards
            }

            audioManager.FlushQueue(endCurrent: true);
            audioManager.QueueAudio(audSlipping);
            audioManager.SetLoop(val: true);
            if (!force.Dead)
            {
                entity.RemoveForce(force);
            }
        }
        return false;
    }
}