using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace LotsOfItems.Patches
{
	[HarmonyPatch(typeof(TapePlayer))]
	internal static class TapePlayerPatch
	{
		[HarmonyPatch(nameof(TapePlayer.InsertItem))]
		[HarmonyReversePatch]
		static void OverridenInsertItem(this TapePlayer instance, PlayerManager pm, EnvironmentController ec)
		{
			IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> i) =>
				new CodeMatcher(i)
				.Start()
				.InsertAndAdvance(
					new(OpCodes.Ldarg_0),
					Transpilers.EmitDelegate((TapePlayer tapePlayer) =>
					{
						if (newAudio == null)
						{
							tapePlayer.time = tapePlayer.beep.soundClip.length; // Make sure it has a dynamic length to the environment muting thing
						}
						else
						{
							tapePlayer.time = 0f;
							for (int i = 0; i < newAudio.Length; i++)
								tapePlayer.time += newAudio[i].soundClip.length;
						}

						if (newAudio != null)
						{
							tapePlayer.audMan.FlushQueue(true);
							for (int i = 0; i < newAudio.Length; i++)
								tapePlayer.audMan.QueueAudio(newAudio[i]);
						}
						else
						{
							tapePlayer.audMan.FlushQueue(true);
							tapePlayer.audMan.PlaySingle(tapePlayer.beep);
						}
						tapePlayer.audMan.PlaySingle(tapePlayer.audInsert);

						if (newEnumerator != null)
						{
							if (activateOriginalEnumeratorToo)
								tapePlayer.StartCoroutine(tapePlayer.Cooldown());
							tapePlayer.StartCoroutine(newEnumerator);
						}
						else
							tapePlayer.StartCoroutine(tapePlayer.Cooldown());

						newAudio = null;
						newEnumerator = null;
						activateOriginalEnumeratorToo = false;
					})
					)
				.MatchForward(false,
				new(OpCodes.Ldarg_0),
				new(CodeInstruction.LoadField(typeof(TapePlayer), "audMan"))
				)
				.RemoveInstructions(10) // Remove the audMan instructions
				.MatchForward(false,
				new(OpCodes.Ldarg_0),
				new(OpCodes.Ldarg_0),
				new(CodeInstruction.Call(typeof(TapePlayer), "Cooldown")),
				new(CodeInstruction.Call(typeof(MonoBehaviour), "StartCoroutine", [typeof(IEnumerator)])),
				new(OpCodes.Pop)
				)
				.RemoveInstructions(5) // remove this StartCoroutine

				.InstructionEnumeration();

			var _ = Transpiler;

			throw new System.NotImplementedException("stub");
		}

		static SoundObject[] newAudio = null;
		static IEnumerator newEnumerator = null;
		static bool activateOriginalEnumeratorToo = false;

		public static void CustomInsertItem(this TapePlayer tp, PlayerManager pm, EnvironmentController ec, IEnumerator newEnumerator = null, SoundObject[] newAudio = null, bool activateOgEnumerator = false)
		{
			TapePlayerPatch.newAudio = newAudio;
			TapePlayerPatch.newEnumerator = newEnumerator;
			activateOriginalEnumeratorToo = activateOgEnumerator;
			tp.OverridenInsertItem(pm, ec);
		}

	}
}
