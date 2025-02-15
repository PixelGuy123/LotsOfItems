using UnityEngine;
using PixelInternalAPI.Extensions;
using LotsOfItems.ItemPrefabStructures;
using System.Collections;

namespace LotsOfItems.CustomItems.PortalPosters;
public class ITM_PortalDoor : Item, IItemPrefab, IClickable<int>
{
	[SerializeField] 
	private SoundObject audTeleport, audNoHere, audOpen, audClose;

	[SerializeField]
	internal AudioManager audman;

	[SerializeField]
	internal SpriteRenderer portalSprite;

	[SerializeField]
	internal Sprite closed, open;

	[SerializeField]
	internal float defaultShutTime = 5f;

	[SerializeField]
	internal int usesBeforeDying = 3, noiseVal = 16;

	[SerializeField]
	internal ItemObject doorPre;

	public ITM_PortalDoor Linkage { get; private set; }

	public Direction PlacedDirection { get; private set; }

	EnvironmentController ec;
	int uses;
	Coroutine closeCor;

	public bool IsDead { get; private set; } = false;
	public bool IsOpen => portalSprite.sprite == open;

	public void SetupPrefab(ItemObject itm)
	{
		audman = gameObject.CreatePropagatedAudioManager(125f, 155f);
		audOpen = GenericExtensions.FindResourceObjectByName<SoundObject>("Doors_StandardOpen");
		audClose = GenericExtensions.FindResourceObjectByName<SoundObject>("Doors_StandardShut");
		audTeleport = GenericExtensions.FindResourceObjectByName<SoundObject>("Teleport");
		audNoHere = GenericExtensions.FindResourceObjectByName<SoundObject>("ErrorMaybe");

		// Create portal visual
		var sprs = this.GetSpriteSheet("PortalDoor_World.png", 2, 1, 25f);
		portalSprite = ObjectCreationExtensions.CreateSpriteBillboard(sprs[0], false);
		portalSprite.transform.SetParent(transform);
		portalSprite.transform.localPosition = Vector3.zero;
		portalSprite.name = "PortalDoor";

		closed = sprs[0];
		open = sprs[1];

		var boxCollider = gameObject.AddComponent<BoxCollider>();
		boxCollider.isTrigger = true;
		boxCollider.size = new(4.5f, 5f, 0.7f);
		boxCollider.center = Vector3.back * 0.35f;

		doorPre = itm;
	}

	public void SetLinkage(ITM_PortalDoor link) 
	{
		if (link == Linkage)
			return;

		Linkage = link;
		link.SetLinkage(this);
	}

	public void Spawn(EnvironmentController ec, Cell cell, Direction direction)
	{
		uses = usesBeforeDying;
		this.ec = ec;

		transform.position = cell.CenterWorldPosition + direction.ToVector3() * 4.98f;
		transform.rotation = direction.ToRotation();
		PlacedDirection = direction;

		audman.PlaySingle(audClose);
	}

	public override bool Use(PlayerManager pm)
	{
		if (!Physics.Raycast(pm.transform.position, Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, out var hit, pm.pc.reach, pm.pc.ClickLayers, QueryTriggerInteraction.Ignore) || !hit.transform.CompareTag("Wall"))
		{
			Destroy(gameObject);
			return false;
		}

		Direction direction = Directions.DirFromVector3(hit.transform.forward, 5f);
		Cell cell = pm.ec.CellFromPosition(IntVector2.GetGridPosition(hit.transform.position - hit.transform.forward * 5f));
		if (!cell.Null && cell.HasWallInDirection(direction) && !cell.WallHardCovered(direction))
		{
			Spawn(pm.ec, cell, direction);

			var cells = ec.AllTilesNoGarbage(false, false);
			cells.Remove(cell);

			if (cells.Count == 0)
				return true; // Just not make a linkage if not possible

			var linkCell = cells[Random.Range(0, cells.Count)];

			var myLinkage = Instantiate((ITM_PortalDoor)doorPre.item);
			myLinkage.SetLinkage(this);
			myLinkage.Spawn(ec, linkCell, linkCell.RandomUncoveredDirection(new()));
			myLinkage.name = name + "_Link";
			return true;
		}

		Destroy(gameObject);
		Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audNoHere);

		return false;
	}

	public void SetupPrefabPost() { }

	IEnumerator CloseEnum()
	{
		if (!IsOpen)
		{
			portalSprite.sprite = open;
			audman.PlaySingle(audOpen);
		}
		float t = defaultShutTime;
		while (t > 0f)
		{
			t -= ec.EnvironmentTimeScale * Time.deltaTime;
			yield return null;
		}
	}

	IEnumerator DieSequence()
	{
		float t = defaultShutTime * 1.5f;
		while (t > 0f)
		{
			t -= ec.EnvironmentTimeScale * Time.deltaTime;
			yield return null;
		}
		t = 1f;
		float s = 1.5f;
		while (t > 0f)
		{
			s += ec.EnvironmentTimeScale * Time.deltaTime;
			t -= s * ec.EnvironmentTimeScale * Time.deltaTime;
			if (t < 0f)
			{
				Destroy(gameObject);
				yield break;
			}
			transform.localScale = Vector3.one * t;
			yield return null;
		}
	}

	void Open()
	{
		if (IsDead)
			return;

		if (closeCor != null)
			StopCoroutine(closeCor);
		closeCor = StartCoroutine(CloseEnum());
	}

	public void Close()
	{
		if (closeCor != null)
			StopCoroutine(closeCor);
		if (IsOpen)
		{
			audman.PlaySingle(audClose);
			portalSprite.sprite = closed;
		}
	}

	public void Clicked(int player)
	{
		if (IsDead)
			return;

		if (!IsOpen)
			ec.MakeNoise(transform.position, noiseVal);
		Open();
	}
	public bool ClickableHidden() => IsDead;
	public bool ClickableRequiresNormalHeight() => false;
	public void ClickableSighted(int player) { }
	public void ClickableUnsighted(int player) { }

	public void Die() 
	{
		if (IsDead)
			return;
		IsDead = true;
		Close();
		StartCoroutine(DieSequence());
		Linkage?.Die();
	}

	public void Teleport() =>
		audman.PlaySingle(audTeleport);
	

	void OnTriggerEnter(Collider other)
	{
		if (IsDead)
			return;

		if (other.isTrigger)
		{
			if (other.CompareTag("NPC"))
				Open();

			if (IsOpen && other.TryGetComponent<Entity>(out var entity))
			{
				if (--uses <= 0)
					Die();
				else
					Open();
				
				Teleport();

				if (!Linkage)
				{
					var cells = ec.AllTilesNoGarbage(false, false);

					if (cells.Count == 0)
						throw new System.ArgumentOutOfRangeException("No available cell for the door (should be impossible).");

					var cell = cells[Random.Range(0, cells.Count)];

					entity.Teleport(cell.FloorWorldPosition);
					return;
				}
				Linkage.Teleport();
				Linkage.Open();
				var oppositeDirection = Linkage.PlacedDirection.GetOpposite();

				entity.Teleport(Linkage.transform.position + oppositeDirection.ToVector3() * 4f);
				entity.transform.rotation = oppositeDirection.ToRotation();
			}
		}


	}
}