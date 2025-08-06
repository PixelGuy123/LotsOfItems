using UnityEngine;

namespace LotsOfItems.CustomItems.Eatables
{
	public class ITM_Reusable_GenericZestyEatable : ITM_GenericZestyEatable
	{
		public override bool Use(PlayerManager pm)
		{
			if (maxStaminaLimit != -1 && pm.plm.staminaMax >= maxStaminaLimit)
			{
				Destroy(gameObject);
				return false;
			}

			this.pm = pm;
			bool flag = base.Use(pm);
			if (flag && nextItem)
			{
				pm.itm.SetItem(nextItem, pm.itm.selectedItem);
				return false;
			}

			return flag;
		}

		[SerializeField]
		internal ItemObject nextItem = null;

		[SerializeField]
		internal float maxStaminaLimit = -1f;
	}
}
