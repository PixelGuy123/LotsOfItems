using UnityEngine;

namespace LotsOfItems.Components
{
	public class NavigationState_ForceTargetPosition(NPC npc, int priority, Vector3 pos) : NavigationState_TargetPosition(npc, priority, pos)
	{
		bool exited = false;
		public override void DestinationEmpty()
		{
			End();
		}

		public void End()
		{
			if (exited)
				return;
			exited = true;
			npc.behaviorStateMachine.RestoreNavigationState();
			priority = 0;
		}
	}
}
