using System.Collections;
using System.Collections.Generic;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using MTM101BaldAPI;
using UnityEngine;

namespace LotsOfItems.CustomItems.BSODAs;

public class ITM_AcidSoda : ITM_GenericBSODA
{
    private readonly HashSet<NPC> hitNpcs = [];

    [SerializeField]
    internal float blindTime = 10f;

    protected override void VirtualSetupPrefab(ItemObject itm)
    {
        base.VirtualSetupPrefab(itm);
        spriteRenderer.sprite = this.GetSprite("AcidSoda_Spray.png", spriteRenderer.sprite.pixelsPerUnit);
        this.DestroyParticleIfItHasOne();
    }

    public override bool VirtualEntityTriggerEnter(Collider other)
    {
        if (other.isTrigger && other.CompareTag("NPC"))
        {
            NPC npc = other.GetComponent<NPC>();
            if (npc != null && !hitNpcs.Contains(npc))
            {
                hitNpcs.Add(npc);
                StartCoroutine(BlindNpc(npc, blindTime));
            }
        }
        return true;
    }

    private IEnumerator BlindNpc(NPC npc, float duration)
    {
        npc.Navigator.Entity.SetBlinded(true);
        yield return new WaitForSecondsEnvironmentTimescale(npc.ec, duration);
        if (npc)
        {
            npc.Navigator.Entity.SetBlinded(false);
            hitNpcs.Remove(npc);
        }
    }

    private void OnDestroy()
    {
        foreach (var npc in hitNpcs)
        {
            npc?.Navigator.Entity.SetBlinded(false);
        }
    }
}
