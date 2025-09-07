using System.Collections;
using LotsOfItems.ItemPrefabStructures;
using UnityEngine;

namespace LotsOfItems.CustomItems.Eatables;

public class ITM_MysteryBrewCoffee : ITM_GenericZestyEatable
{
    private enum BrewEffect { Hyper, Decaf, Crash }

    [SerializeField]
    internal float minEffectTimer = 15f, maxEffectTimer = 20f;

    protected override void VirtualSetupPrefab(ItemObject itemObject)
    {
        base.VirtualSetupPrefab(itemObject);
        audEat = this.GetSound("MysteryBrewCoffee_Drink.wav", "LtsOItems_Vfx_Drinking", SoundType.Effect, Color.white);
    }

    public override bool Use(PlayerManager pm)
    {
        this.pm = pm;
        BrewEffect effect = (BrewEffect)Random.Range(0, 3);
        affectorTime = Random.Range(minEffectTimer, maxEffectTimer);

        switch (effect)
        {
            case BrewEffect.Hyper:
                speedMultiplier = 1.8f;
                staminaGain = pm.plm.staminaMax;
                break;
            case BrewEffect.Decaf:
                speedMultiplier = 0.85f;
                pm.ec.MakeSilent(affectorTime);
                break;
            case BrewEffect.Crash:
                StartCoroutine(CrashEffect());
                return base.Use(pm); // Let base handle the gauge
        }

        return base.Use(pm);
    }

    private IEnumerator CrashEffect()
    {
        float crashSpeedStart = 2f;
        float crashSpeedEnd = 0.7f;
        float timer = affectorTime;

        while (timer > 0f)
        {
            float progress = 1f - (timer / affectorTime);
            speedMultiplier = Mathf.Lerp(crashSpeedStart, crashSpeedEnd, progress);
            timer -= Time.deltaTime * pm.PlayerTimeScale;
            yield return null;
        }

        speedMultiplier = 1f; // Reset speed
        pm.plm.stamina = 0f;
        yield return null;
    }
}