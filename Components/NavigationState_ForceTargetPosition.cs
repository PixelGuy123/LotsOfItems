using UnityEngine;

namespace LotsOfItems.Components
{
	public class NavigationState_ForceTargetPosition(NPC npc, int priority, Vector3 pos) : NavigationState_TargetPosition(npc, priority, pos)
	{
		readonly Cell cellToGo = npc.ec.CellFromPosition(pos);
		public override void DestinationEmpty()
		{
			if (active && npc.ec.CellFromPosition(npc.transform.position) != cellToGo)
				npc.Navigator.FindPath(destination);
			else
				base.DestinationEmpty();
		}

		public override void Exit()
		{
			base.Exit();
			priority = 0;
		}
	}
}
