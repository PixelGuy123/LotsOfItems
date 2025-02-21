using UnityEngine;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Components;

namespace LotsOfItems.CustomItems.Eatables
{
	public class ITM_HotCrossBun : ITM_GenericZestyEatable
	{
		protected override void VirtualSetupPrefab(ItemObject itemObject)
		{
			base.VirtualSetupPrefab(itemObject);
			audSecondEatNoise = this.GetSoundNoSub("HCB_Jingle.wav", SoundType.Effect);

			affectorTime = 10f;
			maxMultiplier = 1.5f;
			staminaDropChanger = 0.5f;
			speedMultiplier = 1.15f;
		}

		public override bool Use(PlayerManager pm)
		{
			pm.ec.MakeNoise(pm.transform.position, noiseLevel);

			DijkstraMap map = new(pm.ec, PathType.Nav, pm.transform);
			map.Calculate();

			foreach (var npc in pm.ec.Npcs)
			{
				if (npc.Navigator.enabled && map.Value(IntVector2.GetGridPosition(npc.transform.position)) <= minimumDistanceForCall)
					npc.navigationStateMachine.ChangeState(new NavigationState_ForceTargetPosition(npc, 12, pm.transform.position));
			}

			pm.RuleBreak("Eating", 3f, 0.5f);

			return base.Use(pm);
		}

		[SerializeField]
		internal int minimumDistanceForCall = 12, noiseLevel = 15;
	}

	
}
