using LotsOfItems.ItemPrefabStructures;
using MTM101BaldAPI.Components;
using PixelInternalAPI.Extensions;
using System.Collections;
using UnityEngine;

namespace LotsOfItems.CustomItems.Tapes
{
	public class ITM_LeastFavoriteTape : ITM_GenericTape
	{
		readonly ValueModifier blindModifier = new(0f);

		protected override void VirtualSetupPrefab(ItemObject itm)
		{
			base.VirtualSetupPrefab(itm);
			useOriginalTapePlayerFunction = true;
			audioToOverride = [this.GetSound("LeastFavorite_SufferingInAudio.wav", "LtsOItems_Vfx_AnnoyingNoise", SoundType.Effect, Color.white)];
		}
		protected override IEnumerator NewCooldown(TapePlayer tapePlayer)
		{
			while (tapePlayer.dijkstraMap.PendingUpdate)
				yield return null;

			foreach (var npc in tapePlayer.Ec.Npcs)
			{
				npc.Navigator.Entity.SetBlinded(true);
				npc.GetNPCContainer().AddLookerMod(blindModifier);
			}

			while (tapePlayer.audMan.AnyAudioIsPlaying)
				yield return null;

			foreach (var npc in tapePlayer.Ec.Npcs)
			{
				npc.Navigator.Entity.SetBlinded(false);
				npc.GetNPCContainer().RemoveLookerMod(blindModifier);
			}
		}
	}
}
