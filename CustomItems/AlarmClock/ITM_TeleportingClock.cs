using System.Collections;
using LotsOfItems.Components;
using LotsOfItems.ItemPrefabStructures;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.AlarmClock;

public class ITM_TeleportingClock : ITM_GenericAlarmClock
{

	[SerializeField]
	internal SoundObject audTeleport;

	[SerializeField]
	[Range(0f, 1f)]
	internal float chanceToDrawBaldi = 0.5f;

	[SerializeField]
	internal int realNoiseVal = 75;

	protected override void VirtualSetupPrefab(ItemObject itm)
	{
		base.VirtualSetupPrefab(itm);
		audTeleport = GenericExtensions.FindResourceObjectByName<SoundObject>("Teleport");
		spriteRenderer.sprite = this.GetSprite("TeleportingClock_World.png", spriteRenderer.sprite.pixelsPerUnit);
		clockSprite = [spriteRenderer.sprite];
	}

	public override bool Use(PlayerManager pm)
	{
		this.pm = pm;
		return base.Use(pm);
	}

	public override bool AllowClickable() => false;

	protected override bool ShouldRingOnEnd()
	{
		noiseVal = Random.value <= chanceToDrawBaldi ? realNoiseVal : 0;
		return base.ShouldRingOnEnd();
	}

	protected override void OnClockRing()
	{
		base.OnClockRing();
		pm.Teleport(transform.position);
		audMan.PlaySingle(audTeleport);
	}
}
