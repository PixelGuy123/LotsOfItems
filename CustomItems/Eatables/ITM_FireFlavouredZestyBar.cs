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
    private float minEffectDuration = 45f, maxEffectDuration = 60f, beforeChaosDelay = 2.5f, smokeOutTreshold = 7.54f, smokeEndThreshold = 10.095f, hitDistanceCheck = 1.5f; // Exact timestamp of the sound effect

    [SerializeField]
    private SoundObject audTrainSmoke, audBang;

    [SerializeField]
    private ParticleSystem smokeParticles;

    [SerializeField]
    private AudioManager audMan;

    readonly ValueModifier fovMod = new();

    protected override void VirtualSetupPrefab(ItemObject itemObject)
    {
        base.VirtualSetupPrefab(itemObject);
        audTrainSmoke = this.GetSoundNoSub("FireFlavouredZestyBar_train.wav", SoundType.Effect);
        audBang = LotOfItemsPlugin.assetMan.Get<SoundObject>("audBump");

        smokeParticles = ParticleExtensions.GetNewParticleSystem(ParticleExtensions.GetRawChalkParticleGenerator(true).particles, out var systemRenderer);
        Destroy(smokeParticles.GetComponent<CoverCloud>());

        // Setup Dust Cloud visual
        var newTexture = Instantiate(LotOfItemsPlugin.assetMan.Get<Texture2D>("DustCloudTexture"));
        newTexture.name = "SmokeFireZestyBar";
        TextureExtensions.ApplyLightLevel(newTexture, -35f);
        systemRenderer.material.mainTexture = newTexture;

        var main = smokeParticles.main;
        main.startLifetime = 1.5f;
        main.gravityModifierMultiplier = -2f;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        var emission = smokeParticles.emission;
        emission.enabled = true;
        emission.rateOverTime = 65f;
        var velocity = smokeParticles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.x = new(-5f, 5f);
        velocity.y = new(-3f, 0.25f);
        velocity.z = new(5f, 12f);
        var rotation = smokeParticles.rotationOverLifetime;
        rotation.enabled = true;
        rotation.x = new(-8.5f, 8.5f);
        rotation.y = rotation.x;
        rotation.z = rotation.x;

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
        MovementModifier openOnCollisionMod = new(Vector3.zero, 1f, 0)
        {
            forceTrigger = true
        };
        audMan.SetLoop(true);
        audMan.maintainLoop = true;
        audMan.QueueAudio(audTrainSmoke);

        var cam = pm.GetCustomCam();
        cam.SlideFOVAnimation(fovMod, 35f, 1.75f);

        smokeParticles.gameObject.SetActive(true);
        float maxTime = Random.Range(minEffectDuration, maxEffectDuration);
        gauge = Singleton<CoreGameManager>.Instance.GetHud(pm.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, maxTime);

        PlayerMovementPatches.AddDisabledPlayer(pm);

        var emission = smokeParticles.emission;
        emission.enabled = false;
        byte reachedThresholdState = 0;

        pm.Am.moveMods.Add(openOnCollisionMod);

        float timer = maxTime;
        while (timer > 0f)
        {
            if (Time.timeScale == 0)
            {
                yield return null;
                continue;
            }
            transform.position = pm.transform.position + Vector3.down;
            transform.forward = pm.transform.forward;
            timer -= Time.deltaTime * pm.ec.EnvironmentTimeScale;
            gauge.SetValue(maxTime, timer);

            float invertRun = pm.plm.runSpeed * (pm.reversed ? -1 : 1);
            pm.plm.Entity.UpdateInternalMovement(pm.transform.forward * invertRun * pm.PlayerTimeScale);
            if (pm.plm.Entity.Velocity != Vector3.zero) // Checks velocity to be sure player is actually running
            {
                pm.plm.stamina = Mathf.Max(
                        pm.plm.stamina - pm.plm.staminaDrop * Time.deltaTime * pm.PlayerTimeScale,
                        0f
                    );
            }
            float audManTime = audMan.audioDevice.time;
            switch (reachedThresholdState)
            {
                case 0:
                    if (audManTime >= smokeOutTreshold)
                    {
                        reachedThresholdState = 1;
                        emission.enabled = true;
                        smokeParticles.Emit(Random.Range(1, 5));
                    }
                    break;
                case 1:
                    if (audManTime >= smokeEndThreshold)
                    {
                        reachedThresholdState = 2;
                        emission.enabled = false;
                    }
                    break;
                case 2:
                    if (audManTime < smokeEndThreshold)
                    {
                        reachedThresholdState = 0; // Waits until the audManTime resets
                    }
                    break;
                default: break;
            }

            if (Physics.Raycast(pm.transform.position, pm.transform.forward, out var hit, hitDistanceCheck))
            {
                if (hit.transform.CompareTag("Window") && hit.transform.TryGetComponent<Window>(out var window))
                {
                    if (!window.broken)
                        Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audBang);
                    window.Break(true);
                }
            }


            yield return null;
        }

        pm.Am.moveMods.Remove(openOnCollisionMod);
        pm.plm.Entity.UpdateInternalMovement(Vector3.zero);
        PlayerMovementPatches.RemoveDisabledPlayer(pm);
        cam.ResetSlideFOVAnimation(fovMod, 2.25f);
        gauge.Deactivate();

        emission.enabled = false;

        audMan.FadeOut(0.5f);

        while (audMan.AnyAudioIsPlaying || smokeParticles.particleCount != 0)
            yield return null;

        Destroy(gameObject);
    }
}