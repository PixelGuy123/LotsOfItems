using System.Collections;
using MTM101BaldAPI.PlusExtensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.Eatables;

public class ITM_ZestyFlavouredWhiteBar : ITM_GenericZestyEatable
{
    [SerializeField]
    internal float minimumStaminaThreshold = 50f;
    float maxTime;
    public override bool Use(PlayerManager pm)
    {
        this.pm = pm;
        var statModifier = pm.GetMovementStatModifier();

        pm.plm.stamina = statModifier.baseStats["staminaMax"] * 2f;
        maxTime = Mathf.Abs(pm.plm.stamina - minimumStaminaThreshold);
        gauge = Singleton<CoreGameManager>.Instance.GetHud(pm.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, maxTime);
        StartCoroutine(StaminaDrain());
        Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audEat);
        return true;
    }

    private IEnumerator StaminaDrain()
    {
        while (pm.plm.stamina > minimumStaminaThreshold)
        {
            pm.plm.stamina = Mathf.Max(pm.plm.stamina - pm.plm.staminaDrop * Time.deltaTime * pm.PlayerTimeScale, 0f);
            gauge.SetValue(maxTime, Mathf.Abs(pm.plm.stamina - minimumStaminaThreshold));
            yield return null;
        }
        gauge?.Deactivate();
        Destroy(gameObject);
    }
}