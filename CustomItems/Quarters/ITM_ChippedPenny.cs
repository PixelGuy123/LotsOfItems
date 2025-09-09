using LotsOfItems.ItemPrefabStructures;
using MTM101BaldAPI.Registers;
using UnityEngine;

namespace LotsOfItems.CustomItems.Quarters;

public class ITM_ChippedPenny : Item, IItemPrefab
{
    [SerializeField]
    private ItemObject quarterItem, pennyItem;

    public void SetupPrefab(ItemObject itm)
    {
        quarterItem = ItemMetaStorage.Instance.FindByEnum(Items.Quarter).value;
        pennyItem = itm;
    }

    public void SetupPrefabPost() { }

    public override bool Use(PlayerManager pm)
    {
        if (Physics.Raycast(pm.transform.position, Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, out var hit, pm.pc.Reach, pm.pc.ClickLayers))
        {
            IItemAcceptor component = hit.transform.GetComponent<IItemAcceptor>();
            if (component != null && component.ItemFits(quarterItem.itemType))
            {
                int outcome = Random.Range(0, 4);
                switch (outcome)
                {
                    case 0: // Get both back
                        component.InsertItem(pm, pm.ec);
                        GiveItem(pm, pennyItem);
                        GiveItem(pm, quarterItem);
                        break;
                    case 1: // Get penny back
                        component.InsertItem(pm, pm.ec);
                        GiveItem(pm, pennyItem);
                        break;
                    case 2: // Normal use
                        component.InsertItem(pm, pm.ec);
                        break;
                    case 3: // Gets destroyed
                        // Do nothing, just consume the item
                        break;
                }
                Destroy(gameObject);
                return true;
            }
        }
        Destroy(gameObject);
        return false;
    }

    private void GiveItem(PlayerManager pm, ItemObject item)
    {
        if (!pm.itm.AddItem(item, null))
        {
            Pickup pickup = pm.ec.CreateItem(pm.plm.Entity.CurrentRoom, item, new Vector2(pm.transform.position.x, pm.transform.position.z));
            pickup.transform.position += Vector3.up * 5f + Random.insideUnitSphere * 2f;
        }
    }
}