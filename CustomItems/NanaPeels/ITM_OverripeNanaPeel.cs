using System.Collections;
using LotsOfItems.Plugin;
using UnityEngine;

namespace LotsOfItems.CustomItems.NanaPeels;

public class ITM_OverripeNanaPeel : ITM_GenericNanaPeel
{
    [SerializeField]
    internal float hitTouchForce = 85f;
    protected override void VirtualSetupPrefab(ItemObject itm)
    {
        base.VirtualSetupPrefab(itm);
        var renderer = GetComponentInChildren<SpriteRenderer>();
        renderer.sprite = itm.itemSpriteLarge.DuplicateItself(renderer.sprite.pixelsPerUnit);
        endHeight = 1.1f;
    }
    internal override bool EntityTriggerStayOverride(Collider other, bool validCollision)
    {
        if (!validCollision || !ready || slipping)
            return false;

        if (other.isTrigger && (other.CompareTag("NPC") || other.CompareTag("Player")) && other.TryGetComponent<Entity>(out var e) && e.Grounded && e.Velocity.magnitude > 0f)
        {
            // Add force to both the Entity and 'Nana Peel
            entity.AddForce(new(e.Velocity.normalized, hitTouchForce, -hitTouchForce * 0.35f));
            e.AddForce(new(e.Velocity.normalized, hitTouchForce, -hitTouchForce * 0.35f)); // A very strong, single push
            audioManager.PlaySingle(audEnd);
            slipping = true;
            StartCoroutine(WaitToDestroy());
        }

        return false;
    }

    internal override bool EntityTriggerExitOverride(Collider other, bool validCollision) => false;
    internal override bool DisableUpdate => ready; // If it is ready, we don't need this active anymore
    internal override bool OnCollisionOverride(RaycastHit hit) => false;

    internal override bool VirtualEnd() { Destroy(gameObject); return false; }

    IEnumerator WaitToDestroy()
    {
        while (audioManager.AnyAudioIsPlaying) yield return null;
        VirtualEnd();
    }
}