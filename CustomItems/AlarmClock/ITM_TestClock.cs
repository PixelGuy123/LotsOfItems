using LotsOfItems.ItemPrefabStructures;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.AlarmClock;
public class ITM_TestClock : ITM_GenericAlarmClock
{
	protected override void VirtualSetupPrefab(ItemObject itm)
	{
		base.VirtualSetupPrefab(itm);
		audMan.soundOnStart =
			[
			GenericExtensions.FindResourceObjectByName<SoundObject>("LAt_Loop")
			]; // The audio loop is overriden
		spriteRenderer.sprite = this.GetSprite("TestClock_World.png", spriteRenderer.sprite.pixelsPerUnit);
		spriteRenderer.transform.localPosition = Vector3.down * 0.25f;
		clockSprite = [spriteRenderer.sprite];
	}

	private TimeScaleModifier timeScaleModifier;

	public override bool Use(PlayerManager pm)
	{
		ec = pm.ec;
		timeScaleModifier = new TimeScaleModifier(0f, 1f, 1f);
		ec.AddTimeScale(timeScaleModifier);
		return base.Use(pm);
	}

	public override bool AllowClickable() => false;

	protected override bool ShouldRingOnEnd()
	{
		audMan.FadeOut(5f);
		ec.RemoveTimeScale(timeScaleModifier);
		return false;
	}
}
