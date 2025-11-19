using System.Collections;
using LotsOfItems.Plugin;
using MTM101BaldAPI;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.NanaPeels;

public class ITM_GlassNanaPeel : ITM_GenericNanaPeel
{
    private bool isRiding = false;
    private float rideTime = 0f;
    [SerializeField]
    private float startSpeed = 15f;
    [SerializeField]
    private float acceleration = 5f;
    [SerializeField]
    private float stunMoveModMultiplier = 0.45f;
    [SerializeField]
    int noiseVal = 89;
    [SerializeField]
    SoundObject audShatter;
    [SerializeField]
    float rayCastHitDistance = 3.5f;
    [SerializeField]
    private LayerMask collisionLayer = LayerStorage.gumCollisionMask;
    [SerializeField]
    SpriteRenderer renderer;
    private MovementModifier rideForce;

    bool shouldDisableUpdate = false, shattered = false;

    internal override bool DisableUpdate => shouldDisableUpdate;

    public override bool Use(PlayerManager pm)
    {
        this.pm = pm;
        pm.onPlayerTeleport += OnPlayerTeleport;
        return base.Use(pm);
    }

    void OnPlayerTeleport(PlayerManager player, Vector3 pos, Vector3 positionDelta) =>
        VirtualEnd();


    protected override void VirtualSetupPrefab(ItemObject itm)
    {
        base.VirtualSetupPrefab(itm);
        renderer = GetComponentInChildren<SpriteRenderer>();
        renderer.sprite = itm.itemSpriteLarge.DuplicateItself(renderer.sprite.pixelsPerUnit);
        audShatter = GenericExtensions.FindResourceObjectByName<SoundObject>("GlassBreak");
        endHeight = 1.1f;
    }

    internal override bool EntityTriggerStayOverride(Collider other, bool validCollision)
    {
        if (shattered) return false;

        if (other.isTrigger && validCollision && pm.plm.Entity.Grounded && pm.plm.Entity.Velocity.magnitude > 0f && other.gameObject == pm.gameObject && !isRiding && ready)
        {
            isRiding = true;
            ready = false;
            slipping = true;
            audioManager.FlushQueue(true);
            audioManager.QueueAudio(audSlipping);
            audioManager.SetLoop(true);
            StartCoroutine(SlideRoutine());
            shouldDisableUpdate = true; // Disables update, so that it doesn't override the nana's behavior
        }
        return false;
    }

    internal override void VirtualUpdate()
    {
        base.VirtualUpdate();
        if (!pm) // There should be never a glass 'nana peel existing without a Player owner
        {
            Destroy(gameObject);
            return;
        }

        // Raycast wall check
        if (isRiding && Physics.Raycast(pm.transform.position, direction, out var wallHit, rayCastHitDistance, collisionLayer, QueryTriggerInteraction.Collide) && wallHit.transform.CompareTag("Wall"))
        {
            VirtualEnd();
        }
    }

    internal override bool OnCollisionOverride(RaycastHit hit) => false;

    internal override bool EntityTriggerExitOverride(Collider other, bool validCollision) => false; // No work for here

    private IEnumerator SlideRoutine()
    {
        float currentSpeed = startSpeed;
        Vector3 mouseDir = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward;
        rideForce = new(mouseDir * currentSpeed, 0f);
        pm.Am.moveMods.Add(rideForce);

        while (isRiding)
        {
            entity.Teleport(pm.transform.position);
            mouseDir = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward;
            rideForce.movementAddend = mouseDir * currentSpeed;

            rideTime += Time.deltaTime * ec.EnvironmentTimeScale;
            currentSpeed += Time.deltaTime * ec.EnvironmentTimeScale * acceleration;
            yield return null;
        }
    }

    internal override bool VirtualEnd()
    {
        shattered = true;
        isRiding = false;

        pm.Am.moveMods.Remove(rideForce);

        pm.StartCoroutine(StunPlayer(pm));
        ec.MakeNoise(pm.transform.position, noiseVal);

        audioManager.FlushQueue(true);
        audioManager.PlaySingle(audShatter);

        StartCoroutine(WaitToDie());
        renderer.enabled = false;

        pm.onPlayerTeleport -= OnPlayerTeleport;

        return false;
    }

    IEnumerator WaitToDie()
    {
        while (audioManager.AnyAudioIsPlaying) yield return null;

        Destroy(gameObject);
    }

    IEnumerator StunPlayer(PlayerManager pm)
    {
        MovementModifier moveMod = new(Vector3.zero, stunMoveModMultiplier);
        pm.Am.moveMods.Add(moveMod);
        yield return new WaitForSecondsEnvironmentTimescale(pm.ec, rideTime * 0.5f);
        pm.Am.moveMods.Remove(moveMod);
    }
}