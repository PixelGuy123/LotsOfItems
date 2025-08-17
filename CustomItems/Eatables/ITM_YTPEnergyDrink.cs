using LotsOfItems.Plugin;
using MTM101BaldAPI.PlusExtensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.Eatables;

public class ITM_YTPEnergyDrink : ITM_GenericZestyEatable
{
    [SerializeField]
    internal int[] pointValues = [25, 50, 100, -25, -50, -100];

    [SerializeField]
    internal SoundObject[] pointSounds;

    protected override void VirtualSetupPrefab(ItemObject itemObject)
    {
        base.VirtualSetupPrefab(itemObject);
        audEat = LotOfItemsPlugin.assetMan.Get<SoundObject>("audDrink");
        pointSounds = new SoundObject[6];
        for (int i = 0; i < 3; i++)
            pointSounds[i] = LotOfItemsPlugin.assetMan.Get<SoundObject>("YtpPickup_" + i);
        pointSounds[3] = LotOfItemsPlugin.assetMan.Get<SoundObject>("YtpPickup_greenFakeYtp");
        pointSounds[4] = LotOfItemsPlugin.assetMan.Get<SoundObject>("YtpPickup_silverFakeYtp");
        pointSounds[5] = LotOfItemsPlugin.assetMan.Get<SoundObject>("YtpPickup_goldenFakeYtp");
    }

    public override bool Use(PlayerManager pm)
    {
        pm.plm.stamina = pm.GetMovementStatModifier().baseStats["staminaMax"] * 2f;
        int index = Random.Range(0, pointValues.Length);
        Singleton<CoreGameManager>.Instance.AddPoints(pointValues[index], pm.playerNumber, true);

        Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audEat);
        Singleton<CoreGameManager>.Instance.audMan.PlaySingle(pointSounds[index]);
        Destroy(gameObject);
        return true;
    }
}