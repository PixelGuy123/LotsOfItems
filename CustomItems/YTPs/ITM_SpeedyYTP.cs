using MTM101BaldAPI.PlusExtensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.YTPs;

public class ITM_SpeedyYTP : ITM_GenericZestyEatable
{
    [SerializeField]
    internal int minPoints = 40, maxPoints = 60;
    protected override void VirtualSetupPrefab(ItemObject itemObject)
    {
        base.VirtualSetupPrefab(itemObject);
        affectorTime = 10f;
        speedMultiplier = 1.2f;
    }
    public override bool Use(PlayerManager pm)
    {
        int points = Random.Range(minPoints, maxPoints + 1);
        Singleton<CoreGameManager>.Instance.AddPoints(points, pm.playerNumber, true);

        this.pm = pm;
        gauge = Singleton<CoreGameManager>.Instance.GetHud(pm.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, affectorTime);
        StartCoroutine(SpeedAffector(pm.GetMovementStatModifier()));
        return true;
    }
}