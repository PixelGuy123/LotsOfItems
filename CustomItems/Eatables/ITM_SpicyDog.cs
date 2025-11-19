using System.Collections;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.PlusExtensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.Eatables;

public class ITM_SpicyDog : ITM_GenericZestyEatable
{
    public MovementModifier speedMod = new(Vector3.zero, 1.25f); // 25% faster
    public ValueModifier noGainMod = new(0f), slightMoreDropMod = new(1.1f);
    public float duration = 15f;

    protected override void VirtualSetupPrefab(ItemObject itemObject)
    {
        base.VirtualSetupPrefab(itemObject);
        // We handle the gauge manually in the coroutine
        affectorTime = 0f;
        staminaGain = 100f; // Normal zesty amount usually
    }

    protected override bool CanBeDestroyed() => false;

    public override bool Use(PlayerManager pm)
    {
        this.pm = pm;
        bool used = base.Use(pm);
        if (used)
        {
            StartCoroutine(SpicyEffect());
        }
        return used;
    }

    public IEnumerator SpicyEffect()
    {
        pm.Am.moveMods.Add(speedMod);
        var stats = pm.GetMovementStatModifier();

        stats.AddModifier("staminaRise", noGainMod); // 0 out stamina rise
        stats.AddModifier("staminaDrop", slightMoreDropMod); // 0 out stamina rise
        gauge = Singleton<CoreGameManager>.Instance.GetHud(pm.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, duration);

        float timer = duration;
        float lastStamina = pm.plm.stamina;

        while (timer > 0f)
        {
            timer -= Time.deltaTime * pm.PlayerTimeScale;
            gauge.SetValue(duration, timer);

            // Check if stamina increased (got stamina from any source)
            if (pm.plm.stamina > lastStamina)
                break;

            lastStamina = pm.plm.stamina;
            yield return null;
        }

        stats.RemoveModifier(noGainMod); // 0 out stamina
        stats.RemoveModifier(slightMoreDropMod);
        pm.Am.moveMods.Remove(speedMod);
        gauge.Deactivate();
        Destroy(gameObject);
    }

    void OnDestroy() => gauge?.Deactivate();
}