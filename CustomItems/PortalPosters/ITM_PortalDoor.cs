using UnityEngine;
using PixelInternalAPI.Extensions;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Components;
using PixelInternalAPI.Classes;

namespace LotsOfItems.CustomItems.PortalPosters;
public class ITM_PortalDoor : Item, IItemPrefab
{
	[SerializeField]
	private Camera portalCam;
	[SerializeField]
	private RenderTexture renderTex;
	[SerializeField]
	private MeshRenderer portalSprite;

	[SerializeField] 
	private SoundObject audTeleport, audNoHere, audInsert;

	[SerializeField]
	internal AudioManager audman;

	BasicLookerInstance looker;

	public void SetupPrefab(ItemObject itm)
	{
		audman = gameObject.CreatePropagatedAudioManager(65f, 75f);
		audInsert = GenericExtensions.FindResourceObjectByName<SoundObject>("Doors_StandardOpen");
		audTeleport = GenericExtensions.FindResourceObjectByName<SoundObject>("Teleport");
		audNoHere = GenericExtensions.FindResourceObjectByName<SoundObject>("ErrorMaybe");

		// Create portal visual
		portalSprite = GameObject.CreatePrimitive(PrimitiveType.Quad).GetComponent<MeshRenderer>();
		portalSprite.name = "PortalDoorQuad";
		portalSprite.transform.SetParent(transform);
		portalSprite.transform.localPosition = Vector3.zero;
		portalSprite.receiveShadows = false;
		portalSprite.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		Destroy(portalSprite.GetComponent<MeshCollider>());
		

		// Setup render texture
		renderTex = new RenderTexture(256, 256, 16)
		{
			isPowerOfTwo = false,
			memorylessMode = RenderTextureMemoryless.Depth | RenderTextureMemoryless.MSAA,
			antiAliasing = 1,
			dimension = UnityEngine.Rendering.TextureDimension.Tex2D,
			name = "PortalDoorRenderTex"
		};
		portalCam = new GameObject("PortalCamera").AddComponent<Camera>();
		portalCam.transform.SetParent(transform);
		portalCam.targetTexture = renderTex;
		portalCam.cullingMask = LayerStorage.billboardLayer;
		portalCam.useOcclusionCulling = false;
		portalCam.enabled = false;

		var boxCollider = gameObject.AddComponent<BoxCollider>();
		boxCollider.isTrigger = true;
		boxCollider.size = new(2.5f, 5f, 0.1f);

		// add texture to material
		portalSprite.material.SetTexture("_MainTex", renderTex);
	}

	public override bool Use(PlayerManager pm)
	{
		transform.position = pm.transform.position + pm.transform.forward * 5f; // temporary spawn
		transform.LookAt(pm.transform);

		var cells = pm.ec.AllTilesNoGarbage(false, false);
		for (int i = 0; i < cells.Count; i++)
		{
			if (cells[i].shape == TileShapeMask.Closed)
				cells.RemoveAt(i--);
		}

		if (cells.Count == 0)
			throw new System.ArgumentOutOfRangeException("No available cell for the door (should be impossible).");

		var cell = cells[Random.Range(0, cells.Count)];
		portalCam.transform.position = cell.CenterWorldPosition;
		portalCam.transform.rotation = cell.RandomUnoccupiedDirection(cell.constBin, new()).ToRotation();
		portalCam.enabled = true;

		//MaterialModifier.ChangeHole(quad.GetComponent<MeshRenderer>(), this.mask, quad.GetComponent<MeshRenderer>().materials[1]);
		

		audman.PlaySingle(audInsert);

		return true;
	}

	void OnDestroy() =>
		renderTex.Release();

	public void SetupPrefabPost() { }

	void OnTriggerEnter(Collider other)
	{
		if (other.isTrigger && other.TryGetComponent<Entity>(out var entity))
		{
			entity.Teleport(portalCam.transform.position + portalCam.transform.forward * 2f);
			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audTeleport);
		}
	}

	
}