using System.Collections;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Patches;
using MTM101BaldAPI.Registers;
using UnityEngine;

namespace LotsOfItems.CustomItems.StretchyGloves;

public class ITM_StretchyStickyGloves : ITM_ReachExtender, IItemPrefab
{
    public void SetupPrefab(ItemObject itm)
    {
        audBreak = (ItemMetaStorage.Instance.FindByEnum(Items.GrapplingHook).value.item as ITM_GrapplingHook).audSnap;
        gaugeSprite = itm.itemSpriteSmall;
    }

    public void SetupPrefabPost() { }

    [SerializeField]
    internal float usageSubtraction = 15f;

    [SerializeField]
    internal SoundObject audBreak;
    [SerializeField]
    internal LayerMask clickLayerMaskCheck = LayerMask.GetMask("Wall", "EntityBuffer", "CollidableEntities", "ClickableCollidable");
    float time;
    public override bool Use(PlayerManager pm)
    {
        this.pm = pm;
        pm.pc.GetHandler().OnPlayerClick += OnClick;
        base.pm = pm;
        gauge = Singleton<CoreGameManager>.Instance.GetHud(pm.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, setTime);
        reachExtension.distance = 10000f;
        reachExtension.overrideSquished = true;
        pm.pc.reachExtensions.Add(reachExtension);
        StartCoroutine(NewTimer());
        Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audUse);
        return true;
    }

    IEnumerator NewTimer()
    {
        time = setTime;
        yield return null;
        while (time > 0f)
        {
            time -= Time.deltaTime * pm.PlayerTimeScale;
            gauge.SetValue(setTime, time);
            yield return null;
        }

        pm.pc.reachExtensions.Remove(reachExtension);
        gauge.Deactivate();
        Destroy(gameObject);
    }

    private void OnClick(GameObject clickable, PlayerClick click)
    {
        Vector3 teleportPos = pm.ec.CellFromPosition(clickable.transform.position).FloorWorldPosition;
        if (Physics.CheckCapsule(teleportPos, teleportPos + Vector3.up * 10f, click.pm.plm.Entity.colliderRadius * 0.25f, clickLayerMaskCheck)) // Failsafe to not teleport into a table
            return;

        pm.Teleport(teleportPos);
        time -= usageSubtraction;
        if (time <= 0f)
        {
            pm.pc.reachExtensions.Remove(reachExtension);
            gauge.Deactivate();
            Destroy(gameObject);
            Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audBreak);
        }
    }

    void OnDestroy()
    {
        if (pm != null)
            pm.pc.GetHandler().OnPlayerClick -= OnClick;
    }
}