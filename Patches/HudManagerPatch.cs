using HarmonyLib;
using UnityEngine;

namespace LotsOfItems.Patches
{
	[HarmonyPatch(typeof(HudManager))]
	internal static class HudManagerPatch
	{
		[HarmonyPatch("SetStaminaValue", [typeof(float)])]
		[HarmonyPostfix]
		static void MakeSureItDoesntGoTooMuchLol(ref float ___needleTargetValue) =>
			___needleTargetValue = Mathf.Min(___needleTargetValue, 3f); // 3 is max here!
	}
}
