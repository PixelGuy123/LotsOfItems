using System.Collections.Generic;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using MTM101BaldAPI;
using MTM101BaldAPI.Registers;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;

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
		this.DestroyParticleIfItHasOne();

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
		collider.size = new Vector3(4.96f, 1f, 4.96f);
		collider.center = Vector3.up * 2f;

		stainEffectorPre = new GameObject("StainEffector").AddComponent<StainEffector>();
		stainEffectorPre.gameObject.ConvertToPrefab(true);
		stainEffectorPre.entity = stainEffectorPre.gameObject.CreateEntity(4.5f, 2f);
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
	readonly internal HashSet<Entity> affectedEntities = [];

	public void Initialize(ITM_BloxyCola soda) =>
		owner = soda;


	public void CreateStain(Cell cell)
	{
		if (stainedCells.Contains(cell)) return;

		var stain = Instantiate(owner.stainPre, transform);
		stain.transform.position = cell.FloorWorldPosition;
		stain.Initialize(this);
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
}
public class Stain : MonoBehaviour
{
	private StainController controller;

	float spawnDelay = 0.1f;

	public void Initialize(StainController controller)
	{
		this.controller = controller;
	}

	void Update()
	{
		if (spawnDelay > 0f)
			spawnDelay -= Time.deltaTime;
	}


	void OnTriggerEnter(Collider other)
	{
		if (spawnDelay > 0f)
			return;

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
	internal float speed = 15f, speedLimit = 50f;

	[SerializeField]
	internal AudioManager audMan;

	[SerializeField]
	internal SoundObject audHitWall, audStartSlip;

	public void Initialize(StainController controller, Entity target)
	{
		this.controller = controller;
		targetEntity = target;
		targetEntity.ExternalActivity.moveMods.Add(slipMod);
		entity.ExternalActivity.ignoreFrictionForce = true;
		speed += targetEntity.Velocity.magnitude * 22.5f;
		if (speed > speedLimit)
			speed = speedLimit;
		slipDirection = targetEntity.Velocity.normalized;

		entity.Initialize(controller.owner.ec, target.transform.position);
		entity.OnEntityMoveInitialCollision += (hit) =>
		{
			slipDirection = Vector3.Reflect(slipDirection, hit.normal);
			audMan.PlaySingle(audHitWall);
		};

		entity.IgnoreEntity(target, true); // Avoid collision with whatever is slipping above

		audMan.PlaySingle(audStartSlip);
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
		controller?.affectedEntities.RemoveWhere(x => !x || x == targetEntity);
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