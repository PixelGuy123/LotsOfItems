using HarmonyLib;
using UnityEngine;

namespace LotsOfItems.Components
{
	public class PlayerCustomAttributes : MonoBehaviour
	{
		public void SetAddendOnlyImmunity(bool resist)
		{
			if (resist)
				immuneResists++;
			else
			{
				if (--immuneResists < 0)
					immuneResists = 0;
			}
		}
		public bool AddendImmune => immuneResists != 0;
		int immuneResists = 0;
		public void SetDoorOpeningSilent(bool silent)
		{
			if (silent)
				doorSilences++;
			else
			{
				if (--doorSilences < 0)
					doorSilences = 0;
			}
		}
		public bool DoorOpeningIsSilent => doorSilences != 0;

		int doorSilences = 0;
	}

	[HarmonyPatch]
	public static class AttributeContributes
	{
		public static PlayerCustomAttributes GetAttributes(this PlayerManager pm)
		{
			if (pm.TryGetComponent<PlayerCustomAttributes>(out var comp))
				return comp;
			return pm.gameObject.AddComponent<PlayerCustomAttributes>();
		}

		[HarmonyPatch(typeof(StandardDoor), "Clicked")]
		[HarmonyPrefix]
		static void MakeSureToBeSilentIfNeeded(int player, out bool __state, ref bool ___makesNoise)
		{
			__state = ___makesNoise;

			var comp = Singleton<CoreGameManager>.Instance.GetPlayer(player).GetAttributes();
			if (comp && comp.DoorOpeningIsSilent)
				___makesNoise = false;
		}

		[HarmonyPatch(typeof(StandardDoor), "Clicked")]
		[HarmonyPostfix]
		static void CorrectNoiseAttributeAfterwards(bool __state, ref bool ___makesNoise) =>
			___makesNoise = __state;

		[HarmonyPatch(typeof(ActivityModifier), "Addend", MethodType.Getter)]
		[HarmonyPrefix]
		static bool OverrideActivityAddend(ref Vector3 __result, Entity ___entity)
		{
			if (___entity is not PlayerEntity)
				return true;
			if (___entity.GetComponent<PlayerCustomAttributes>()?.AddendImmune ?? false)
			{
				__result = Vector3.zero;
				return false;
			}
			return true;
		}
	}

}
