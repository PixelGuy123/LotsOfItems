using System.Collections;
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
		gaugeSprite = itm.itemSpriteLarge;
		audMan.useUnscaledPitch = true;

		for (int i = 0; i < clockSprite.Length; i++)
			clockSprite[i] = spriteRenderer.sprite;
	}

	private TimeScaleModifier timeScaleModifier;

	private Coroutine slideDownCoroutine;
	private Coroutine slideUpCoroutine;

	public override bool Use(PlayerManager pm)
	{
		ec = pm.ec;
		timeScaleModifier = new TimeScaleModifier(1f, 1f, 1f);
		ec.AddTimeScale(timeScaleModifier);
		ec.FlickerLights(true);
		if (slideDownCoroutine != null)
			StopCoroutine(slideDownCoroutine);
		slideDownCoroutine = StartCoroutine(SlideTimeScaleDown());

		bool flag = base.Use(pm);

		gauge = Singleton<CoreGameManager>.Instance.GetHud(pm.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, setTime[CurrentTimeSet]);

		return flag;
	}

	// Slides time scale from 1 to 0.75 with ease-in-out over 1.5 seconds
	private IEnumerator SlideTimeScaleDown()
	{
		float duration = timeSlowdownAnimationDuration;
		float elapsed = 0f;
		float start = 1f;
		float end = timeSlowdownValue;
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime * ec.EnvironmentTimeScale;
			float t = Mathf.Clamp01(elapsed / duration);
			// Ease-in-out (smoothstep)
			float value = Mathf.SmoothStep(start, end, t);
			timeScaleModifier.environmentTimeScale = value;
			timeScaleModifier.npcTimeScale = value;
			yield return null;
		}
		timeScaleModifier.environmentTimeScale = end;
		timeScaleModifier.npcTimeScale = end;
	}

	// Slides time scale from 0.75 to 1 with ease-out over 5 seconds (fade out duration)
	private IEnumerator SlideTimeScaleUp()
	{
		float duration = effectFadeOutDuration;
		float elapsed = 0f;
		float start = timeSlowdownValue;
		float end = 1f;
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime * ec.EnvironmentTimeScale;
			float t = Mathf.Clamp01(elapsed / duration);
			// Ease-out (quadratic)
			float eased = 1f - (1f - t) * (1f - t);
			float value = Mathf.Lerp(start, end, eased);
			timeScaleModifier.environmentTimeScale = value;
			timeScaleModifier.npcTimeScale = value;
			yield return null;
		}
		timeScaleModifier.environmentTimeScale = end;
		timeScaleModifier.npcTimeScale = end;
	}

	public override bool AllowClickable()
	{
		gauge.SetValue(setTime[CurrentTimeSet], time);
		return false;
	}

	protected override bool ShouldRingOnEnd()
	{
		audMan.FadeOut(effectFadeOutDuration);
		if (slideDownCoroutine != null)
			StopCoroutine(slideDownCoroutine);
		if (slideUpCoroutine != null)
			StopCoroutine(slideUpCoroutine);
		slideUpCoroutine = StartCoroutine(SlideTimeScaleUp());
		gauge.Deactivate();
		ec.FlickerLights(false);
		// RemoveTimeScale will be called after fade out, so don't remove here
		return false;
	}

	protected override void Destroy()
	{
		base.Destroy();
		ec.RemoveTimeScale(timeScaleModifier);
	}

	[SerializeField]
	float effectFadeOutDuration = 5f, timeSlowdownValue = 0.75f, timeSlowdownAnimationDuration = 1.5f;
	[SerializeField]
	Sprite gaugeSprite;

	HudGauge gauge;
}
