using LotsOfItems.Plugin;
using UnityEngine;

namespace LotsOfItems.CustomItems.NanaPeels;

public class ITM_ChocolateNanaPeel : ITM_GenericNanaPeel
{
    private bool isSliding = false;

    protected override void VirtualSetupPrefab(ItemObject itm)
    {
        base.VirtualSetupPrefab(itm);
        var renderer = GetComponentInChildren<SpriteRenderer>();
        renderer.sprite = itm.itemSpriteLarge.DuplicateItself(renderer.sprite.pixelsPerUnit);
        renderer.sprite.name = $"{itm.itemSpriteLarge.name}_World";
    }

    internal override void OnFloorHit()
    {
        isSliding = true;
        audioManager.FlushQueue(true);
        audioManager.QueueAudio(audSlipping); // Already slips by itself
        audioManager.SetLoop(true);
    }

    internal override void VirtualUpdate()
    {
        base.VirtualUpdate();

        if (isSliding)
        {
            entity.UpdateInternalMovement(transform.forward * speed * ec.EnvironmentTimeScale);
        }
        else if (slipping)
        {
            transform.position = slippingEntity.transform.position;
        }
    }

    internal override bool EntityTriggerStayOverride(Collider other)
    {
        if (!ready || slipping) return false;

        Entity component = other.GetComponent<Entity>();
        if (component != null && component.Grounded)
        {
            isSliding = false;
            slippingEntity = component;
            slipping = true;
            ready = false;
            direction = transform.forward;
            moveMod.movementAddend = direction * speed;
            component.ExternalActivity.moveMods.Add(moveMod);
        }
        return false;
    }
}