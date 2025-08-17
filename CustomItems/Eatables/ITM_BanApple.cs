using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using MTM101BaldAPI.PlusExtensions;
using MTM101BaldAPI.Registers;
using PixelInternalAPI.Extensions;

namespace LotsOfItems.CustomItems.Eatables;

public class ITM_BanApple : Item, IItemPrefab
{
    public static ITM_NanaPeel nanaPeelItem;
    public float slipForce = 45f;
    public float slipAcceleration = -36f;
    public SoundObject audEat;

    public void SetupPrefab(ItemObject itm)
    {
        nanaPeelItem = ItemMetaStorage.Instance.FindByEnum(Items.NanaPeel).value.item as ITM_NanaPeel;
        audEat = GenericExtensions.FindResourceObjectByName<SoundObject>("ChipCrunch");
    }
    public void SetupPrefabPost() { }

    public override bool Use(PlayerManager pm)
    {
        pm.plm.stamina = pm.GetMovementStatModifier().baseStats["staminaMax"] * 5f;
        var nanaPeel = Instantiate(nanaPeelItem);
        nanaPeel.SpawnInstantly(pm.ec, pm.transform.position, pm.transform.forward, false);
        Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audEat);
        Destroy(gameObject);
        return true;
    }
}