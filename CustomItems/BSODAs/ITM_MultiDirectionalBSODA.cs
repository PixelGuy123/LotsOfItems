using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using UnityEngine;

namespace LotsOfItems.CustomItems.BSODAs;

public class ITM_MultiDirectionalBSODA : Item, IItemPrefab
{
    [SerializeField]
    internal ITM_BSODA bsodaPrefab;
    [SerializeField]
    internal SoundObject audUse;
    [SerializeField]
    internal int numOfSodas = 16;
    public void SetupPrefab(ItemObject itm) => VirtualSetupPrefab(itm);
    public void SetupPrefabPost() => VirtualSetupPrefabPost();
    protected virtual void VirtualSetupPrefab(ItemObject itm)
    {
        bsodaPrefab = ItemExtensions.GetVariantInstance<ITM_BSODA>(Items.Bsoda);
        audUse = bsodaPrefab.sound;
    }
    protected virtual void VirtualSetupPrefabPost() { }
    public override bool Use(PlayerManager pm)
    {
        Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audUse);
        float angleAddition = 360f / numOfSodas;
        float angle = pm.transform.eulerAngles.y;
        for (int i = 0; i < numOfSodas; i++)
        {
            ITM_BSODA bsoda = Instantiate(bsodaPrefab);
            bsoda.IndividuallySpawn(pm.ec, pm.transform.position, Quaternion.Euler(0, angle, 0) * Vector3.forward);
            angle = (angle + angleAddition) % 360f;
        }

        Destroy(gameObject);
        return true;
    }
}