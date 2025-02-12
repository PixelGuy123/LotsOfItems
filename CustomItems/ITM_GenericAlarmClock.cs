using System.Collections;
using UnityEngine;
using HarmonyLib;
using LotsOfItems.ItemPrefabStructures;

namespace LotsOfItems.CustomItems;
public abstract class ITM_GenericAlarmClock : ITM_AlarmClock, IItemPrefab
{
	public void SetupPrefab(ItemObject itm) =>
		VirtualSetupPrefab(itm);
	
	public void SetupPrefabPost() { }
	protected virtual void VirtualSetupPrefab(ItemObject itm) { }
	protected virtual bool ShouldRingOnEnd() => true;
	protected virtual void OnClockRing() { }
	public virtual bool AllowClickable() => true;
	protected virtual void OnClockClicked() { }

	public IEnumerator OverrideTimer(float initTime)
	{
		time = initTime;
		while (time > 0f)
		{
			time -= Time.deltaTime * ec.EnvironmentTimeScale;

			if (AllowClickable())
			{
				if (time <= setTime[0])
					spriteRenderer.sprite = clockSprite[0];
				else if (time <= setTime[1])
					spriteRenderer.sprite = clockSprite[1];
				else if (time <= setTime[2])
					spriteRenderer.sprite = clockSprite[2];
				else
					spriteRenderer.sprite = clockSprite[3];
			}
			yield return null;
		}
		if (ShouldRingOnEnd())
		{
			ec.MakeNoise(transform.position, noiseVal);
			audMan.FlushQueue(endCurrent: true);
			audMan.PlaySingle(audRing);
			spriteRenderer.sprite = clockSprite[3];
			OnClockRing();
		}

		finished = true;
		while (audMan.QueuedAudioIsPlaying)
			yield return null;
		Destroy(gameObject);
	}

	// Custom click handler with hook
	public void OverridenClicked(int playerNumber)
	{
		if (!finished && AllowClickable())
		{
			audMan.PlaySingle(audWind);
			if (time <= setTime[0])
				time = setTime[1];
			else if (time <= setTime[1])
				time = setTime[2];
			else if (time <= setTime[2])
				time = setTime[3];
			else
				time = setTime[0];
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

