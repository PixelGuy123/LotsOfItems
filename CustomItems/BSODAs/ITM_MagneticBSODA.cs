using System.Collections.Generic;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using UnityEngine;

namespace LotsOfItems.CustomItems.BSODAs;

public class ITM_MagneticBSODA : ITM_GenericBSODA
{
    [SerializeField]
    private float pullRadius = 50f; // 5 tiles
    [SerializeField]
    private float pullForce = 40f;

    private readonly List<NPC> affectedNpcs = [];
    private readonly Dictionary<NPC, MovementModifier> pullModifiers = [];
    private DijkstraMap attractionMap;

    protected override void VirtualSetupPrefab(ItemObject itm)
    {
        base.VirtualSetupPrefab(itm);
        spriteRenderer.sprite = this.GetSprite("MSODA_Spray.png", spriteRenderer.sprite.pixelsPerUnit);
        this.DestroyParticleIfItHasOne();
    }

    public override bool Use(PlayerManager pm)
    {
        bool result = base.Use(pm);
        if (result)
        {
            attractionMap = new DijkstraMap(ec, PathType.Const, Mathf.RoundToInt(pullRadius / 10f), transform);
            attractionMap.Activate();
        }
        return result;
    }

    public override void VirtualUpdate()
    {
        base.VirtualUpdate();
        if (attractionMap == null || hasEnded) return;

        if (!attractionMap.PendingUpdate)
            attractionMap.QueueUpdate();

        // Remove modifiers from NPCs that are out of range or inactive
        for (int i = affectedNpcs.Count - 1; i >= 0; i--)
        {
            NPC npc = affectedNpcs[i];
            IntVector2 npcAlignedPos = IntVector2.GetGridPosition(npc.transform.position);
            if (npc == null || !npc.Navigator.isActiveAndEnabled || attractionMap.Value(npcAlignedPos) > attractionMap.maxDistance)
            {
                if (pullModifiers.ContainsKey(npc))
                {
                    npc.Navigator.Am.moveMods.Remove(pullModifiers[npc]);
                    pullModifiers.Remove(npc);
                }
                affectedNpcs.RemoveAt(i);
            }
        }

        // Add modifiers to new NPCs in range
        foreach (NPC npc in ec.Npcs)
        {
            IntVector2 npcAlignedPos = IntVector2.GetGridPosition(npc.transform.position);
            if (npc.Navigator.isActiveAndEnabled && !affectedNpcs.Contains(npc) && attractionMap.Value(npcAlignedPos) <= attractionMap.maxDistance)
            {
                var mod = new MovementModifier(Vector3.zero, 1f, 5);
                npc.Navigator.Am.moveMods.Add(mod);
                affectedNpcs.Add(npc);
                pullModifiers[npc] = mod;
            }
        }

        // Update pull force for all affected NPCs
        foreach (NPC npc in affectedNpcs)
        {
            if (pullModifiers.TryGetValue(npc, out var mod))
            {
                mod.movementAddend = (transform.position - npc.transform.position).normalized * pullForce;
            }
        }
    }

    protected override void VirtualEnd()
    {
        foreach (var pair in pullModifiers)
        {
            pair.Key?.Navigator.Am.moveMods.Remove(pair.Value);

        }
        pullModifiers.Clear();
        affectedNpcs.Clear();
        attractionMap?.Deactivate();

        base.VirtualEnd();
    }
}