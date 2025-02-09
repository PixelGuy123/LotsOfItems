using UnityEngine;
using HarmonyLib;

namespace LotsOfItems.Components
{
	public class PlayerCustomAttributes : MonoBehaviour
	{
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
		public static PlayerCustomAttributes GetAttributes(this PlayerManager pm) =>
			pm.GetComponent<PlayerCustomAttributes>();

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
	}
	
}
