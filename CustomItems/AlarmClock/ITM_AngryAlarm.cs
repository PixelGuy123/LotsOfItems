using System.Collections;
using LotsOfItems.ItemPrefabStructures;
using UnityEngine;

namespace LotsOfItems.CustomItems.AlarmClock;

public class ITM_AngryAlarm : ITM_GenericAlarmClock
{
    public Components.MomentumNavigator navigator;
    public Baldi target;
    public float punchCooldown;
    public float punchForce = 20f, durationBeforeBreaking = 15f;

    protected override void VirtualSetupPrefab(ItemObject itm)
    {
        base.VirtualSetupPrefab(itm);
        navigator = gameObject.AddComponent<Components.MomentumNavigator>();
        navigator.maxSpeed = 45f; // Fast speed
        navigator.accel = 60f;

        clockSprite = this.GetSpriteSheet("AngryAlarm_Clock.png", 2, 2, clockSprite[0].pixelsPerUnit);
    }

    protected override bool ShouldRingOnEnd()
    {
        // Prevent destruction and start chasing
        StartCoroutine(ChaseSequence());
        return false;
    }

    public IEnumerator ChaseSequence()
    {
        navigator.Initialize(ec);

        audMan.PlaySingle(audRing); // Play ring
        ec.MakeNoise(transform.position, noiseVal + 1); // 1 higher noise

        float duration = durationBeforeBreaking;

        while (duration > 0f)
        {
            duration -= Time.deltaTime * ec.EnvironmentTimeScale;

            if (target == null)
            {
                target = ec.GetBaldi();
            }
            else
            {
                navigator.FindPath(target.transform.position);

                // Punch logic
                if (Vector3.Distance(transform.position, target.transform.position) < 2.5f)
                {
                    if (punchCooldown <= 0f)
                    {
                        audMan.PlaySingle(audWind); // reusing this lol
                        target.Navigator.Entity.AddForce(new Force((target.transform.position - transform.position).normalized, punchForce, -punchForce * 0.5f));
                        punchCooldown = 0.25f;
                    }
                }
            }

            if (punchCooldown > 0f) punchCooldown -= Time.deltaTime * ec.EnvironmentTimeScale;

            yield return null;
        }

        Destroy(gameObject);
    }
}