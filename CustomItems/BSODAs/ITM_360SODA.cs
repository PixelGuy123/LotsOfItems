using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using UnityEngine;

namespace LotsOfItems.CustomItems.BSODAs;

public class ITM_360SODA : Item, IItemPrefab
{
    [SerializeField]
    internal ITM_BSODA bsodaPrefab;
    [SerializeField]
    internal SoundObject audUse;
    public void SetupPrefab(ItemObject itm)
    {
        bsodaPrefab = ItemExtensions.GetVariantInstance<ITM_BSODA>(Items.Bsoda);
        audUse = bsodaPrefab.sound;
        bsodaPrefab.spriteRenderer.sprite = this.GetSprite("ThreeSixtySODA_Spray.png", bsodaPrefab.spriteRenderer.sprite.pixelsPerUnit);
    }
    public void SetupPrefabPost() { }
    public override bool Use(PlayerManager pm)
    {
        Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audUse);
        for (int i = 0; i < 16; i++)
        {
            float angle = i * (360f / 16f);
            Quaternion rotation = Quaternion.Euler(0, angle, 0);
            Vector3 direction = rotation * Vector3.forward;

            ITM_BSODA bsoda = Instantiate(bsodaPrefab);
            bsoda.IndividuallySpawn(pm.ec, pm.transform.position, direction);
        }

        Destroy(gameObject);
        return true;
    }
}