using UnityEngine;

namespace LotsOfItems.CustomItems.YTPs
{
	public class ITM_MysteryYTP : ITM_YTPs
	{
		public override bool Use(PlayerManager pm)
		{
			value = Random.Range(minVal, maxVal + 1);
			return base.Use(pm);
		}

		[SerializeField]
		internal int minVal = 5, maxVal = 105;
	}
}
