using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace LotsOfItems.CustomItems
{
	internal class Baldi_CustomAppleState(Baldi baldi, NpcState prevState, Sprite[] customEatSprites, WeightedSoundObject[] eatSounds = null, float eatTime = -1f, SoundObject thanksAudio = null) : Baldi_SubState(baldi, baldi, prevState)
	{
		internal Baldi_CustomAppleState(Baldi baldi, NpcState prevState, Sprite[] customEatSprites, Action postAppleEat, WeightedSoundObject[] eatSounds = null, float eatTime = -1f, SoundObject thanksAudio = null) : this(baldi, prevState, customEatSprites, eatSounds, eatTime, thanksAudio)
		{
			postEating = postAppleEat;
		}
		const float minEatDelay = 0.015f, maxEatDelay = 0.07f;
		readonly protected Action postEating;
		readonly protected SoundObject thanksAudio = thanksAudio;
		readonly protected Sprite eat1 = customEatSprites[0], eat2 = customEatSprites[1];
		protected float time = eatTime <= 0f ? baldi.appleTime : eatTime;
		readonly protected WeightedSoundObject[] eatSounds = eatSounds ?? baldi.eatSounds;
		float waitTimeBeforeEatingApple = 0f, eatDelay = 0f;
		public override void Enter()
		{
			base.Enter();
			baldi.animator.enabled = false; // Make sure to use customized sprites
			baldi.AudMan.FlushQueue(true);

			baldi.spriteRenderer[0].sprite = customEatSprites.Length > 2 ? customEatSprites[2] : eat1;

			SoundObject activeAudio = thanksAudio ?? baldi.audAppleThanks;
			baldi.AudMan.QueueAudio(activeAudio);

			waitTimeBeforeEatingApple = activeAudio.subDuration + 0.185f;
		}

		public override void Update()
		{
			base.Update();
			time -= Time.deltaTime * npc.TimeScale;
			if (time <= 0f)
			{
				npc.behaviorStateMachine.ChangeState(previousState);
				return;
			}

			if (waitTimeBeforeEatingApple > 0f)
			{
				waitTimeBeforeEatingApple -= Time.deltaTime * npc.TimeScale;
				return;
			}

			if (eatDelay > 0f)
			{
				eatDelay -= Time.deltaTime * npc.TimeScale;
				return;
			}

			eatDelay += UnityEngine.Random.Range(minEatDelay, maxEatDelay);
			baldi.spriteRenderer[0].sprite = baldi.spriteRenderer[0].sprite == eat1 ? eat2 : eat1;
			if (baldi.spriteRenderer[0].sprite == eat2 && eatSounds.Length != 0)
				baldi.audMan.PlaySingle(WeightedSoundObject.RandomSelection(eatSounds));
		}

		public override void Exit()
		{
			base.Exit();
			baldi.animator.enabled = true;
			postEating?.Invoke();
		}
	}

	internal class Baldi_CustomNoEatAppleState(Baldi baldi, NpcState prevState, Sprite standingSprite, SoundObject talkAudio, Action postAppleEat) : Baldi_CustomAppleState(baldi, prevState, [standingSprite, standingSprite], postAppleEat, thanksAudio: talkAudio)
	{
		internal Baldi_CustomNoEatAppleState(Baldi baldi, NpcState prevState, Sprite standingSprite, SoundObject talkAudio) : this(baldi, prevState, standingSprite, talkAudio, null) { }
		public override void Update()
		{
			if (!baldi.AudMan.QueuedAudioIsPlaying)
				npc.behaviorStateMachine.ChangeState(previousState);
		}
	}

	[HarmonyPatch]
	internal static class AppleItemPatches
	{

		static readonly Dictionary<Items, Func<Baldi, Baldi_CustomAppleState>> appleFuncs = [];

		public static ItemObject AddItemAsApple(this ItemObject itmObj, Func<Baldi, Baldi_CustomAppleState> func)
		{
			appleFuncs.Add(itmObj.itemType, func);
			return itmObj;
		}

		public static void TriggerBaldiApple(this Baldi baldi, Baldi_CustomAppleState state)
		{
			baldi.behaviorStateMachine.ChangeState(state);
			baldi.StopAllCoroutines();
			baldi.navigator.SetSpeed(0f);
			baldi.volumeAnimator.enabled = false;
		}

		[HarmonyPatch(typeof(Baldi), "CaughtPlayer")]
		[HarmonyPrefix]
		static bool MakeSureBaldiCanPickUpOtherApples(Baldi __instance, PlayerManager player)
		{
			if (__instance.Character != Character.Baldi) // TeacherAPI support
				return true;

			foreach (var apple in appleFuncs)
			{
				if (player.itm.Has(apple.Key))
				{
					player.itm.Remove(apple.Key);
					__instance.TriggerBaldiApple(apple.Value(__instance));
					return false;
				}
			}
			return true;
		}

		[HarmonyPatch(typeof(Baldi), nameof(Baldi.Praise))]
		[HarmonyPrefix]
		private static void LongerPraise(ref float time, Baldi __instance)
		{
			for (int i = 0; i < Singleton<CoreGameManager>.Instance.TotalPlayers; i++)
			{
				PlayerManager player = Singleton<CoreGameManager>.Instance.GetPlayer(i);
				if (__instance.looker.PlayerInSight(player) && player.itm.Has(trophyItem))
				{
					time *= 2f;
					return;
				}
			}
		}

		internal static Items trophyItem;
	}
}
