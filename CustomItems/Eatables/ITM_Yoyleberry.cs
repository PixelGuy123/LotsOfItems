using System.Collections;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.PlusExtensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.Eatables;

public class ITM_Yoyleberry : ITM_GenericZestyEatable
{
    public float immunityDuration = 5f;
    public float slowMultiplier = 0.5f;

    public override bool Use(PlayerManager pm)
    {
        this.pm = pm;
        pm.plm.stamina = pm.GetMovementStatModifier().baseStats["staminaMax"];
        StartCoroutine(YoyleEffect());
        Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audEat);
        return true;
    }

    private IEnumerator YoyleEffect()
    {
        gauge = Singleton<CoreGameManager>.Instance.GetHud(pm.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, immunityDuration);

        var stats = pm.GetMovementStatModifier();
        ValueModifier slowMod = new(slowMultiplier);
        stats.AddModifier("runSpeed", slowMod);
        stats.AddModifier("walkSpeed", slowMod);

        var baldi = pm.ec.GetBaldi();
        if (baldi)
            pm.plm.Entity.IgnoreEntity(baldi.Navigator.Entity, true);

        float timer = immunityDuration;
        while (timer > 0f)
        {
            timer -= Time.deltaTime * pm.PlayerTimeScale;
            gauge.SetValue(immunityDuration, timer);
            yield return null;
        }

        if (baldi)
            pm.plm.Entity.IgnoreEntity(baldi.Navigator.Entity, false);

        stats.RemoveModifier(slowMod);
        gauge.Deactivate();
        Destroy(gameObject);
    }
}