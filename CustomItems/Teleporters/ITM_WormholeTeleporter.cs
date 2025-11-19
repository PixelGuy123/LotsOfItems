using LotsOfItems.Components;
using LotsOfItems.ItemPrefabStructures;
using MTM101BaldAPI.Registers;
using UnityEngine;

namespace LotsOfItems.CustomItems.Teleporters;

public class ITM_WormholeTeleporter : Item, IItemPrefab
{
    [SerializeField]
    internal MiniBlackHole blackHolePre;

    [SerializeField]
    internal SoundObject audTeleport;

    public void SetupPrefab(ItemObject itm)
    {
        blackHolePre = MiniBlackHole.CreatePrefab(35f, 5);

        audTeleport = ((ITM_Teleporter)ItemMetaStorage.Instance.FindByEnum(Items.Teleporter).value.item).audTeleport;
    }
    public void SetupPrefabPost() { }

    public override bool Use(PlayerManager pm)
    {
        var bh = Instantiate(blackHolePre, pm.transform.position, Quaternion.identity);
        bh.Ec = pm.ec;
        Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audTeleport);
        Destroy(gameObject);

        return true;
    }
}
