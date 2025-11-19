using System.Collections;
using System.Collections.Generic;
using LotsOfItems.Plugin;
using MTM101BaldAPI;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.Components;

// Copy paste of Wormhole Controller, but in miniature version
public class MiniBlackHole : EnvironmentObject
{
    public static MiniBlackHole CreatePrefab(float baseLifeTime = -1f, int entityLimit = -1)
    {
        var wormholeReference = GenericExtensions.FindResourceObject<WormholeController>();

        var indicatorPrefab = wormholeReference.indicatorPrefab.SafeDuplicatePrefab(true);
        indicatorPrefab.gameObject.ConvertToPrefab(true);
        indicatorPrefab.name = "MiniBlackHole";

        // Creates MiniBlackhole
        var miniBlackHole = indicatorPrefab.gameObject.AddComponent<MiniBlackHole>();
        miniBlackHole.audIntoWormHole = wormholeReference.audIntoWormhole;
        miniBlackHole.audTeleport = wormholeReference.audTeleport;
        miniBlackHole.audioManager = indicatorPrefab.audioManager;
        if (baseLifeTime > 0f)
            miniBlackHole.lifeTime = baseLifeTime;
        if (entityLimit > 0)
            miniBlackHole.entityLimit = entityLimit;
        miniBlackHole.indicatorPrefab = wormholeReference.indicatorPrefab;
        miniBlackHole.setPulseTime = wormholeReference.setPulseTime;

        miniBlackHole.gameObject.layer = LayerStorage.ignoreRaycast;
        var collider = miniBlackHole.gameObject.AddComponent<SphereCollider>();
        collider.radius = 1f;
        collider.isTrigger = true;

        Destroy(indicatorPrefab); // Destroys this component since it is unneeded

        return miniBlackHole;
    }
    private void Update()
    {
        pulseTime -= Time.deltaTime * ec.EnvironmentTimeScale;
        if (pulseTime <= 0f)
        {
            pulseTime += setPulseTime;
        }

        lifeTime -= ec.EnvironmentTimeScale * Time.deltaTime;
        if (lifeTime <= 0f || (entityLimit <= teleportedEntityCount && !audioManager.AnyAudioIsPlaying))
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger || teleportedEntityCount >= entityLimit) return;

        Entity component = other.GetComponent<Entity>();
        if (component != null)
        {
            EntityOverrider entityOverrider = new();
            if (component.Override(entityOverrider))
            {
                component.Teleport(transform.position);
                Cell cell = ec.RandomCell(false, false, true);
                overriders.Add(entityOverrider);

                entityOverrider.SetFrozen(true);
                entityOverrider.SetInteractionState(false);
                entityOverrider.SetVisible(false);
                StartCoroutine(TeleportEntity(cell.CenterWorldPosition, entityOverrider, 3f));
                entityOverrider.SetFrozen(true);
                audioManager.PlaySingle(audIntoWormHole);
            }
        }
    }

    private IEnumerator TeleportEntity(Vector3 destination, EntityOverrider overrider, float time)
    {
        teleportedEntityCount++;
        WormholeIndicator wormholeIndicator = Instantiate(indicatorPrefab);
        wormholeIndicator.Initialize(ec, time);
        wormholeIndicator.transform.position = destination;
        while (time > 0f)
        {
            time -= Time.deltaTime;
            yield return null;
        }
        overrider.entity.Teleport(destination);
        overrider.entity.AddForce(new Force(overrider.entity.transform.forward * -1f, 7f, -5f));
        yield return null;
        overrider.Release();
        audioManager.PlaySingle(audTeleport);
        yield break;
    }

    void OnDestroy()
    {
        foreach (var ov in overriders)
            ov.Release();
    }

    int teleportedEntityCount = 0;
    readonly List<EntityOverrider> overriders = [];

    [SerializeField]
    internal WormholeIndicator indicatorPrefab;

    [SerializeField]
    internal AudioManager audioManager;

    [SerializeField]
    internal SoundObject audTeleport, audIntoWormHole;

    [SerializeField]
    internal float setPulseTime = 2.5f, lifeTime = 10f;

    [SerializeField]
    internal float pushSpeed = 2f;

    [SerializeField]
    internal int entityLimit = 5;

    [SerializeField]
    internal float pullSpeed = 5f;

    private float pulseTime;
}