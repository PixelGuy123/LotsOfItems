using HarmonyLib;

namespace LotsOfItems.Patches
{
	[HarmonyPatch(typeof(Principal))]
	internal static class PrincipalPatch
	{
		[HarmonyPatch("Scold")]
		static bool Prefix(AudioManager ___audMan, SoundObject ___audNoEating, string brokenRule)
		{
			if (brokenRule == "Eating")
			{
				___audMan.QueueAudio(___audNoEating);
				return false;
			}

			return true;
		}
	}
}
