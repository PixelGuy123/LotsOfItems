using System.Collections;
using System.Collections.Generic;
using LotsOfItems.Components;
using LotsOfItems.CustomItems.Eatables;
using LotsOfItems.Plugin;
using MTM101BaldAPI;
using MTM101BaldAPI.Registers;
using UnityEngine;

namespace LotsOfItems.CustomItems.NanaPeels;

public class ITM_IceNanaPeel : ITM_GenericNanaPeel
{
    public SlippingObject icePatchPrefab;
    public float iceLifetime = 10f;
    public float freezeDuration = 5f;
    readonly private HashSet<Cell> iceCoveredCells = [];
    public Sprite gaugeSprite;

    protected override void VirtualSetupPrefab(ItemObject itm)
    {
        var iceZestyBar = ItemMetaStorage.Instance.FindByEnumFromMod(EnumExtensions.GetFromExtendedName<Items>("IceZestyBar"), LotOfItemsPlugin.plug.Info).value.item as ITM_IceZestyBar;
        icePatchPrefab = iceZestyBar.slipObjPre;
        gaugeSprite = itm.itemSpriteLarge;

        var renderer = GetComponentInChildren<SpriteRenderer>();
        renderer.sprite = itm.itemSpriteLarge.DuplicateItself(renderer.sprite.pixelsPerUnit);
        renderer.sprite.name = $"{itm.itemSpriteLarge.name}_World";

        endHeight = 1.11f;
    }

    internal override bool EntityTriggerStayOverride(Collider other)
    {
        Entity component = other.GetComponent<Entity>();
        if (component != null && component.Grounded && component.Velocity.magnitude > 0f)
        {
            StartCoroutine(LeaveIceTrail());
        }
        return true;
    }

    private IEnumerator LeaveIceTrail()
    {
        while (slipping && slippingEntity)
        {
            Cell currentCell = ec.CellFromPosition(slippingEntity.transform.position);
            if (!currentCell.Null && !iceCoveredCells.Contains(currentCell))
            {
                iceCoveredCells.Add(currentCell);
                var ice = Instantiate(icePatchPrefab, currentCell.FloorWorldPosition, Quaternion.identity);
                ice.SetAnOwner(slippingEntity.gameObject);
                ice.StartCoroutine(IceSelfDestruct(ec, ice.gameObject));
            }
            yield return null;
        }
    }

    internal override bool OnCollisionOverride(RaycastHit hit)
    {
        if (slipping && slippingEntity)
        {
            End();
        }
        return false;
    }

    internal override bool VirtualEnd()
    {
        slippingEntity.StartCoroutine(UnfreezeTimer(ec, slippingEntity, freezeDuration, slippingEntity.GetComponent<PlayerManager>()));
        return base.VirtualEnd();
    }

    private IEnumerator UnfreezeTimer(EnvironmentController ec, Entity entity, float duration, PlayerManager player)
    {
        bool hasGauge = false;
        HudGauge gauge = null;
        if (player)
        {
            gauge = Singleton<CoreGameManager>.Instance.GetHud(player.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, freezeDuration);
            hasGauge = true;
        }

        MovementModifier moveMod = new(Vector3.zero, 0f);
        entity.ExternalActivity.moveMods.Add(moveMod);
        float t = freezeDuration;
        while (t > 0f)
        {
            t -= Time.deltaTime * ec.EnvironmentTimeScale;
            if (hasGauge)
                gauge.SetValue(freezeDuration, t);
            yield return null;
        }

        if (hasGauge)
            gauge.Deactivate();
        entity.ExternalActivity.moveMods.Remove(moveMod);
    }

    private IEnumerator IceSelfDestruct(EnvironmentController ec, GameObject obj)
    {
        yield return new WaitForSecondsEnvironmentTimescale(ec, iceLifetime);
        Destroy(obj);
    }
}