using System.Collections;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.PlusExtensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.Eatables;

public class ITM_ZestyMegaBar : ITM_GenericZestyEatable
{
    [SerializeField]
    internal float burstDuration = 10f;
    [SerializeField]
    internal float crashDuration = 5f;
    [SerializeField]
    internal float burstSpeedMultiplier = 2.5f;
    [SerializeField]
    internal float crashSpeedMultiplier = 0.5f;

    protected override bool CanBeDestroyed() => false;

    public override bool Use(PlayerManager pm)
    {
        this.pm = pm;
        pm.plm.stamina = pm.plm.staminaMax;
        Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audEat);

        gauge = Singleton<CoreGameManager>.Instance.GetHud(pm.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, burstDuration + crashDuration);
        StartCoroutine(MegaEffect());

        return true;
    }

    private IEnumerator MegaEffect()
    {
        var statModifier = pm.GetMovementStatModifier();
        ValueModifier burstMod = new(burstSpeedMultiplier);
        statModifier.AddModifier("runSpeed", burstMod);

        float timer = burstDuration;
        while (timer > 0f)
        {
            gauge.SetValue(burstDuration, timer);
            timer -= Time.deltaTime * pm.PlayerTimeScale;
            yield return null;
        }

        statModifier.RemoveModifier(burstMod);

        ValueModifier crashMod = new(crashSpeedMultiplier);
        statModifier.AddModifier("runSpeed", crashMod);
        statModifier.AddModifier("walkSpeed", crashMod);
        pm.plm.stamina = 0f;

        timer = crashDuration;
        while (timer > 0f)
        {
            gauge.SetValue(crashDuration, timer);
            timer -= Time.deltaTime * pm.PlayerTimeScale;
            yield return null;
        }

        statModifier.RemoveModifier(crashMod);
        gauge.Deactivate();
        Destroy(gameObject);
    }
}