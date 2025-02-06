using UnityEngine;

namespace LotsOfItems.CustomItems.Eatables
{
	public class ITM_IceZestyBar : ITM_GenericZestyEatable
	{
		protected override void VirtualSetupPrefab(ItemObject itemObject)
		{
			base.VirtualSetupPrefab(itemObject);
			// To-do: Implement freezing functionality that causes characters to slip on frozen ground.
		}

		protected override bool CanBeDestroyed() =>
			false; // Avoid being destroyed for a while

		public override bool Use(PlayerManager pm)
		{
			return base.Use(pm);
		}
	}
}
