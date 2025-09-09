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
    public float timer = 5f, tintCloudTime = 120f;
    public float cloudRadius = 50f; // 5 tiles
    public float playerEffectDuration = 30f;
    public int explosionRadius = 9;
    public SoundObject hissSound;
    public SoundObject explosionSound;
    public TintedCoverCloud cloudPrefab;
    // public Canvas screenOverlayPrefab;
    public Sprite gaugeSprite;
    // Needs to be static for serialization
    static internal List<Sprite[]> bombTickingSecondSprites = []; // Collection of 3 sprites for each 5 seconds
    [SerializeField]
    internal AnimationComponent animComp;
    DijkstraMap map;

    // SIDE-NOTE: It has a screen overlay feature that's commented out since there's currently no asset from the creator of this item for it.

    protected override void VirtualSetupPrefab(ItemObject itm)
    {
        base.VirtualSetupPrefab(itm);
        for (int i = 5; i >= 1; i--)
            bombTickingSecondSprites.Add(this.GetSpriteSheet($"TintedPaintBomb_World_{i}Sec.png", 2, 2, 65f).RemoveEmptySprites());

        animComp = gameObject.AddComponent<AnimationComponent>();
        animComp.animation = bombTickingSecondSprites[0];
        animComp.speed = 10f;
        animComp.renderers = [GetComponentInChildren<SpriteRenderer>()];
        animComp.renderers[0].sprite = bombTickingSecondSprites[0][0];

        gaugeSprite = this.GetSprite("TintedPaintBomb_GaugeIcon.png", 1);
        cloudPrefab = ParticleExtensions.GetRawChalkParticleGenerator()
                .gameObject.SwapComponent<CoverCloud, TintedCoverCloud>();
        cloudPrefab.name = "TintedCloud";
        var cloudRenderer = cloudPrefab.particelRenderer.GetComponent<ParticleSystemRenderer>();
        var newTexture = Instantiate(LotOfItemsPlugin.assetMan.Get<Texture2D>("DustCloudTexture"));
        newTexture.name = "TintedCloud";

        cloudRenderer.material = new Material(cloudRenderer.material)
        {
            name = "TintedCloudMat",
            mainTexture = TextureExtensions.ApplyLightLevel(newTexture, -90f)
        };

        hissSound = GenericExtensions.FindResourceObjectByName<SoundObject>("Fuse");
        explosionSound = LotOfItemsPlugin.assetMan.Get<SoundObject>("aud_explode");

        endHeight = 1f;

        // screenOverlayPrefab = ObjectCreationExtensions.CreateCanvas();
        // screenOverlayPrefab.name = "TintedBombCanvasPrefab";
        // screenOverlayPrefab.gameObject.ConvertToPrefab(true);

        // var img = ObjectCreationExtensions.CreateImage(screenOverlayPrefab, this.GetSprite());
        // img.name = "TintedBombCover";
    }

    public override bool Use(PlayerManager pm)
    {
        animComp.Initialize(pm.ec);
        animComp.Pause(true);
        return base.Use(pm);
    }

    internal override void OnFloorHit()
    {
        map = new(ec, PathType.Nav, explosionRadius, transform);
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
                animComp.animation = bombTickingSecondSprites[System.Math.Min(currentIndex, bombTickingSecondSprites.Count - 1)];
                animComp.ResetFrame(true);
            }
            yield return null;
        }

        audioManager.FlushQueue(true);
        audioManager.PlaySingle(explosionSound);
        animComp.renderers[0].enabled = false;
        animComp.enabled = false;

        // Create permanent fog clouds
        map.Calculate(explosionRadius, true, [IntVector2.GetGridPosition(transform.position)]);
        foreach (var cell in map.FoundCells())
        {
            TintedCoverCloud cloud = Instantiate(cloudPrefab, cell.CenterWorldPosition, Quaternion.identity, ec.transform);
            cloud.Ec = ec;
            cloud.owner = this;
            cloud.StartEndTimer(tintCloudTime);
            activeTintedCoverClouds++;
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
        entity.SetFrozen(true);
        entity.SetInteractionState(false);
        entity.SetVisible(false);
        while (activePlayerScreens != 0 || audioManager.AnyAudioIsPlaying || activeTintedCoverClouds != 0) // Wait for the player effects to expire out
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

    internal override bool VirtualEnd()
    {
        ec.RemoveFog(fog);
        return base.VirtualEnd();
    }

    int activePlayerScreens = 0;
    int activeTintedCoverClouds = 0;
    internal void MarkTintedCloudOver() => activeTintedCoverClouds--;
    internal void ActivateTintedFog()
    {
        if (++fogActivations == 1)
        {
            if (fogCoroutine != null)
                StopCoroutine(fogCoroutine);
            fogCoroutine = StartCoroutine(FadeInFog());
        }
    }
    internal void DeactivateTintedFog()
    {
        if (fogActivations > 0)
        {
            if (--fogActivations == 0)
            {
                if (fogCoroutine != null)
                    StopCoroutine(fogCoroutine);
                fogCoroutine = StartCoroutine(FadeOutFog());
            }
        }
    }
    int fogActivations = 0;
    readonly Fog fog = new()
    {
        color = Color.black,
        maxDist = 65f,
        startDist = 5f
    };
    Coroutine fogCoroutine;
    IEnumerator FadeInFog()
    {
        ec.AddFog(fog);
        while (fog.strength < 1f)
        {
            fog.strength += 0.25f * Time.deltaTime;
            ec.UpdateFog();
            yield return null;
        }
        fog.strength = 1f;
        ec.UpdateFog();
    }

    IEnumerator FadeOutFog()
    {
        while (fog.strength > 0f)
        {
            fog.strength -= 0.25f * Time.deltaTime;
            ec.UpdateFog();
            yield return null;
        }
        fog.strength = 0f;
        ec.RemoveFog(fog);
    }
}

public class TintedCoverCloud : CoverCloud
{
    bool hasActiveFog = false;
    public ITM_TintedPaintBomb owner;


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
            owner.ActivateTintedFog();
        }
        if (!foundPlayer && hasActiveFog)
        {
            hasActiveFog = false;
            owner.DeactivateTintedFog();
        }

        if (ec.CurrentEventTypes.Contains(RandomEventType.Flood))
        {
            Destroy(gameObject); // Get cleaned out from the world
        }
    }



    private new void OnDestroy()
    {
        while (entities.Count > 0)
        {
            entities[0].SetHidden(value: false);
            entities.RemoveAt(0);
        }
        owner.MarkTintedCloudOver();
    }
}


