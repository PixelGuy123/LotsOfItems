using System.Collections;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Patches;
using LotsOfItems.Plugin;
using MTM101BaldAPI;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.PlusExtensions;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.Eatables;

public class ITM_FireFlavouredZestyBar : ITM_GenericZestyEatable
{
    [SerializeField]
    private float effectDuration = 10f, beforeChaosDelay = 2.5f, smokeOutTreshold = 7.54f, smokeEndThreshold = 10.095f; // Exact timestamp of the sound effect

    [SerializeField]
    private SoundObject audTrainSmoke;

    [SerializeField]
    private ParticleSystem smokeParticles;

    [SerializeField]
    private AudioManager audMan;

    readonly ValueModifier fovMod = new();

    protected override void VirtualSetupPrefab(ItemObject itemObject)
    {
        base.VirtualSetupPrefab(itemObject);
        audTrainSmoke = this.GetSoundNoSub("FireFlavouredZestyBar_train.wav", SoundType.Effect);

        smokeParticles = ParticleExtensions.GetNewParticleSystem(ParticleExtensions.GetRawChalkParticleGenerator(true).particles, out var systemRenderer);
        Destroy(smokeParticles.GetComponent<CoverCloud>());

        // Setup Dust Cloud visual
        var newTexture = Instantiate(systemRenderer.material.mainTexture as Texture2D);
        newTexture.name = "SmokeFireZestyBar";
        TextureExtensions.ApplyLightLevel(newTexture, -50f);
        systemRenderer.material.mainTexture = newTexture;

        var main = smokeParticles.main;
        main.startLifetime = 1.5f;
        main.gravityModifierMultiplier = -2f;
        var emission = smokeParticles.emission;
        emission.enabled = true;
        emission.rateOverTime = 40f;
        var velocity = smokeParticles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.x = new(-2f, 2f);
        velocity.y = new(-3f, 0.25f);
        velocity.z = new(5f, 10f);

        smokeParticles.gameObject.SetActive(false);
        smokeParticles.transform.SetParent(transform);
        smokeParticles.transform.localPosition = Vector3.zero;
        smokeParticles.transform.localRotation = Quaternion.identity;

        audMan = gameObject.CreateAudioManager(35f, 75f).MakeAudioManagerNonPositional();
    }

    public override bool Use(PlayerManager pm)
    {
        this.pm = pm;
        pm.plm.stamina = pm.GetMovementStatModifier().baseStats["staminaMax"] * 3f;
        Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audEat);

        StartCoroutine(EffectCoroutine());
        return true;
    }

    protected override bool CanBeDestroyed() => false;

    private IEnumerator EffectCoroutine()
    {
        yield return new WaitForSecondsEnvironmentTimescale(pm.ec, beforeChaosDelay);
        audMan.SetLoop(true);
        audMan.maintainLoop = true;
        audMan.QueueAudio(audTrainSmoke);

        var cam = pm.GetCustomCam();
        cam.SlideFOVAnimation(fovMod, 35f, 1.75f);

        smokeParticles.gameObject.SetActive(true);
        gauge = Singleton<CoreGameManager>.Instance.GetHud(pm.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, effectDuration);

        PlayerMovementPatches.AddDisabledPlayer(pm);

        var emission = smokeParticles.emission;
        emission.enabled = false;
        float thresholdTimer = 0f;
        byte reachedThresholdState = 0;

        float timer = effectDuration;
        while (timer > 0f)
        {
            transform.position = pm.transform.position;
            transform.forward = pm.transform.forward;
            timer -= Time.deltaTime * pm.ec.EnvironmentTimeScale;
            gauge.SetValue(effectDuration, timer);

            float invertRun = pm.plm.runSpeed * (pm.reversed ? -1 : 1);
            pm.plm.Entity.UpdateInternalMovement(pm.transform.forward * invertRun * pm.PlayerTimeScale);
            if (pm.plm.Entity.Velocity != Vector3.zero) // Checks velocity to be sure player is actually running
            {
                pm.plm.stamina = Mathf.Max(
                        pm.plm.stamina - pm.plm.staminaDrop * Time.deltaTime * pm.PlayerTimeScale,
                        0f
                    );
            }
            switch (reachedThresholdState)
            {
                case 0:
                    if (thresholdTimer >= smokeOutTreshold)
                    {
                        reachedThresholdState = 1;
                        emission.enabled = true;
                    }
                    break;
                case 1:
                    if (thresholdTimer >= smokeEndThreshold)
                    {
                        reachedThresholdState = 2;
                        emission.enabled = false;
                    }
                    break;
                case 2:
                    if (thresholdTimer >= audTrainSmoke.subDuration)
                    {
                        reachedThresholdState = 0;
                        thresholdTimer %= audTrainSmoke.subDuration;
                    }
                    break;
                default:
                    throw new System.IndexOutOfRangeException("Somehow, in some world, the smoke system of the Fire Zesty Bar broke.");
            }
            thresholdTimer += Time.deltaTime * pm.ec.EnvironmentTimeScale;

            yield return null;
        }

        PlayerMovementPatches.RemoveDisabledPlayer(pm);
        cam.ResetSlideFOVAnimation(fovMod, 2.25f);


        emission.enabled = false;

        audMan.FadeOut(0.5f);

        while (audMan.AnyAudioIsPlaying)
            yield return null;

        Destroy(gameObject);
    }
}