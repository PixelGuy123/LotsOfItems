using System.Collections;
using HarmonyLib;
using LotsOfItems.ItemPrefabStructures;
using UnityEngine;

namespace LotsOfItems.CustomItems;

public abstract class ITM_GenericAlarmClock : ITM_AlarmClock, IItemPrefab
{
	protected int CurrentTimeSet { get; private set; } = 0;
	public void SetupPrefab(ItemObject itm) =>
		VirtualSetupPrefab(itm);

	public void SetupPrefabPost() { }
	protected virtual void VirtualSetupPrefab(ItemObject itm) { }
	protected virtual bool ShouldRingOnEnd() => true;
	protected virtual void OnClockRing() { }
	public virtual bool AllowClickable() => true;
	protected virtual void OnClockClicked() { }
	protected virtual void Destroy() => Destroy(gameObject);

	public IEnumerator OverrideTimer(float initTime)
	{
		CurrentTimeSet = initSetTime;
		yield return null;
		time = initTime;
		while (time > 0f)
		{
			time -= Time.deltaTime * ec.EnvironmentTimeScale;

			if (AllowClickable())
			{
				if (time <= setTime[CurrentTimeSet])
					spriteRenderer.sprite = clockSprite[0];
				else if (time <= setTime[CurrentTimeSet])
					spriteRenderer.sprite = clockSprite[1];
				else if (time <= setTime[CurrentTimeSet])
					spriteRenderer.sprite = clockSprite[2];
				else
					spriteRenderer.sprite = clockSprite[3];
			}
			yield return null;
		}
		if (ShouldRingOnEnd())
		{
			if (noiseVal > 0)
				ec.MakeNoise(transform.position, noiseVal);
			audMan.FlushQueue(endCurrent: true);
			audMan.PlaySingle(audRing);
			OnClockRing();
		}

		finished = true;
		while (audMan.AnyAudioIsPlaying)
			yield return null;

		Destroy();
	}

	// Custom click handler with hook
	public void OverridenClicked(int playerNumber)
	{
		if (!finished && AllowClickable())
		{
			audMan.PlaySingle(audWind);
			if (time <= setTime[0])
			{
				CurrentTimeSet = 1;
				time = setTime[1];
			}
			else if (time <= setTime[1])
			{
				CurrentTimeSet = 2;
				time = setTime[2];
			}
			else if (time <= setTime[2])
			{
				CurrentTimeSet = 3;
				time = setTime[3];
			}
			else
			{
				CurrentTimeSet = 0;
				time = setTime[0];
			}
			OnClockClicked();
		}
	}
}

[HarmonyPatch(typeof(ITM_AlarmClock))]
static class GenericAlarmClockPatch
{
	[HarmonyPatch("Timer")]
	[HarmonyPrefix]
	static bool TimerPrefix(ITM_AlarmClock __instance, float initTime, ref IEnumerator __result)
	{
		if (__instance is ITM_GenericAlarmClock generic)
		{
			__result = generic.OverrideTimer(initTime);
			return false;
		}
		return true;
	}

	[HarmonyPatch("Clicked")]
	[HarmonyPrefix]
	static bool ClickedPrefix(ITM_AlarmClock __instance, int playerNumber)
	{
		if (__instance is ITM_GenericAlarmClock generic)
		{
			generic.OverridenClicked(playerNumber);
			return false; // Skip the original Clicked method.
		}
		return true;
	}

	[HarmonyPatch("ClickableHidden")]
	[HarmonyPostfix]
	static void ClickableHiddenOverride(ITM_AlarmClock __instance, ref bool __result) =>
		__result = __instance is ITM_GenericAlarmClock generic ? !generic.AllowClickable() : __result;
}

