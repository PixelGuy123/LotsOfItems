using System.Collections;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using MTM101BaldAPI;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.BSODAs;

public class ITM_SnowGun : ITM_GenericBSODA
{
    public float gravity = 0.1f;
    public float slowDuration = 10f, minVerticalSpeed = 0.05f, maxVerticalSpeed = 0.15f;
    public float slowMultiplier = 0.75f;
    private float verticalSpeed = 0f;
    float verticalLimit = 9.5f, verticalHeight;
    private MovementModifier slowMod;
    public AudioManager audMan;
    [SerializeField]
    private SoundObject audHit;
    [SerializeField]
    internal ItemObject nextItem = null;

    protected override void VirtualSetupPrefab(ItemObject itm)
    {
        spriteRenderer.sprite = this.GetSprite("SnowGun_Snowball.png", 35f);
        sound = this.GetSoundNoSub("SnowGun_Shoot.wav", SoundType.Effect);
        this.DestroyParticleIfItHasOne();

        audMan = gameObject.CreatePropagatedAudioManager(45f, 85f);
        audHit = this.GetSoundNoSub("SnowGun_Hit.wav", SoundType.Effect);

        speed = 40f;
        time = 15f;
        entity.collisionLayerMask = LayerStorage.gumCollisionMask;

        useRandomRotation = false;
    }

    public override bool Use(PlayerManager pm)
    {
        bool flag = base.Use(pm);

        slowMod = new(Vector3.zero, slowMultiplier);
        verticalSpeed = Random.Range(minVerticalSpeed, maxVerticalSpeed);
        verticalHeight = pm.plm.Entity.BaseHeight;
        entity.OnEntityMoveInitialCollision += (hit) =>
        {
            if (!hasEnded) VirtualEnd();
        };

        if (flag && nextItem)
        {
            pm.itm.SetItem(nextItem, pm.itm.selectedItem);
            return false;
        }
        return flag;
    }

    public override void VirtualUpdate()
    {
        verticalSpeed -= gravity * Time.deltaTime * ec.EnvironmentTimeScale;
        verticalHeight += verticalSpeed;
        if (verticalHeight > verticalLimit)
        {
            verticalHeight = verticalLimit;
            verticalSpeed = 0f;
        }

        entity.SetHeight(verticalHeight);
        if (verticalHeight <= 0f)
        {
            Destroy(gameObject); // Directly kills the BSODA instance, to avoid the hit sound thing 
            return;
        }
        base.VirtualUpdate();
    }

    public override bool VirtualEntityTriggerEnter(Collider other, bool validCollision)
    {
        if (validCollision && other.isTrigger && other.CompareTag("NPC"))
        {
            NPC npc = other.GetComponent<NPC>();
            if (npc && npc.Navigator.isActiveAndEnabled)
            {
                npc.Navigator.Am.moveMods.Add(slowMod);
                npc.Navigator.Entity.AddForce(new(transform.forward, speed, -speed * 0.5f));
                npc.StartCoroutine(RemoveSlow(npc.Navigator.Entity));
                VirtualEnd();
                return false;
            }
        }
        return false;
    }

    public override bool VirtualEntityTriggerExit(Collider other, bool validCollision) => false; // Prevents any action with this

    protected override void VirtualEnd()
    {
        hasEnded = true;
        foreach (ActivityModifier activityMod in activityMods)
            activityMod.moveMods.Remove(moveMod);
        entity.SetFrozen(true);
        entity.SetVisible(false);
        entity.SetInteractionState(false);
        StartCoroutine(PlaySoundThenDies(audHit));
    }

    private IEnumerator RemoveSlow(Entity entity)
    {
        yield return new WaitForSecondsEnvironmentTimescale(ec, slowDuration);
        entity?.ExternalActivity.moveMods.Remove(slowMod);
    }

    IEnumerator PlaySoundThenDies(SoundObject sound)
    {
        audMan.PlaySingle(sound);
        while (audMan.AnyAudioIsPlaying) yield return null;
        Destroy(gameObject);
    }
}