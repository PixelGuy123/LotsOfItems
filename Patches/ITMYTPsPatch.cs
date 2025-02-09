using HarmonyLib;

namespace LotsOfItems.Patches
{
	[HarmonyPatch(typeof(ITM_YTPs), "Use")]
	internal static class ITMYTPsPatch
	{
		static void Prefix(PlayerManager pm, ref int ___value)
		{
			int points = Singleton<CoreGameManager>.Instance.GetPoints(pm.playerNumber);
			if (points >= 0 && points + ___value < 0)
				___value = -points;
		}
	}
}
