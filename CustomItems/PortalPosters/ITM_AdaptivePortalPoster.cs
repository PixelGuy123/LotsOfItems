using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using MTM101BaldAPI.Registers;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.PortalPosters;

public class ITM_AdaptivePortalPoster : Item, IItemPrefab
{
    private EnvironmentController ec;
    private readonly static HashSet<Character> threatCharacters = [Character.Baldi, Character.Principal, Character.Sweep];

    public static void AddThreateningCharacter(Character c) => threatCharacters.Add(c);

    [SerializeField]
    internal AudioManager audMan;
    [SerializeField]
    internal SoundObject audPlace, audTeleport, audDestroy, audNo;
    [SerializeField]
    internal SpriteRenderer portalRenderer;
    [SerializeField]
    internal BoxCollider triggerCollider;

    DijkstraMap threatMap;
    readonly List<Transform> threatningNPCs = [];
    Direction placedDir;

    public void SetupPrefab(ItemObject itm)
    {
        portalRenderer = ObjectCreationExtensions.CreateSpriteBillboard(this.GetSprite("AdaptivePortalPoster_Portal.png", 25f), false);
        portalRenderer.transform.SetParent(transform);
        portalRenderer.transform.localPosition = Vector3.zero;
        portalRenderer.name = "PortalRenderer";

        triggerCollider = gameObject.AddComponent<BoxCollider>();
        triggerCollider.center = Vector3.up * 5f;
        triggerCollider.size = new(2f, 2f, 2f);
        triggerCollider.isTrigger = true;
        gameObject.layer = LayerStorage.ignoreRaycast;

        audMan = gameObject.CreatePropagatedAudioManager(50f, 150f);
        audMan.soundOnStart = [LotOfItemsPlugin.assetMan.Get<SoundObject>("WormholeAmbience")];
        audMan.loopOnStart = true;

        var portalPoster = (ITM_PortalPoster)ItemMetaStorage.Instance.FindByEnum(Items.PortalPoster).value.item;
        audPlace = portalPoster.audYes;
        audNo = portalPoster.audNo;
        audTeleport = GenericExtensions.FindResourceObjectByName<SoundObject>("Teleport");
        audDestroy = ((ITM_Scissors)ItemMetaStorage.Instance.FindByEnum(Items.Scissors).value.item).audSnip;
    }

    public void SetupPrefabPost() { }

    public override bool Use(PlayerManager pm)
    {
        if (!Physics.Raycast(pm.transform.position, Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, out var hit, pm.pc.reach, pm.pc.ClickLayers, QueryTriggerInteraction.Ignore) || !hit.transform.CompareTag("Wall"))
        {
            Destroy(gameObject);
            Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audNo);
            return false;
        }

        placedDir = Directions.DirFromVector3(hit.transform.forward, 5f);
        Cell cell = pm.ec.CellFromPosition(IntVector2.GetGridPosition(hit.transform.position - hit.transform.forward * 5f));

        if (!cell.Null && cell.HasWallInDirection(placedDir) && !cell.WallHardCovered(placedDir))
        {
            ec = pm.ec;
            transform.position = cell.CenterWorldPosition + placedDir.ToVector3() * 4.9f;
            transform.rotation = placedDir.ToRotation();
            cell.SoftCoverWall(placedDir);
            Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audPlace);
            TryUpdateThreatningNPCsCountAndThreatMap();

            return true;
        }

        Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audNo);
        Destroy(gameObject);
        return false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger) return;

        if (other.CompareTag("Player"))
        {
            PlayerManager pm = other.GetComponent<PlayerManager>();
            TeleportToSafety(pm);
        }
        else if (other.CompareTag("NPC"))
        {
            NPC npc = other.GetComponent<NPC>();
            if (threatCharacters.Contains(npc.Character))
            {
                DestroyPortal();
            }
        }
    }

    private void TeleportToSafety(PlayerManager pm)
    {
        TryUpdateThreatningNPCsCountAndThreatMap();

        if (threatningNPCs.Count == 0)
        {
            pm.Teleport(ec.RandomCell(false, false, true).FloorWorldPosition);
            audMan.PlaySingle(audTeleport);
            return;
        }

        List<Cell> safeCells = ec.AllTilesNoGarbage(false, false);
        safeCells.RemoveAll(c => c.room.type != RoomType.Hall && !c.room.entitySafeCells.Contains(c.position));

        Cell safestCell = null;
        int highestScore = -1;

        for (int i = 0; i < safeCells.Count; i++)
        {
            var cell = safeCells[i];
            int currentScore = threatMap.Value(cell.position);

            if (currentScore > highestScore)
            {
                highestScore = currentScore;
                safestCell = cell;
            }
        }

        if (safestCell != null)
        {
            pm.Teleport(safestCell.FloorWorldPosition);
            audMan.PlaySingle(audTeleport);
        }
    }

    private void DestroyPortal()
    {
        audMan.FlushQueue(true);
        audMan.PlaySingle(audDestroy);
        Cell cell = ec.CellFromPosition(transform.position);
        cell.UncoverSoftWall(placedDir);

        portalRenderer.enabled = false;
        triggerCollider.enabled = false;
        StartCoroutine(WaitAndDestroy());
    }

    void TryUpdateThreatningNPCsCountAndThreatMap()
    {
        if (threatningNPCs.Count == ec.Npcs.Count) return;

        threatningNPCs.Clear();
        for (int i = 0; i < ec.Npcs.Count; i++)
        {
            if (threatCharacters.Contains(ec.Npcs[i].Character))
                threatningNPCs.Add(ec.Npcs[i].transform);
        }

        threatMap?.Deactivate(); // Deactivate current instance before going to a new threat map.

        threatMap = new(ec, PathType.Nav, int.MaxValue, [.. threatningNPCs]);
        threatMap.Activate();

    }

    private IEnumerator WaitAndDestroy()
    {
        while (audMan.AnyAudioIsPlaying)
            yield return null;
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        threatMap?.Deactivate(); // Failsafe
    }
}