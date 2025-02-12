using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace LotsOfItems.Patches
{
	[HarmonyPatch(typeof(TapePlayer))]
	internal static class TapePlayerPatch
	{
		[HarmonyPatch("InsertItem")]
		[HarmonyReversePatch]
		public static void OverridenInsertItem(this TapePlayer instance, PlayerManager pm, EnvironmentController ec, IEnumerator newEnumerator = null, SoundObject[] newAudio = null)
		{
			IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> i) =>
				new CodeMatcher(i)
				.MatchForward(false,
				new(OpCodes.Ldarg_0),
				new(OpCodes.Ldarg_0),
				new(CodeInstruction.Call(typeof(TapePlayer), "Cooldown")),
				new(CodeInstruction.Call(typeof(MonoBehaviour), "StartCoroutine", [typeof(IEnumerator)])),
				new(OpCodes.Pop)
				)
				.RemoveInstructions(5) // remove this StartCoroutine
				.InstructionEnumeration();

#pragma warning disable CS8321
			void Prefix()
			{
				if (newAudio == null)
					instance.time = instance.audInsert.soundClip.length; // Make sure it has a dynamic length to the environment muting thing
				else
				{
					instance.time = 0f;
					for (int i = 0; i < newAudio.Length; i++)
						instance.time += newAudio[i].soundClip.length;
				}
				instance.StartCoroutine(newEnumerator ?? instance.Cooldown());
			}

			void Postfix()
			{
				if (newAudio == null) return;
				instance.audMan.FlushQueue(true);
				for (int i = 0; i < newAudio.Length; i++)
					instance.audMan.QueueAudio(newAudio[i]);
				instance.audMan.PlaySingle(instance.beep);
			}
#pragma warning restore CS8321

			var _ = Transpiler;

			throw new System.NotImplementedException("stub");
		}
	}
}
