using LotsOfItems.ItemPrefabStructures;
using MTM101BaldAPI.Components;
using PixelInternalAPI.Components;
using PixelInternalAPI.Extensions;
using System.Collections;
using UnityEngine;

namespace LotsOfItems.CustomItems.Tapes;
public class ITM_BaldisMostFavoriteTape : ITM_GenericTape
{
	readonly ValueModifier blindBaldiModifier = new(0f);

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
		NavigationState_PartyEvent forceTarget = null;
		var baldi = pm.ec.GetBaldi();
		NPCAttributesContainer container = null;
		while (baldi == null || !tapePlayer.audMan.AnyAudioIsPlaying) // tv announcer compat
		{
			baldi = pm.ec.GetBaldi();
			yield return null;
		}

		if (baldi != null)
		{
			container = baldi.GetNPCContainer();
			container.AddLookerMod(blindBaldiModifier);

			forceTarget = new(baldi, 63, pm.ec.CellFromPosition(tapePlayer.transform.position).room);
			baldi.navigationStateMachine.ChangeState(forceTarget);
		}

		while (tapePlayer.audMan.AnyAudioIsPlaying)
			yield return null;

		if (forceTarget != null && baldi)
		{
			container.RemoveLookerMod(blindBaldiModifier);
			forceTarget.End();
		}
		yield break;
	}
}
