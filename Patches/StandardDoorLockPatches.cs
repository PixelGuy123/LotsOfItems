using HarmonyLib;
using LotsOfItems.Components;
using LotsOfItems.Plugin;
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
		__instance && !__instance.GetComponent<Marker_BlockedStandardDoor>(); // Null check because prefixes are still called in events

	[HarmonyPatch(typeof(StandardDoor), "OnTriggerEnter")]
	[HarmonyPrefix]
	static bool PreventNPCOpenPatch(StandardDoor __instance, Collider other)
	{
		if (!__instance.locked) return true;

		var weakMarker = __instance.GetComponent<Marker_WeakLockedDoor>();
		if (weakMarker && other.isTrigger && other.CompareTag("NPC") && other.TryGetComponent<NPC>(out var npc))
		{
			if (npc.IsPrincipal())
				return true;
			if (!weakMarker.IncrementRattle())
			{
				npc.Navigator.Entity.Teleport(__instance.ec.CellFromPosition(npc.transform.position - npc.Navigator.Velocity).FloorWorldPosition);
				return false;
			}
		}

		return !__instance.GetComponent<Marker_BlockedStandardDoor>();
	}


	// Lock mechanic
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Door), "Lock")]
	static void ActualLock(Door __instance, ref bool ___lockBlocks)
	{
		if (!___lockBlocks)
			___lockBlocks = __instance.GetComponent<Marker_BlockedStandardDoor>();
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(Door), "Unlock")]
	static void ActualUnlockPre(out bool __state, bool ___lockBlocks) =>
		__state = ___lockBlocks;
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Door), "Unlock")]
	static void ActualUnlock(Door __instance, bool __state, ref bool ___lockBlocks)
	{
		var marker = __instance.GetComponent<Marker_BlockedStandardDoor>();
		if (marker) // If closeBlocks was previously false (__state), it'll destroy the marker since it actually opens
		{
			Object.Destroy(marker);
			___lockBlocks = __state;
		}

		var weakMarker = __instance.GetComponent<Marker_WeakLockedDoor>();
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
		var marker = __instance.GetComponent<Marker_WeakLockedDoor>();
		// Check if the door has the marker and is currently locked
		if (marker != null && __instance.locked)
		{
			marker.IncrementRattle();
		}
	}

	// ***** Proper Muting Part *****
	[HarmonyPatch(typeof(Door), "Open", [typeof(bool), typeof(bool)])]
	[HarmonyPrefix]
	static void OverrideNoise(Door __instance, ref bool makeNoise)
	{
		if (!makeNoise)
			return;

		foreach (var silence in __instance.GetComponents<Marker_StandardDoorSilenced>()) // Get all silence components, since they can be stacked on the door
		{
			makeNoise = false;
			if (--silence.counter <= 0)
				Object.Destroy(silence); // Removes the marker automatically
		}
	}
}