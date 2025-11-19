using System.Collections;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using MTM101BaldAPI.Registers;
using PixelInternalAPI.Classes;
using UnityEngine;

namespace LotsOfItems.CustomItems.GrapplingHooks;

public class ITM_CharginHook : Item, IItemPrefab
{
    private bool isCharging = false;
    [SerializeField]
    internal float chargeDuration = 2f;
    [SerializeField]
    internal float chargeSpeed = 120f, rayCastHitDistance = 3.5f;
    [SerializeField]
    internal int noiseVal = 64;
    [SerializeField]
    private LayerMask collisionLayer = LayerStorage.gumCollisionMask;
    [SerializeField]
    internal SoundObject audUse, audBump;
    [SerializeField]
    internal Sprite gaugeSprite;
    Force chargeForce;
    Vector3 direction;
    EnvironmentController ec;
    HudGauge gauge;

    public void SetupPrefab(ItemObject itm)
    {
        audUse = (ItemMetaStorage.Instance.FindByEnum(Items.GrapplingHook).value.item as ITM_GrapplingHook).audLaunch;
        audBump = LotOfItemsPlugin.assetMan.Get<SoundObject>("audBump");
        gaugeSprite = itm.itemSpriteSmall;
    }
    public void SetupPrefabPost() { }

    public override bool Use(PlayerManager pm)
    {
        this.pm = pm;
        ec = pm.ec;

        isCharging = true;
        direction = pm.transform.forward;
        chargeForce = new Force(direction, chargeSpeed, 0f);
        pm.plm.Entity.AddForce(chargeForce);

        gauge = Singleton<CoreGameManager>.Instance.GetHud(pm.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, chargeDuration);

        StartCoroutine(ChargeTimer());

        Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audUse);

        return true;
    }

    void Update()
    {
        if (!isCharging) return;

        // Make npcs constantly ignore the player
        for (int i = 0; i < pm.ec.Npcs.Count; i++) // Inside the while loop to make sure recently spawned npcs are included as well
        {
            if (!pm.ec.Npcs[i].Navigator.Entity.IsIgnoring(pm.plm.Entity))
                pm.ec.Npcs[i].Navigator.Entity.IgnoreEntity(pm.plm.Entity, true);
        }

        // Manual collision check for NPCs
        Collider[] hits = Physics.OverlapSphere(pm.transform.position, 2f, LayerMask.GetMask("NPCs"));
        foreach (var hit in hits)
        {
            if (hit.isTrigger && hit.TryGetComponent<Entity>(out var npcEntity))
            {
                npcEntity.AddForce(new Force(pm.transform.forward, chargeSpeed, -chargeSpeed * 0.85f));
                Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audBump);
                ec.MakeNoise(pm.transform.position, noiseVal, true);
                EndCharge();
                return;
            }
        }

        // Raycast wall check
        if (Physics.Raycast(pm.transform.position, direction, out var wallHit, rayCastHitDistance, collisionLayer, QueryTriggerInteraction.Collide) && wallHit.transform.CompareTag("Wall"))
        {
            Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audBump);
            ec.MakeNoise(pm.transform.position, noiseVal, true);
            EndCharge();
        }
    }

    private IEnumerator ChargeTimer()
    {
        float t = chargeDuration;
        while (t > 0f)
        {
            t -= ec.EnvironmentTimeScale * Time.deltaTime;
            gauge.SetValue(chargeDuration, t);
            yield return null;
        }
        EndCharge();
    }

    private void EndCharge(bool forceIgnoreOff = false)
    {
        if (isCharging)
        {
            isCharging = false;
            gauge?.Deactivate();
            if (chargeForce != null)
                pm.plm.Entity.RemoveForce(chargeForce);

            if (forceIgnoreOff)
            {
                for (int i = 0; i < pm.ec.Npcs.Count; i++) // Don't ignore anymore
                {
                    if (pm.ec.Npcs[i].Navigator.Entity.IsIgnoring(pm.plm.Entity))
                        pm.ec.Npcs[i].Navigator.Entity.IgnoreEntity(pm.plm.Entity, false);
                }
                Destroy(gameObject);
                return;
            }
            StartCoroutine(DelayForIgnoreEntityOff());
        }
    }

    IEnumerator DelayForIgnoreEntityOff()
    {
        yield return null;
        yield return null;
        for (int i = 0; i < pm.ec.Npcs.Count; i++) // Don't ignore anymore
        {
            if (pm.ec.Npcs[i].Navigator.Entity.IsIgnoring(pm.plm.Entity))
                pm.ec.Npcs[i].Navigator.Entity.IgnoreEntity(pm.plm.Entity, false);
        }
        Destroy(gameObject);
    }

    void OnDestroy() =>
        EndCharge(true);

}