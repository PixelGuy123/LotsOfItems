using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Patches;
using MTM101BaldAPI.Registers;
using UnityEngine;

namespace LotsOfItems.CustomItems.StretchyGloves;

public class ITM_StretchyStickyGloves : ITM_ReachExtender, IItemPrefab
{
    public void SetupPrefab(ItemObject itm) =>
        audBreak = (ItemMetaStorage.Instance.FindByEnum(Items.GrapplingHook).value.item as ITM_GrapplingHook).audSnap;

    public void SetupPrefabPost() { }

    [SerializeField]
    internal int uses = 3;

    [SerializeField]
    internal SoundObject audBreak;
    public override bool Use(PlayerManager pm)
    {
        this.pm = pm;
        pm.pc.GetHandler().OnPlayerClick += OnClick;
        return base.Use(pm);
    }

    private void OnClick(GameObject clickable, PlayerClick click)
    {
        Vector3 teleportPos = pm.ec.CellFromPosition(clickable.transform.position).FloorWorldPosition;
        if (Physics.CheckCapsule(teleportPos, teleportPos + Vector3.up * 10f, 4.5f)) // Failsafe to not teleport into a table
            return;

        pm.Teleport(teleportPos);
        if (--uses < 0)
        {
            Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audBreak);
            pm.pc.reachExtensions.Remove(reachExtension);
            gauge?.Deactivate();
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (pm != null)
            pm.pc.GetHandler().OnPlayerClick -= OnClick;
    }
}