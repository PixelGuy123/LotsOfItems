using UnityEngine;

namespace LotsOfItems.CustomItems
{
	public class ITM_SquareYTP : Item
	{
		public override bool Use(PlayerManager pm)
		{
			var points = Singleton<CoreGameManager>.Instance.GetPoints(pm.playerNumber).ToString();
			int pointToMultiply = -1;
			for (int i = 0; i < points.Length; i++)
			{
				
				if (points[i] != '0' && points[i] != '1')
				{
					double num = char.GetNumericValue(points[i]);
					if (pointToMultiply < num)
						pointToMultiply = (int)num;
				}
			}

			Singleton<CoreGameManager>.Instance.AddPoints(
				pointToMultiply == -1 ? defaultGivingValue :
				Mathf.FloorToInt(Mathf.Pow(pointToMultiply, squareVal)),
				pm.playerNumber, true);

			Destroy(gameObject);

			return true;
		}

		[SerializeField]
		internal int defaultGivingValue = 35;

		[SerializeField]
		internal float squareVal = 2.25f;
	}
}
