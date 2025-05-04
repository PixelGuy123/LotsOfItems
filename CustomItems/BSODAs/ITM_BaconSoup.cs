using System.Collections;
using System.Collections.Generic;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using MTM101BaldAPI;
using UnityEngine;

namespace LotsOfItems.CustomItems.BSODAs;
public class ITM_BaconSoup : ITM_GenericBSODA
{
    [SerializeField]
    private float slowDuration = 25f;

    private readonly HashSet<Entity> inkedEntities = [];

    [SerializeField]
    private MovementModifier slowMod = new(Vector3.zero, 0.7f);

    protected override void VirtualSetupPrefab(ItemObject itm)
    {
        base.VirtualSetupPrefab(itm);
        speed *= 1.25f;
        spriteRenderer.sprite = this.GetSprite("BaconSoup_spray.png", spriteRenderer.sprite.pixelsPerUnit);
        sound = this.GetSoundNoSub("BaconSoup_Gulp.wav", SoundType.Effect);
        this.DestroyParticleIfItHasOne();
    }

    public override bool Use(PlayerManager pm)
    {
        pm.plm.stamina = pm.plm.staminaMax;
        return base.Use(pm);
    }

    public override bool VirtualEntityTriggerEnter(Collider other)
    {
        if (other.isTrigger && !hasEnded)
        {
            Entity target = other.GetComponent<Entity>();
            if (target != null && !inkedEntities.Contains(target))
            {
                target.ExternalActivity.moveMods.Add(slowMod);
                StartCoroutine(RemoveSlowAfterDelay(target));
                inkedEntities.Add(target);
            }
        }
        return true;
    }

    private IEnumerator RemoveSlowAfterDelay(Entity target)
    {
        yield return new WaitForSecondsEnvironmentTimescale(ec, slowDuration);
        if (target)
        {
            target.ExternalActivity.moveMods.Remove(slowMod);
            inkedEntities.Remove(target);
        }
        else
            inkedEntities.RemoveWhere(x => !x);
    }

    protected override void VirtualEnd()
    {
        hasEnded = true;
        foreach (ActivityModifier activityMod in activityMods)
            activityMod.moveMods.Remove(moveMod);
        StartCoroutine(WaitToDie());
    }

    IEnumerator WaitToDie()
    {
        while (inkedEntities.Count != 0)
            yield return null;
        Destroy(gameObject);
    }
}
