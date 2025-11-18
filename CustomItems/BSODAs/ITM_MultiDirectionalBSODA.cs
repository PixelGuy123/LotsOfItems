using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using UnityEngine;

namespace LotsOfItems.CustomItems.BSODAs;

public class ITM_MultiDirectionalBSODA : Item, IItemPrefab, IBsodaShooter
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
        float angle = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.eulerAngles.y;
        for (int i = 0; i < numOfSodas; i++)
        {
            ShootBsoda(bsodaPrefab, pm, pm.transform.position, Quaternion.Euler(0, angle, 0));
            angle = (angle + angleAddition) % 360f;
        }

        Destroy(gameObject);
        return true;
    }
    public void ShootBsoda(ITM_BSODA bsoda, PlayerManager pm, Vector3 position, Quaternion rotation)
    {
        ITM_BSODA newBsoda = Instantiate(bsoda);
        newBsoda.IndividuallySpawn(pm.ec, position, rotation * Vector3.forward);
    }
}