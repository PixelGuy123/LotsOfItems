using UnityEngine;

namespace LotsOfItems.CustomItems.YTPs
{
	public class ITM_SquareRootYTP : Item
	{
		public override bool Use(PlayerManager pm)
		{
			var points = Singleton<CoreGameManager>.Instance.GetPoints(pm.playerNumber);

			Singleton<CoreGameManager>.Instance.AddPoints(
				System.Math.Max(1, Mathf.FloorToInt(Mathf.Sqrt(points)))
				, pm.playerNumber, true);

			Destroy(gameObject);

			return true;
		}
	}
}
