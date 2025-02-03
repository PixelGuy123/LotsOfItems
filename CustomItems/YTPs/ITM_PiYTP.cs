using UnityEngine;

namespace LotsOfItems.CustomItems.YTPs
{
	public class ITM_PiYTP : Item
	{
		public override bool Use(PlayerManager pm)
		{
			int point = 3;
			if (Random.value <= chanceForSecondDigit)
			{
				point = 31;
				if (Random.value <= chanceForThirdDigit)
				{
					point = 314;
				}
			}


			Singleton<CoreGameManager>.Instance.AddPoints(point, pm.playerNumber, true);

			Destroy(gameObject);

			return true;
		}

		[SerializeField]
		[Range(0f, 1f)]
		internal float chanceForSecondDigit = 0.85f, chanceForThirdDigit = 0.25f;
	}
}
