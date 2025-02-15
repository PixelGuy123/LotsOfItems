using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.Rendering;

namespace LotsOfItems.Patches
{
	[HarmonyPatch(typeof(TapePlayer))]
	internal static class TapePlayerPatch
	{
		[HarmonyPatch("InsertItem")]
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
						//Debug.Log($"playing with: {newAudio} ref and enumerator ref: {newEnumerator}");

						if (newAudio == null)
							tapePlayer.time = tapePlayer.audInsert.soundClip.length; // Make sure it has a dynamic length to the environment muting thing
						else
						{
							tapePlayer.time = 0f;
							for (int i = 0; i < newAudio.Length; i++)
								tapePlayer.time += newAudio[i].soundClip.length;
						}

						if (newEnumerator != null)
						{
							tapePlayer.StartCoroutine(newEnumerator);
							if (activateOriginalEnumeratorToo)
								tapePlayer.StartCoroutine(tapePlayer.Cooldown());
						}
						else
							tapePlayer.StartCoroutine(tapePlayer.Cooldown());

						if (newAudio != null)
						{
							tapePlayer.audMan.FlushQueue(true);
							for (int i = 0; i < newAudio.Length; i++)
								tapePlayer.audMan.QueueAudio(newAudio[i]);
							tapePlayer.audMan.PlaySingle(tapePlayer.audInsert);
						}

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
