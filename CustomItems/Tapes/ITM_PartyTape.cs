using PixelInternalAPI.Extensions;
using System.Collections;
using System.Collections.Generic;

namespace LotsOfItems.CustomItems.Tapes;
public class ITM_PartyTape : ITM_GenericTape
{
	protected override void VirtualSetupPrefab(ItemObject itm) =>
		audioToOverride = [GenericExtensions.FindResourceObjectByName<SoundObject>("mus_party")];
	
	public override bool Use(PlayerManager pm)
	{
		this.pm = pm;
		return base.Use(pm);
	}

	// Coroutine that drags Baldi to the player's position until the tape finishes playing.
	protected override IEnumerator NewCooldown(TapePlayer tapePlayer)
	{
		List<NavigationState_PartyEvent> navigationStates = [];
		foreach (NPC npc in pm.ec.Npcs)
		{
			if (npc.Navigator.enabled)
			{
				NavigationState_PartyEvent navigationState_PartyEvent = new(npc, 31, pm.ec.CellFromPosition(tapePlayer.transform.position).room);
				navigationStates.Add(navigationState_PartyEvent);
				npc.navigationStateMachine.ChangeState(navigationState_PartyEvent);
			}
		}

		yield return null;
		while (tapePlayer.audMan.QueuedAudioIsPlaying)
			yield return null;

		foreach (NavigationState_PartyEvent navigationState_PartyEvent in navigationStates)
			navigationState_PartyEvent.End();
	}
}
