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

    public void SetupPrefab(ItemObject itm) =>
        audBuzz = LotOfItemsPlugin.assetMan.Get<SoundObject>("audBuzz");

    public void SetupPrefabPost() { }

    public override bool Use(PlayerManager pm)
    {
        this.pm = pm;

        if (Physics.Raycast(pm.transform.position, Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, out hit, pm.pc.reach, pm.pc.ClickLayers))
        {
            IItemAcceptor acceptor = hit.transform.GetComponent<IItemAcceptor>();

            if (acceptor != null && acceptor.ItemFits(quarterType))
            {
                // Roll the dice for success chance
                if (Random.value <= successChance)
                    // Success!
                    acceptor.InsertItem(pm, pm.ec);
                else
                {
                    // Fail!
                    Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audBuzz);
                    pm.RuleBreak("Bullying", 2.5f, 1f);
                }


                // Quartrer is used regardless
                Destroy(gameObject);
                return true;
            }
        }

        // If raycast missed or hit something invalid, consume the item and return false.
        Destroy(gameObject);
        return false;
    }
}