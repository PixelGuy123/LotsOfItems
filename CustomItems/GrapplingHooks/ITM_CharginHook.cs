using System.Collections;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using MTM101BaldAPI;
using MTM101BaldAPI.Registers;
using UnityEngine;

namespace LotsOfItems.CustomItems.GrapplingHooks;

public class ITM_CharginHook : Item, IItemPrefab
{
    private bool isCharging = false;
    [SerializeField]
    internal float chargeDuration = 2f;
    [SerializeField]
    internal float chargeSpeed = 120f;
    [SerializeField]
    internal int noiseVal = 64;
    [SerializeField]
    internal SoundObject audUse, audBump;
    Force chargeForce;
    EnvironmentController ec;

    public void SetupPrefab(ItemObject itm)
    {
        audUse = (ItemMetaStorage.Instance.FindByEnum(Items.GrapplingHook).value.item as ITM_GrapplingHook).audLaunch;
        audBump = LotOfItemsPlugin.assetMan.Get<SoundObject>("audBump");
    }
    public void SetupPrefabPost() { }

    public override bool Use(PlayerManager pm)
    {
        this.pm = pm;
        ec = pm.ec;

        isCharging = true;
        chargeForce = new Force(pm.transform.forward, chargeSpeed, 0f);
        pm.plm.Entity.AddForce(chargeForce);

        pm.plm.Entity.OnEntityMoveInitialCollision += WallHit;
        StartCoroutine(ChargeTimer());

        Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audUse);

        return true;
    }

    void Update()
    {
        if (!isCharging) return;

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
    }

    private void WallHit(RaycastHit hit)
    {
        if (isCharging)
        {
            Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audBump);
            ec.MakeNoise(pm.transform.position, noiseVal, true);
            EndCharge();
        }
    }

    private IEnumerator ChargeTimer()
    {
        yield return new WaitForSecondsEnvironmentTimescale(ec, chargeDuration);
        EndCharge();
    }

    private void EndCharge()
    {
        if (isCharging)
        {
            isCharging = false;
            pm.plm.Entity.OnEntityMoveInitialCollision -= WallHit;
            chargeForce?.Kill();

            Destroy(gameObject);
        }
    }

    void OnDestroy() =>
        EndCharge();

}