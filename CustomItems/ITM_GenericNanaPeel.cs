using UnityEngine;
using HarmonyLib;

namespace LotsOfItems.CustomItems
{
	public abstract class ITM_GenericNanaPeel : ITM_NanaPeel
	{
		internal virtual void AdditionalSpawnContribute() { }

		internal virtual bool OnCollisionOverride(RaycastHit hit) => true;
	}

	[HarmonyPatch(typeof(ITM_NanaPeel))]
	internal static class NanaPeelPatch
	{
		[HarmonyPatch("Spawn")]
		[HarmonyPostfix]
		static void OverrideSpawn(ITM_NanaPeel __instance)
		{
			if (__instance is ITM_GenericNanaPeel gen)
				gen.AdditionalSpawnContribute();
		}

		[HarmonyPatch("OnEntityMoveCollision")]
		[HarmonyPrefix]
		static bool CollisionEnterOverride(ITM_NanaPeel __instance, RaycastHit hit)
		{
			if (__instance is ITM_GenericNanaPeel gen)
				return gen.OnCollisionOverride(hit);
			return true;
		}
	}
}
