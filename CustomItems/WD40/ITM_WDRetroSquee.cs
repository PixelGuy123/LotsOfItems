using LotsOfItems.Components;
using LotsOfItems.ItemPrefabStructures;
using MTM101BaldAPI.Registers;
using UnityEngine;

namespace LotsOfItems.CustomItems.WD40;

public class ITM_WDRetroSquee : Item, IItemPrefab
{
    [SerializeField]
    SoundObject audUse;
    public void SetupPrefab(ItemObject itm) =>
        audUse = ((ITM_NoSquee)ItemMetaStorage.Instance.FindByEnum(Items.Wd40).value.item).sound;

    public void SetupPrefabPost() { }
    public override bool Use(PlayerManager pm)
    {
        if (Physics.Raycast(pm.transform.position, Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, out RaycastHit hit, pm.pc.Reach, pm.pc.ClickLayers))
        {
            StandardDoor door = hit.transform.GetComponent<StandardDoor>();
            if (door && !door.GetComponent<Marker_StandardDoorSilenced>())
            {
                door.gameObject.AddComponent<Marker_StandardDoorSilenced>().counter = 3;
                Destroy(gameObject);
                Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audUse);
                return true;
            }
        }
        Destroy(gameObject);
        return false;
    }
}