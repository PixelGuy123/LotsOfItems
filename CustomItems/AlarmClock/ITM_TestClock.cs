using LotsOfItems.ItemPrefabStructures;
using PixelInternalAPI.Extensions;

namespace LotsOfItems.CustomItems.AlarmClock;
public class ITM_TestClock : ITM_GenericAlarmClock
{
	protected override void VirtualSetupPrefab(ItemObject itm)
	{
		base.VirtualSetupPrefab(itm);
		audMan.soundOnStart =
			[
			GenericExtensions.FindResourceObjectByName<SoundObject>("LAt_Activating"),
			GenericExtensions.FindResourceObjectByName<SoundObject>("LAt_Loop")
			]; // The audio loop is overriden
		spriteRenderer.sprite = this.GetSprite("TestClock_World", spriteRenderer.sprite.pixelsPerUnit);
		clockSprite = [spriteRenderer.sprite];
	}

	private TimeScaleModifier timeScaleModifier;

	public override bool Use(PlayerManager pm)
	{
		timeScaleModifier = new TimeScaleModifier(0f, 1f, 1f);
		pm.ec.AddTimeScale(timeScaleModifier);
		return base.Use(pm);
	}

	protected override void OnClockRing()
	{
		if (timeScaleModifier != null)
			pm.ec.RemoveTimeScale(timeScaleModifier);
	}

	public override bool AllowClickable() => false;

	protected override bool ShouldRingOnEnd()
	{
		audMan.FadeOut(5f);
		return false;
	}
}
