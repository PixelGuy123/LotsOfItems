using HarmonyLib;
using LotsOfItems.CustomItems.SwingingDoorLocks;
using UnityEngine;

namespace LotsOfItems.Patches;

[HarmonyPatch]
internal static class DoorActualLockPatch
{
	// ****** General part ******
	[HarmonyPatch(typeof(StandardDoor), "TempOpenIfLocked")]
	[HarmonyPatch(typeof(StandardDoor), "TempCloseIfLocked")]
	[HarmonyPrefix]
	static bool NoTempChangeWhenLocked(StandardDoor __instance) =>
		!__instance.GetComponent<DoorActuallyBlockedMarker>() && !__instance.GetComponent<WeakLockMarker>();

	internal static bool IsPrincipal(NPC npc) =>
			npc.Character == Character.Principal; // To be patched later lol

	[HarmonyPatch(typeof(StandardDoor), "OnTriggerEnter")]
	[HarmonyPrefix]
	static bool PreventNPCOpenPatch(StandardDoor __instance, Collider other)
	{
		if (!__instance.locked) return true;

		var weakMarker = __instance.GetComponent<WeakLockMarker>();
		if (weakMarker && other.isTrigger && other.CompareTag("NPC") && other.TryGetComponent<NPC>(out var npc))
			return IsPrincipal(npc);

		return !__instance.GetComponent<DoorActuallyBlockedMarker>();
	}


	[HarmonyPrefix]
	[HarmonyPatch(typeof(Door), "Lock")]
	static void ActualLock(Door __instance, ref bool ___lockBlocks)
	{
		if (!___lockBlocks)
			___lockBlocks = __instance.GetComponent<DoorActuallyBlockedMarker>() || __instance.GetComponent<WeakLockMarker>();
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(Door), "Unlock")]
	static void ActualUnlockPre(out bool __state, bool ___lockBlocks) =>
		__state = ___lockBlocks;
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Door), "Unlock")]
	static void ActualUnlock(Door __instance, bool __state, ref bool ___lockBlocks)
	{
		var marker = __instance.GetComponent<DoorActuallyBlockedMarker>();
		if (marker) // If closeBlocks was previously false (__state), it'll destroy the marker since it actually opens
		{
			Object.Destroy(marker);
			___lockBlocks = __state;
		}

		var weakMarker = __instance.GetComponent<WeakLockMarker>();
		if (weakMarker)
		{
			weakMarker.SelfDestroy();
			___lockBlocks = __state;
		}
	}

	// ******* Weak Lock Specific part ******
	[HarmonyPatch(typeof(StandardDoor), "Clicked")]
	[HarmonyPostfix]
	static void DetectRattlePatch(StandardDoor __instance)
	{
		var marker = __instance.GetComponent<WeakLockMarker>();
		// Check if the door has the marker and is currently locked
		if (marker != null && __instance.locked)
		{
			marker.IncrementRattle();
		}
	}
}