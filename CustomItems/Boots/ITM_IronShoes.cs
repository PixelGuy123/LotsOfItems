using System.Collections;
using LotsOfItems.ItemPrefabStructures;
using UnityEngine;

namespace LotsOfItems.CustomItems.Boots;

public class ITM_IronShoes : ITM_Boots, IItemPrefab
{

    public void SetupPrefab(ItemObject itm)
    {
        gaugeSprite = itm.itemSpriteSmall;
    }
    public void SetupPrefabPost() { }

    readonly EntityOverrider overrider = new();
    public override bool Use(PlayerManager pm)
    {
        if (!pm.plm.Entity.Override(overrider))
        {
            Destroy(gameObject);
            return false;
        }

        this.pm = pm;
        gauge = Singleton<CoreGameManager>.Instance.GetHud(pm.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, setTime);
        StartCoroutine(EffectTimer());
        return true;
    }

    private IEnumerator EffectTimer()
    {
        StandardDoor[] doors = FindObjectsOfType<StandardDoor>();
        // Ignore collision with doors and NPCs
        foreach (StandardDoor door in doors) // get every single door lazily like that
        {
            Physics.IgnoreCollision(pm.plm.Entity.collider, door.colliders[0], true);
            Physics.IgnoreCollision(pm.plm.Entity.collider, door.colliders[1], true);
        }

        float timer = setTime;
        while (timer > 0f)
        {
            overrider.SetHeight(0.5f);
            for (int i = 0; i < pm.ec.Npcs.Count; i++)
            {
                if (!pm.ec.Npcs[i].Navigator.Entity.IsIgnoring(pm.plm.Entity)) // To make sure recently spawned npcs are included too
                    pm.ec.Npcs[i].Navigator.Entity.IgnoreEntity(pm.plm.Entity, true);
            }
            timer -= Time.deltaTime * pm.PlayerTimeScale;
            gauge.SetValue(setTime, timer);
            yield return null;
        }

        CleanUp(doors);
    }

    private void CleanUp(StandardDoor[] doorArray)
    {
        foreach (StandardDoor door in doorArray)
        {
            if (door != null)
            {
                Physics.IgnoreCollision(pm.plm.Entity.collider, door.colliders[0], false);
                Physics.IgnoreCollision(pm.plm.Entity.collider, door.colliders[1], false);
            }
        }
        for (int i = 0; i < pm.ec.Npcs.Count; i++)
        {
            if (pm.ec.Npcs[i].Navigator.Entity.IsIgnoring(pm.plm.Entity))
                pm.ec.Npcs[i].Navigator.Entity.IgnoreEntity(pm.plm.Entity, false);
        }

        overrider.Release();
        gauge?.Deactivate();

        Destroy(gameObject);
    }
}