using System.Collections;
using LotsOfItems.Components;
using LotsOfItems.ItemPrefabStructures;
using MTM101BaldAPI;
using MTM101BaldAPI.Registers;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.ChalkErasers;

public class ITM_ChalkBox : Item, IItemPrefab
{
    [SerializeField]
    private float duration = 30f;
    [SerializeField]
    private SoundObject audChalk, audFail;
    [SerializeField]
    internal ChalkedDoor chalkPre;

    public void SetupPrefab(ItemObject itm)
    {
        var chalkObject = new GameObject("ChalkedDoorObject");
        chalkObject.ConvertToPrefab(true);

        chalkPre = chalkObject.AddComponent<ChalkedDoor>();

        var chalkOverlayMaterialClosed = new Material(GenericExtensions.FindResourceObject<StandardDoor>().overlayShut[0])
        {
            mainTexture = this.GetTexture("ChalkBox_ClosedOverlay.png"),
            name = "ChalkedDoorOverlay"
        };
        var chalkOverlayMaterialOpen = new Material(GenericExtensions.FindResourceObject<StandardDoor>().overlayOpen[0])
        {
            mainTexture = this.GetTexture("ChalkBox_OpenOverlay.png"),
            name = "ChalkedDoorOverlay"
        };
        chalkPre.chalkOverlay = [chalkOverlayMaterialClosed, chalkOverlayMaterialOpen];
        chalkPre.meshRenderers = [CreateQuad(false), CreateQuad(true)];
        chalkPre.colliders = new MeshCollider[chalkPre.meshRenderers.Length];
        for (int i = 0; i < chalkPre.colliders.Length; i++)
            chalkPre.colliders[i] = chalkPre.meshRenderers[i].GetComponent<MeshCollider>();

        chalkPre.UpdateTextures(chalkPre.chalkOverlay[0]);

        audChalk = (ItemMetaStorage.Instance.FindByEnum(Items.ChalkEraser).value.item as ChalkEraser).audUse;
        audFail = GenericExtensions.FindResourceObjectByName<SoundObject>("ErrorMaybe");

        MeshRenderer CreateQuad(bool isBackOne)
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.transform.SetParent(chalkObject.transform);
            quad.layer = LayerStorage.blockRaycast;
            quad.transform.localPosition = Vector3.forward * (isBackOne ? -0.01f : 0.01f);
            quad.transform.localRotation = isBackOne ? Quaternion.identity : Quaternion.Euler(0f, 180f, 0f);
            quad.transform.localScale = Vector3.one * 10f;
            quad.name = "ChalkRenderer_" + (isBackOne ? "Front" : "Back");
            return quad.GetComponent<MeshRenderer>();
        }
    }

    public void SetupPrefabPost() { }

    public override bool Use(PlayerManager pm)
    {
        Destroy(gameObject);

        if (Physics.Raycast(pm.transform.position, Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, out RaycastHit hit, pm.pc.reach, pm.pc.ClickLayers))
        {
            StandardDoor door = hit.transform.GetComponent<StandardDoor>();
            if (door)
            {
                var chalked = Instantiate(chalkPre);
                chalked.Apply(door, duration, pm);
                Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audChalk);
                return true;
            }
        }

        Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audFail);
        return false;
    }
}

public class ChalkedDoor : MonoBehaviour
{
    private StandardDoor door;
    [SerializeField]
    internal Material[] chalkOverlay; // 0 is closed, 1 is open
    [SerializeField]
    internal MeshRenderer[] meshRenderers;
    [SerializeField]
    internal MeshCollider[] colliders;
    private float timer;
    private bool isChalked = false;
    Marker_BlockedStandardDoor blockedDoor;
    bool isOpen = false;

    public void Apply(StandardDoor targetDoor, float duration, PlayerManager user)
    {
        door = targetDoor;
        timer = duration;
        isChalked = true;

        transform.position = targetDoor.doors[0].transform.position;
        transform.forward = targetDoor.doors[0].transform.forward;

        blockedDoor = door.gameObject.AddComponent<Marker_BlockedStandardDoor>();
        door.aTile.Block(door.direction, true);
        door.bTile.Block(door.direction.GetOpposite(), true);
        door.ec.RecalculateNavigation();

        StartCoroutine(RevertAfterTime());
        if (!user) return;

        for (int i = 0; i < colliders.Length; i++) // Prevents collision between the chalk door, to make it passable
            Physics.IgnoreCollision(user.plm.Entity.collider, colliders[i], true);
    }

    private IEnumerator RevertAfterTime()
    {
        float t = timer;
        while (t > 0f)
        {
            t -= door.ec.EnvironmentTimeScale * Time.deltaTime;
            if (door.IsOpen != isOpen)
            {
                isOpen = door.IsOpen;
                UpdateTextures(chalkOverlay[isOpen ? 1 : 0]);
            }
            yield return null;
        }
        Revert();
    }

    internal void UpdateTextures(Material mat)
    {
        for (int i = 0; i < meshRenderers.Length; i++)
            meshRenderers[i].material = mat;
    }

    private void Revert()
    {
        if (!isChalked || !door) return;

        isChalked = false;

        door.aTile.Block(door.direction, false);
        door.bTile.Block(door.direction.GetOpposite(), false);
        door.ec.RecalculateNavigation();

        Destroy(blockedDoor);
        Destroy(this);
    }

    void OnDestroy() =>
        Revert();

}