using System.Collections.Generic;
using LotsOfItems.Components;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using UnityEngine;

namespace LotsOfItems.CustomItems.BSODAs;

public class ITM_BloxyCola : ITM_GenericBSODA, ISlipperOwner
{
	private SlipperController stainController;
	private Cell lastRecordedCell;

	[SerializeField]
	internal Slipper slipperPre;
	[SerializeField]
	internal SlipperEffector slipperEffectorPre;

	// ISlipperOwner
	Slipper ISlipperOwner.slipperPre { get => slipperPre; set => slipperPre = value; }
	SlipperEffector ISlipperOwner.slipperEffectorPre { get => slipperEffectorPre; set => slipperEffectorPre = value; }
	EnvironmentController ISlipperOwner.ec => ec;
	GameObject ISlipperOwner.gameObject => gameObject;

	protected override void VirtualSetupPrefab(ItemObject itm)
	{
		base.VirtualSetupPrefab(itm);
		spriteRenderer.sprite = this.GetSprite("BloxyCola_soda.png", spriteRenderer.sprite.pixelsPerUnit);
		this.DestroyParticleIfItHasOne();
		SlipperController.CreateSlipperPackPrefab(this, this.GetSprite("BloxyCola_stain.png", 10f));

		time = 30f;
		sound = this.GetSoundNoSub("BloxyCola_use.wav", SoundType.Effect);
	}

	public override bool Use(PlayerManager pm)
	{
		bool val = base.Use(pm);
		stainController = SlipperController.CreateSlipperController(this);
		stainController.transform.position = transform.position;
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