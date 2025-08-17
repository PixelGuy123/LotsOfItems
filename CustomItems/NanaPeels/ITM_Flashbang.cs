using System.Collections;
using System.Collections.Generic;
using LotsOfItems.Components;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using MTM101BaldAPI;
using PixelInternalAPI.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace LotsOfItems.CustomItems.NanaPeels;

public class ITM_Flashbang : ITM_GenericNanaPeel
{
    public float blindDuration = 30f;
    public float explosionRadius = 100f; // 10 tiles
    public float timer = 5f, blindTimer = 15f;
    public SoundObject explosionSound;
    public Sprite[] explosionAnimation;
    [SerializeField]
    internal SpriteRenderer renderer;
    [SerializeField]
    internal Canvas blindCanvas;
    public LayerMask explosionLayer = LotOfItemsPlugin.onlyNpcPlayerLayers;
    public float animationSpeed = 12f;
    BasicLookerInstance lookerInstance;

    protected override void VirtualSetupPrefab(ItemObject itm)
    {
        base.VirtualSetupPrefab(itm);
        renderer = GetComponentInChildren<SpriteRenderer>();
        renderer.sprite = itm.itemSpriteLarge.DuplicateItself(renderer.sprite.pixelsPerUnit);
        renderer.sprite.name = $"{itm.itemSpriteLarge.name}_World";

        explosionSound = this.GetSound("Flashbang_explode.wav", "LtsOItems_Vfx_Explode", SoundType.Effect, Color.white);

        blindCanvas = ObjectCreationExtensions.CreateCanvas();
        blindCanvas.name = "FlashbangCanvas";
        blindCanvas.gameObject.ConvertToPrefab(false);

        var image = ObjectCreationExtensions.CreateImage(blindCanvas, LotOfItemsPlugin.assetMan.Get<Texture2D>("tex_white"), true);
        image.name = "FlashbangImage";

        explosionAnimation = this.GetSpriteSheet("Flashbang_Explosion.png", 4, 3, 25f);
    }

    public override bool Use(PlayerManager pm)
    {
        lookerInstance = new(transform, pm.ec);
        return base.Use(pm);
    }

    internal override void OnFloorHit() =>
        StartCoroutine(ExplosionSequence());


    private IEnumerator ExplosionSequence()
    {
        yield return new WaitForSecondsEnvironmentTimescale(ec, timer);

        audioManager.PlaySingle(explosionSound);
        StartCoroutine(AnimateExplosion());

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, explosionLayer);
        foreach (Collider col in colliders)
        {
            Entity entity = col.GetComponent<Entity>();
            if (entity != null && PhysicsManager.RaycastEntity(entity, transform.position, explosionLayer, QueryTriggerInteraction.Ignore, ec.MaxRaycast, false))
            {
                if (entity.CompareTag("Player") && entity.TryGetComponent<PlayerManager>(out var pm) && lookerInstance.IsVisible)
                    StartCoroutine(ApplyPlayerEffect(pm));
                else if (entity.CompareTag("NPC") && entity.TryGetComponent<NPC>(out var npc) && !npc.Blinded && npc.looker && npc.looker.isActiveAndEnabled)
                    StartCoroutine(ApplyEffect(entity));
            }
        }
    }

    void DeaffectEntity(Entity entity)
    {
        if (!entity) return;
        entity.SetBlinded(false);
        entity.ExternalActivity.moveMods.Remove(blindMod);
        foreach (var otherNpc in ec.Npcs)
        {
            if (otherNpc.Navigator.Entity != entity)
                otherNpc.Navigator.Entity.IgnoreEntity(entity, false);
        }
        foreach (var player in ec.Players)
        {
            if (player)
                player.plm.Entity.IgnoreEntity(entity, false);
        }
    }

    void AffectEntity(Entity entity)
    {
        if (!entity) return;
        entity.SetBlinded(true);
        entity.ExternalActivity.moveMods.Add(blindMod);
        foreach (var otherNpc in ec.Npcs)
        {
            if (otherNpc.Navigator.Entity != entity)
                otherNpc.Navigator.Entity.IgnoreEntity(entity, true);
        }
        foreach (var player in ec.Players)
        {
            if (player)
                player.plm.Entity.IgnoreEntity(entity, true);
        }
    }

    private IEnumerator AnimateExplosion()
    {
        entity.SetFrozen(true);
        entity.SetInteractionState(false);
        float frame = 0f;
        while (frame < explosionAnimation.Length)
        {
            renderer.sprite = explosionAnimation[Mathf.Min(explosionAnimation.Length - 1, Mathf.FloorToInt(frame))];
            frame += Time.deltaTime * ec.EnvironmentTimeScale * animationSpeed;
            yield return null;
        }

        renderer.enabled = false;
        while (EffectActive)
            yield return null;

        Destroy(gameObject);
    }

    private IEnumerator ApplyEffect(Entity entity)
    {
        activeEffects++;

        affectedEntities.Add(entity);
        AffectEntity(entity);
        yield return new WaitForSecondsEnvironmentTimescale(ec, blindDuration);
        DeaffectEntity(entity);
        affectedEntities.Remove(entity);

        activeEffects--;
    }

    private IEnumerator ApplyPlayerEffect(PlayerManager player)
    {
        activeEffects++;
        var canvas = Instantiate(blindCanvas);
        var image = canvas.GetComponentInChildren<Image>();

        canvas.transform.SetParent(transform);
        canvas.worldCamera = Singleton<CoreGameManager>.Instance.GetCamera(player.playerNumber).canvasCam;
        canvas.gameObject.SetActive(true);

        float t = 0f;
        float fadeInTime = blindTimer * 0.1f;
        float fadeOutTime = 0.9f * blindTimer;
        float totalTime = blindTimer;

        Color clearWhite = Color.white;
        clearWhite.a = 0f;

        if (Singleton<PlayerFileManager>.Instance.reduceFlashing)
        {
            totalTime -= fadeInTime;
            while (t < fadeInTime)
            {
                t += ec.EnvironmentTimeScale * Time.deltaTime;
                image.color = Color.Lerp(clearWhite, Color.white, t / fadeInTime);
                yield return null;
            }
        }
        totalTime -= fadeOutTime; // Subtracts the fadeOutTime, which is way longer

        while (totalTime > 0f)
        {
            totalTime -= ec.EnvironmentTimeScale * Time.deltaTime;
            yield return null;
        }

        t = 0f;
        while (t < fadeOutTime)
        {
            t += ec.EnvironmentTimeScale * Time.deltaTime;
            image.color = Color.Lerp(Color.white, clearWhite, t / fadeOutTime);
            yield return null;
        }

        Destroy(canvas.gameObject);
        activeEffects--;
    }

    internal override bool VirtualEnd()
    {
        foreach (var entity in affectedEntities)
            DeaffectEntity(entity);

        return base.VirtualEnd();
    }
    // To make sure this item does not behave as a 'Nana Peel
    internal override bool OnCollisionOverride(RaycastHit hit) => false;
    internal override bool EntityTriggerStayOverride(Collider other) => false;

    int activeEffects = 0;
    public bool EffectActive => activeEffects != 0;
    readonly MovementModifier blindMod = new(Vector3.zero, 0f);
    readonly List<Entity> affectedEntities = [];
}