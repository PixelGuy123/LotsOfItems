using LotsOfItems.Components;
using LotsOfItems.ItemPrefabStructures;
using System.Collections;
using UnityEngine;

namespace LotsOfItems.CustomItems.Tapes;
public class ITM_BaldisMostFavoriteTape : ITM_GenericTape
{
	protected override void VirtualSetupPrefab(ItemObject itm) =>
		audioToOverride = [this.GetSound("BaldisMostFavoriteTape_song.wav", "LtsOItems_Vfx_BaldiFavoriteSound", SoundType.Effect, Color.green)];
	
	public override bool Use(PlayerManager pm)
	{
		this.pm = pm;
		return base.Use(pm);
	}

	// Coroutine that drags Baldi to the player's position until the tape finishes playing.
	protected override IEnumerator NewCooldown(TapePlayer tapePlayer)
	{
		NavigationState_ForceTargetPosition forceTarget = null;
		var baldi = pm.ec.GetBaldi();
		while (baldi == null || !tapePlayer.audMan.AnyAudioIsPlaying) // tv announcer compat
		{
			baldi = pm.ec.GetBaldi();
			yield return null;
		}

		if (baldi != null)
		{
			forceTarget = new(baldi, 63, tapePlayer.transform.position, true);
			baldi.navigationStateMachine.ChangeState(forceTarget);
		}

		while (tapePlayer.audMan.AnyAudioIsPlaying)
			yield return null;

		if (forceTarget != null && baldi)
		{
			forceTarget.NoDestinEmpty = false;
			forceTarget.DestinationEmpty();
		}
		yield break;
	}
}
