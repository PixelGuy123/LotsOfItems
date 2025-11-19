using LotsOfItems.Plugin;
using UnityEngine;

namespace LotsOfItems.CustomItems.NanaPeels;

public class ITM_SadPeel : ITM_GenericNanaPeel
{
    [SerializeField]
    private Components.MomentumNavigator momentumNav;
    private bool hunting = false;
    private Entity target;

    protected override void VirtualSetupPrefab(ItemObject itm)
    {
        base.VirtualSetupPrefab(itm);
        momentumNav = gameObject.AddComponent<Components.MomentumNavigator>();
        momentumNav.maxSpeed = 25f; // Fast sliding
        momentumNav.autoRotate = false;
        momentumNav.useAcceleration = false;

        // Visuals
        var rend = GetComponentInChildren<SpriteRenderer>();
        rend.sprite = itm.itemSpriteLarge.DuplicateItself(rend.sprite.pixelsPerUnit);
        endHeight = 1.1f;
    }

    internal override void OnFloorHit()
    {
        base.OnFloorHit();
        momentumNav.Initialize(ec);
        audioManager.FlushQueue(true);
        audioManager.QueueAudio(audSlipping);
        audioManager.SetLoop(true);

        FindTarget();
    }

    void FindTarget()
    {
        float dist = float.MaxValue;
        foreach (var npc in ec.Npcs)
        {
            if (!npc.Navigator.isActiveAndEnabled || !npc.Navigator.Entity.TotallyActive) continue;
            float d = Vector3.Distance(transform.position, npc.transform.position);
            if (d < dist)
            {
                dist = d;
                target = npc.Navigator.Entity;
            }
        }

        if (target == null)
            slipping = true; // Stops this ready thing
        else
            hunting = true;
    }


    internal override void VirtualUpdate()
    {
        base.VirtualUpdate();
        if (hunting)
        {
            entity.UpdateInternalMovement(Vector3.zero);
            if (target != null)
            {
                momentumNav.FindPath(target.transform.position);
            }
            else
                FindTarget();
            if (slipping) // If ready is enabled, then we don't have to hunt anymore
            {
                hunting = false;
                momentumNav.enabled = false;
                direction = momentumNav.Velocity.normalized;
                if (direction == Vector3.zero) direction = transform.forward;
            }
        }
    }

    internal override bool EntityTriggerStayOverride(Collider other, bool validCollision) // ITM_Nanapeel, but it doesn't check for velocity
    {
        if (!validCollision || !ready || slipping || (!other.CompareTag("Player") && !other.CompareTag("NPC")))
        {
            return false;
        }

        Entity component = other.GetComponent<Entity>();
        if (component != null && component.Grounded)
        {
            entity.Teleport(((Component)component).transform.position);
            component.ExternalActivity.moveMods.Add(moveMod);
            slippingEntity = component;
            slipping = true;
            ready = false;
            entity.ExternalActivity.ignoreFrictionForce = true;
            direction = momentumNav.Velocity.normalized;
            audioManager.FlushQueue(true);
            audioManager.QueueAudio(audSlipping);
            audioManager.SetLoop(true);
            if (!force.Dead)
            {
                entity.RemoveForce(force);
            }
        }
        return false;
    }

    internal override bool DisableUpdate => hunting;
}