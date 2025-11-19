using System.Collections;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using UnityEngine;

namespace LotsOfItems.CustomItems.BSODAs;

public class ITM_YSODA : ITM_GenericBSODA
{
    [SerializeField]
    private float freezeTime = 15f;

    protected override void VirtualSetupPrefab(ItemObject itm)
    {
        base.VirtualSetupPrefab(itm);
        spriteRenderer.sprite = this.GetSprite("YSODA_spray.png", spriteRenderer.sprite.pixelsPerUnit);
        this.DestroyParticleIfItHasOne();
    }

    public override bool VirtualEntityTriggerEnter(Collider other, bool validCollision)
    {
        if (!other.isTrigger || !validCollision) return true;
        bool isNpc = other.CompareTag("NPC");
        bool isPlayer = other.CompareTag("Player");

        if (!isNpc && !isPlayer || (isPlayer && launching)) return true;

        Entity target = other.GetComponent<Entity>();

        SpriteRenderer renderer = null;
        if (isNpc && other.TryGetComponent<NPC>(out var npc))
            renderer = npc.spriteRenderer[0];

        target?.StartCoroutine(Petrify(renderer, target));
        return true; // Pass through
    }

    IEnumerator Petrify(SpriteRenderer npcRenderer, Entity entity)
    {
        MovementModifier moveMod = new(Vector3.zero, 0f);
        entity.ExternalActivity.moveMods.Add(moveMod);

        if (npcRenderer)
            npcRenderer.color = Color.gray;

        float timer = freezeTime;
        while (timer > 0f)
        {
            for (int i = 0; i < Entity.allEntities.Count; i++)
            {
                var e = Entity.allEntities[i];
                if (e != entity && !entity.IsIgnoring(e))
                    entity.IgnoreEntity(e, true);
            }

            timer -= Time.deltaTime * ec.EnvironmentTimeScale;
            yield return null;
        }

        if (npcRenderer) npcRenderer.color = Color.white;
        entity.ExternalActivity.moveMods.Remove(moveMod);

        for (int i = 0; i < Entity.allEntities.Count; i++)
        {
            var e = Entity.allEntities[i];
            if (e != entity && entity.IsIgnoring(e)) // To make sure recently spawned npcs are included too
                entity.IgnoreEntity(e, false);
        }
    }
}
