using LotsOfItems.ItemPrefabStructures;
using UnityEngine;

namespace LotsOfItems.CustomItems.Key;

public class ITM_KeyRing : Item, IItemPrefab
{
    [SerializeField]
    private Items item;

    public void SetupPrefab(ItemObject itm)
    {
        item = itm.itemType;
    }
    public void SetupPrefabPost() { }
    public ItemObject nextItem;

    public override bool Use(PlayerManager pm)
    {
        Destroy(gameObject);
        if (Physics.Raycast(pm.transform.position, Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, out RaycastHit hit, pm.pc.Reach))
        {
            bool used = false;

            IItemAcceptor acceptor = hit.transform.GetComponent<IItemAcceptor>();
            if (acceptor != null && acceptor.ItemFits(item))
            {
                acceptor.InsertItem(pm, pm.ec);
                used = true;
            }
            else
            {
                GameLock gLock = hit.transform.GetComponent<GameLock>();
                if (gLock && gLock.locked)
                {
                    gLock.InsertItem(pm, pm.ec); // Force a gameLock removal
                    used = true;
                }
            }

            if (used && nextItem)
            {
                pm.itm.SetItem(nextItem, pm.itm.selectedItem);
                return false;
            }
            return used;
        }
        return false;
    }
}