using System.Collections;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using MTM101BaldAPI;
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
    private float acceleration = 0.05f;
    [SerializeField]
    private float stunMoveModMultiplier = 0.45f;
    [SerializeField]
    int noiseVal = 89;
    [SerializeField]
    SoundObject audShatter;
    private Force rideForce;

    protected override void VirtualSetupPrefab(ItemObject itm)
    {
        base.VirtualSetupPrefab(itm);
        var renderer = GetComponentInChildren<SpriteRenderer>();
        renderer.sprite = itm.itemSpriteLarge.DuplicateItself(renderer.sprite.pixelsPerUnit);
        audShatter = GenericExtensions.FindResourceObjectByName<SoundObject>("GlassBreak");
    }

    internal override bool EntityTriggerStayOverride(Collider other, bool validCollision)
    {
        if (other.isTrigger && validCollision && other.gameObject == pm.gameObject && !isRiding && ready)
        {
            isRiding = true;
            ready = false;
            StartCoroutine(SlideRoutine());
        }
        return false;
    }

    private IEnumerator SlideRoutine()
    {
        pm.plm.Entity.OnEntityMoveInitialCollision += Shatter;
        float currentSpeed = startSpeed;
        Vector3 mouseDir = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward;
        rideForce = new(mouseDir, currentSpeed, acceleration);

        while (isRiding)
        {
            mouseDir = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward;
            rideForce.direction = new(mouseDir.x, mouseDir.z);
            pm.plm.Entity.AddForce(rideForce);

            rideTime += Time.deltaTime * ec.EnvironmentTimeScale;
            yield return null;
        }
        rideForce.Kill();
    }

    internal override bool VirtualEnd()
    {
        rideForce?.Kill();

        pm.StartCoroutine(StunPlayer(pm));
        ec.MakeNoise(pm.transform.position, noiseVal);
        audioManager.PlaySingle(audShatter);
        return true;
    }

    private void Shatter(RaycastHit hit)
    {
        if (!isRiding) return;

        isRiding = false;
        pm.plm.Entity.OnEntityMoveInitialCollision -= Shatter;
    }

    IEnumerator StunPlayer(PlayerManager pm)
    {
        MovementModifier moveMod = new(Vector3.zero, stunMoveModMultiplier);
        pm.Am.moveMods.Add(moveMod);
        yield return new WaitForSecondsEnvironmentTimescale(pm.ec, rideTime * 0.5f);
        pm.Am.moveMods.Remove(moveMod);
    }
}