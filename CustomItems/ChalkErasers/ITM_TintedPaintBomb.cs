using System.Collections;
using System.Collections.Generic;
using LotsOfItems.Components;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using MTM101BaldAPI;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.ChalkErasers;

public class ITM_TintedPaintBomb : ITM_GenericNanaPeel
{
    public float timer = 5f;
    public float cloudRadius = 50f; // 5 tiles
    public float playerEffectDuration = 30f;
    public int explosionRadius = 5;
    public SoundObject hissSound;
    public SoundObject explosionSound;
    public CoverCloud cloudPrefab;
    // public Canvas screenOverlayPrefab;
    public Sprite gaugeSprite;
    [SerializeField]
    internal List<Sprite[]> bombTickingSecondSprites = []; // Collection of 3 sprites for each 5 seconds
    [SerializeField]
    internal AnimationComponent animComp;
    DijkstraMap map;

    // SIDE-NOTE: It has a screen overlay feature that's commented out since there's currently no asset from the creator of this item for it.

    protected override void VirtualSetupPrefab(ItemObject itm)
    {
        base.VirtualSetupPrefab(itm);
        for (int i = 5; i >= 1; i++)
            bombTickingSecondSprites.Add(this.GetSpriteSheet($"TintedPaintBomb_World_{i}Sec.png", 2, 2, 25f).RemoveEmptySprites());

        animComp = gameObject.AddComponent<AnimationComponent>();
        animComp.animation = bombTickingSecondSprites[0];
        animComp.speed = 10f;
        animComp.renderers = [GetComponentInChildren<SpriteRenderer>()];

        gaugeSprite = this.GetSprite("TintedPaintBomb_GaugeIcon.png", 1);
        cloudPrefab = ParticleExtensions.GetRawChalkParticleGenerator()
                .gameObject.SwapComponent<CoverCloud, TintedCoverCloud>();
        cloudPrefab.name = "TintedCloud";
        var cloudRenderer = cloudPrefab.particelRenderer.GetComponent<ParticleSystemRenderer>();
        var newTexture = Instantiate((Texture2D)cloudRenderer.material.mainTexture);
        newTexture.name = "TintedCloud";

        cloudRenderer.material = new Material(cloudRenderer.material)
        {
            name = "TintedCloudMat",
            mainTexture = TextureExtensions.ApplyLightLevel(newTexture, -90f)
        };

        hissSound = GenericExtensions.FindResourceObjectByName<SoundObject>("Fuse");
        explosionSound = LotOfItemsPlugin.assetMan.Get<SoundObject>("aud_explode");

        // screenOverlayPrefab = ObjectCreationExtensions.CreateCanvas();
        // screenOverlayPrefab.name = "TintedBombCanvasPrefab";
        // screenOverlayPrefab.gameObject.ConvertToPrefab(true);

        // var img = ObjectCreationExtensions.CreateImage(screenOverlayPrefab, this.GetSprite());
        // img.name = "TintedBombCover";
    }

    public override bool Use(PlayerManager pm)
    {
        animComp.Initialize(ec);
        animComp.Pause(true);
        return base.Use(pm);
    }

    internal override void OnFloorHit()
    {
        map = new(ec, PathType.Nav, explosionRadius, transform);
        map.Calculate();
        StartCoroutine(DetonationSequence());
    }

    private IEnumerator DetonationSequence()
    {
        audioManager.QueueAudio(hissSound);
        audioManager.SetLoop(true);
        audioManager.maintainLoop = true;

        // Actually play the animation
        animComp.Pause(false);

        float t = 0f;
        int currentIndex = Mathf.FloorToInt(t);
        while (t < timer)
        {
            t += ec.EnvironmentTimeScale * Time.deltaTime;
            int idx = Mathf.FloorToInt(t);
            if (currentIndex != idx)
            {
                currentIndex = idx;
                animComp.animation = bombTickingSecondSprites[Mathf.Min(bombTickingSecondSprites.Count - 1, currentIndex)];
                animComp.ResetFrame(true);
            }
            yield return null;
        }

        audioManager.PlaySingle(explosionSound);
        animComp.renderers[0].enabled = false;
        animComp.enabled = false;

        // Create permanent fog clouds
        foreach (var cell in map.FoundCells())
        {
            CoverCloud cloud = Instantiate(cloudPrefab, cell.CenterWorldPosition, Quaternion.identity, ec.transform);
            cloud.Ec = ec;
        }

        // Apply screen effect to nearby players
        // Collider[] players = Physics.OverlapSphere(transform.position, cloudRadius, LotOfItemsPlugin.onlyNpcPlayerLayers);
        // for (int i = 0; i < players.Length; i++)
        // {
        //     if (!players[i].CompareTag("Player")) continue;

        //     PlayerManager p = players[i].GetComponent<PlayerManager>();
        //     if (p)
        //         StartCoroutine(PlayerScreenEffect(p));
        // }

        while (activePlayerScreens != 0 || audioManager.AnyAudioIsPlaying) // Wait for the player effects to expire out
            yield return null;

        Destroy(gameObject);
    }

    // private IEnumerator PlayerScreenEffect(PlayerManager p)
    // {
    //     activePlayerScreens++;
    //     Canvas overlay = Instantiate(screenOverlayPrefab, transform);
    //     overlay.worldCamera = Singleton<CoreGameManager>.Instance.GetCamera(p.playerNumber).canvasCam;

    //     HudGauge gauge = Singleton<CoreGameManager>.Instance.GetHud(p.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, playerEffectDuration);

    //     float t = playerEffectDuration;
    //     while (t > 0)
    //     {
    //         t -= Time.deltaTime;
    //         gauge?.SetValue(playerEffectDuration, t);
    //         yield return null;
    //     }

    //     gauge?.Deactivate();
    //     Destroy(overlay.gameObject);
    //     activePlayerScreens--;
    // }

    internal override bool EntityTriggerStayOverride(Collider other) => false;

    int activePlayerScreens = 0;
}

public class TintedCoverCloud : CoverCloud
{
    bool hasActiveFog = false;
    readonly Fog fog = new()
    {
        color = Color.black,
    };
    Coroutine fogCoroutine;
    private void FixedUpdate()
    {
        bool foundPlayer = false;
        for (int i = 0; i < entities.Count; i++)
        {
            var e = entities[i];
            if (e.CompareTag("Player"))
            {
                foundPlayer = true;
                break;
            }
        }

        if (foundPlayer && !hasActiveFog)
        {
            hasActiveFog = true;
            if (fogCoroutine != null)
                StopCoroutine(fogCoroutine);
            fogCoroutine = StartCoroutine(FadeInFog());
        }

        if (!foundPlayer && hasActiveFog)
        {
            hasActiveFog = false;
            if (fogCoroutine != null)
                StopCoroutine(fogCoroutine);
            fogCoroutine = StartCoroutine(FadeOutFog());
        }

        if (ec.CurrentEventTypes.Contains(RandomEventType.Flood))
        {
            Destroy(gameObject); // Get cleaned out from the world
        }
    }

    IEnumerator FadeInFog()
    {
        fog.startDist = 5f;
        fog.maxDist = 65f;
        fog.strength = 0f;
        float fogStrength = 0f;
        while (fogStrength < 1f)
        {
            fogStrength += 0.25f * Time.deltaTime;
            fog.strength = fogStrength;
            ec.UpdateFog();
            yield return null;
        }
        fogStrength = 1f;
        fog.strength = fogStrength;
        ec.UpdateFog();
    }

    IEnumerator FadeOutFog()
    {
        fog.strength = 1f;
        float fogStrength = 1f;
        while (fogStrength > 0f)
        {
            fogStrength -= 0.25f * Time.deltaTime;
            fog.strength = fogStrength;
            ec.UpdateFog();
            yield return null;
        }
        ec.RemoveFog(fog);
    }
}


