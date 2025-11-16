using System.Collections;
using UnityEngine;

namespace LotsOfItems.CustomItems.Teleporters;

public class ITM_SuperchargedTeleporter : ITM_GenericTeleporter
{
    TimeScaleModifier timeModifier;

    [SerializeField]
    internal float startTimerValue = 0.75f, duration = 15f;

    public override bool Use(PlayerManager pm)
    {
        this.pm = pm;
        timeModifier = new(startTimerValue, startTimerValue, 1f);
        pm.ec.AddTimeScale(timeModifier);
        StartCoroutine(TeleportAndNormalize());
        return true;
    }

    private IEnumerator TeleportAndNormalize()
    {
        // Base dangerous teleporter behavior
        int teleports = Random.Range(minTeleports, maxTeleports + 1);
        int teleportCount = 0;
        float currentTime = baseTime;

        pm.plm.Entity.SetInteractionState(false);
        pm.plm.Entity.SetFrozen(true);

        while (teleportCount < teleports)
        {
            currentTime -= Time.deltaTime; // No EnvironmentTimeScale to not slow down itself
            if (currentTime <= 0f)
            {
                Teleport();
                teleportCount++;
                baseTime *= increaseFactor;
                currentTime = baseTime;
            }
            yield return null;
        }

        pm.plm.Entity.SetInteractionState(true);
        pm.plm.Entity.SetFrozen(false);

        // Normalize time
        float normalizeDuration = duration;
        float elapsedTime = 0f;
        while (elapsedTime < normalizeDuration)
        {
            float t = elapsedTime / normalizeDuration;
            timeModifier.environmentTimeScale = Mathf.Lerp(startTimerValue, 1f, t);
            timeModifier.npcTimeScale = Mathf.Lerp(startTimerValue, 1f, t);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        pm.ec.RemoveTimeScale(timeModifier);
        Destroy(gameObject);
    }
}