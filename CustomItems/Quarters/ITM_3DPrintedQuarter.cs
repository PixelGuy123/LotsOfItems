using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using UnityEngine;

namespace LotsOfItems.CustomItems.Quarters;

public class ITM_3DPrintedQuarter : Item, IItemPrefab
{
    [SerializeField]
    internal Items quarterType = Items.Quarter;
    [SerializeField]
    [Range(0f, 1f)]
    internal float successChance = 0.6f;
    [SerializeField]
    internal SoundObject audBuzz;
    private RaycastHit hit;
    private IItemAcceptor _current;
    bool successInsert = false;

    public void SetupPrefab(ItemObject itm) =>
        audBuzz = LotOfItemsPlugin.assetMan.Get<SoundObject>("audBuzz");

    public void SetupPrefabPost() { }

    public override bool Use(PlayerManager pm)
    {
        this.pm = pm;

        if (Physics.Raycast(pm.transform.position, Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, out hit, pm.pc.reach, pm.pc.ClickLayers))
        {
            _current = hit.transform.GetComponent<IItemAcceptor>();

            if (_current != null && _current.ItemFits(quarterType))
            {
                // Roll the dice for success chance
                if (Random.value <= successChance)
                    // Success!
                    successInsert = true;
                else
                {
                    // Fail!
                    Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audBuzz);
                    pm.RuleBreak("Bullying", 2.5f, 1f);
                    Destroy(gameObject);
                }

                return true;
            }
        }

        // If raycast missed or hit something invalid, consume the item and return false.
        Destroy(gameObject);
        return false;
    }

    public override void PostUse(PlayerManager pm)
    {
        base.PostUse(pm);
        if (_current != null && successInsert)
        {
            _current.InsertItem(pm, pm.ec);
            Destroy(gameObject);
        }
    }
}