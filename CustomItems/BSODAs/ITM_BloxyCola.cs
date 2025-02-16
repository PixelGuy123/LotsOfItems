using UnityEngine;
using System.Collections.Generic;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using MTM101BaldAPI;
using LotsOfItems.ItemPrefabStructures;
using MTM101BaldAPI.Registers;

namespace LotsOfItems.CustomItems.BSODAs;
public class ITM_BloxyCola : ITM_GenericBSODA
{
	private StainController stainController;
	private Cell lastRecordedCell;

	[SerializeField]
	internal Stain stainPre;

	[SerializeField]
	internal StainEffector stainEffectorPre;

	protected override void VirtualSetupPrefab(ItemObject itm)
	{
		base.VirtualSetupPrefab(itm);
		spriteRenderer.sprite = this.GetSprite("BloxyCola_soda.png", spriteRenderer.sprite.pixelsPerUnit);
		Destroy(GetComponentInChildren<ParticleSystem>().gameObject);

		var puddleObject = ObjectCreationExtensions.CreateSpriteBillboard(
				this.GetSprite("BloxyCola_stain.png", 10f),
				false
			).AddSpriteHolder(out var puddleSprite, 0.05f, LayerStorage.ignoreRaycast);
		puddleSprite.name = "Sprite";
		puddleSprite.gameObject.layer = 0;
		puddleSprite.transform.Rotate(90, 0, 0); // Face downward
		stainPre = puddleObject.gameObject.AddComponent<Stain>();
		stainPre.gameObject.ConvertToPrefab(true);
		stainPre.name = "Stain";

		var collider = stainPre.gameObject.AddComponent<BoxCollider>();
		collider.isTrigger = true;
		collider.size = new Vector3(4.5f, 1f, 4.5f);
		collider.center = Vector3.up * 2f;

		stainEffectorPre = new GameObject("StainEffector").AddComponent<StainEffector>();
		stainEffectorPre.gameObject.ConvertToPrefab(true);
		stainEffectorPre.entity = stainEffectorPre.gameObject.CreateEntity(2f, 2f);
		stainEffectorPre.entity.collisionLayerMask = ((ITM_NanaPeel)ItemMetaStorage.Instance.FindByEnum(Items.NanaPeel).value.item).entity.collisionLayerMask;

		stainEffectorPre.audMan = stainEffectorPre.gameObject.CreatePropagatedAudioManager(65f, 75f)
			.AddStartingAudiosToAudioManager(true, [GenericExtensions.FindResourceObjectByName<SoundObject>("Nana_Loop")]);
		stainEffectorPre.audHitWall = GenericExtensions.FindResourceObjectByName<SoundObject>("Nana_Sput");
		stainEffectorPre.audStartSlip = GenericExtensions.FindResourceObjectByName<SoundObject>("Nana_Slip");

		time = 30f;
		sound = this.GetSoundNoSub("BloxyCola_use.wav", SoundType.Effect);
	}

	public override bool Use(PlayerManager pm)
	{
		bool val = base.Use(pm);
		stainController = new GameObject("StainController").AddComponent<StainController>();
		stainController.transform.SetParent(transform);
		stainController.Initialize(this);
		lastRecordedCell = ec.CellFromPosition(transform.position);
		return val;
	}
	public override void VirtualUpdate()
	{
		base.VirtualUpdate();

		Cell currentCell = ec.CellFromPosition(transform.position);
		if (!currentCell.Null && currentCell != lastRecordedCell)
		{
			stainController.CreateStain(currentCell);
			lastRecordedCell = currentCell;
		}
	}

	protected override void VirtualEnd()
	{
		if (stainController != null)
			Destroy(stainController.gameObject);

		base.VirtualEnd();
	}
}
public class StainController : MonoBehaviour
{
	internal ITM_BloxyCola owner;
	readonly internal HashSet<Cell> stainedCells = [];
	readonly private List<Stain> activeStains = [];
	readonly internal HashSet<Entity> affectedEntities = [];

	public void Initialize(ITM_BloxyCola soda) =>
		owner = soda;
	

	public void CreateStain(Cell cell)
	{
		if (stainedCells.Contains(cell)) return;

		var stain = Instantiate(owner.stainPre);
		stain.transform.position = cell.FloorWorldPosition;
		stain.Initialize(this);
		activeStains.Add(stain);
		stainedCells.Add(cell);
	}

	public void CreateEffector(Entity target)
	{
		if (affectedEntities.Contains(target)) return;

		// Create invisible effector using NanaPeel logic
		var effector = Instantiate(owner.stainEffectorPre);
		effector.transform.position = target.transform.position;
		effector.Initialize(this, target);
		affectedEntities.Add(target);
	}

	void OnDestroy()
	{
		foreach (Stain stain in activeStains)
			if (stain != null) Object.Destroy(stain.gameObject);
	}
}
public class Stain : MonoBehaviour
{
	private StainController controller;

	public void Initialize(StainController controller) =>
		this.controller = controller;
	

	void OnTriggerEnter(Collider other)
	{
		Entity entity = other.GetComponent<Entity>();
		if (entity != null && entity.Grounded)
			controller.CreateEffector(entity);
	}
}
public class StainEffector : MonoBehaviour, IEntityTrigger
{
	private StainController controller;
	private Entity targetEntity;
	private readonly MovementModifier slipMod = new(Vector3.zero, 0);
	private Vector3 slipDirection;

	[SerializeField]
	internal Entity entity;

	[SerializeField]
	internal float speed = 15f, slipStartForce = 8.5f;

	[SerializeField]
	internal AudioManager audMan;

	[SerializeField]
	internal SoundObject audHitWall, audStartSlip;

	public void Initialize(StainController controller, Entity target)
	{
		this.controller = controller;
		targetEntity = target;
		targetEntity.ExternalActivity.moveMods.Add(slipMod);
		speed += targetEntity.Velocity.magnitude;
		slipDirection = targetEntity.Velocity.normalized;

		entity.Initialize(controller.owner.ec, target.transform.position);
		entity.OnEntityMoveInitialCollision += (hit) =>
		{
			slipDirection = Vector3.Reflect(slipDirection, hit.normal);
			audMan.PlaySingle(audHitWall);
		};

		audMan.PlaySingle(audStartSlip);

		entity.AddForce(new(slipDirection, slipStartForce, -slipStartForce * 0.85f));
		targetEntity.AddForce(new(slipDirection, slipStartForce, -slipStartForce * 0.85f));
	}

	void Update()
	{
		if (controller == null || !IsInCoveredCell())
		{
			DestroyEffector();
			return;
		}

		entity.UpdateInternalMovement(slipDirection * speed * controller.owner.ec.EnvironmentTimeScale);
		slipMod.movementAddend = entity.ExternalActivity.Addend + slipDirection * speed * controller.owner.ec.EnvironmentTimeScale;

	}

	bool IsInCoveredCell()
	{
		Cell currentCell = controller.owner.ec.CellFromPosition(transform.position);
		return controller.stainedCells.Contains(currentCell);
	}

	void DestroyEffector()
	{
		targetEntity?.ExternalActivity.moveMods.Remove(slipMod);
		controller.affectedEntities.RemoveWhere(x => !x || x == targetEntity);
		Destroy(gameObject);
	}

	public void EntityTriggerEnter(Collider other) { }
	public void EntityTriggerStay(Collider other) { }
	public void EntityTriggerExit(Collider other)
	{
		if (other.transform == targetEntity.transform)
		{
			DestroyEffector();
		}
	}
}