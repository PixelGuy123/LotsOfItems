using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace LotsOfItems.CustomItems
{
	internal class Baldi_CustomAppleState(Baldi baldi, NpcState prevState, Sprite[] customEatSprites, WeightedSoundObject[] eatSounds = null, float eatTime = -1f, SoundObject thanksAudio = null) : Baldi_SubState(baldi, baldi, prevState)
	{
		const float minEatDelay = 0.015f, maxEatDelay = 0.07f;

		readonly protected SoundObject thanksAudio = thanksAudio;
		readonly protected Sprite eat1 = customEatSprites[0], eat2 = customEatSprites[1];
		protected float time = eatTime <= 0f ? baldi.appleTime : eatTime;
		readonly protected WeightedSoundObject[] eatSounds = eatSounds ?? baldi.eatSounds;
		float waitTimeBeforeEatingApple = 0f, eatDelay = 0f;
		public override void Enter()
		{
			base.Enter();
			baldi.animator.enabled = false; // Make sure to use customized sprites
			baldi.audMan.FlushQueue(true);

			baldi.spriteRenderer[0].sprite = customEatSprites.Length > 2 ? customEatSprites[2] : eat1;

			SoundObject activeAudio = thanksAudio ?? baldi.audAppleThanks;
			baldi.audMan.QueueAudio(activeAudio);

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
			if (baldi.spriteRenderer[0].sprite == eat2)
				baldi.audMan.PlaySingle(WeightedSoundObject.RandomSelection(eatSounds));
		}

		public override void Exit()
		{
			base.Exit();
			baldi.animator.enabled = true;
		}
	}

	[HarmonyPatch]
	internal static class AppleItemPatches
	{

		static readonly Dictionary<Items, Func<Baldi, Baldi_CustomAppleState>> appleFuncs = [];
		static readonly HashSet<Items> applesThatCantBePickedUp = [];

		public static ItemObject AddItemAsApple(this ItemObject itmObj, Func<Baldi, Baldi_CustomAppleState> func, bool canBePicked = true)
		{
			appleFuncs.Add(itmObj.itemType, func);
			if (!canBePicked)
				applesThatCantBePickedUp.Add(itmObj.itemType);
			return itmObj;
		}

		public static void TriggerBaldiApple(this Baldi baldi, Baldi_CustomAppleState state)
		{
			baldi.behaviorStateMachine.ChangeState(state);
			baldi.StopAllCoroutines();
			baldi.navigator.SetSpeed(0f);
			baldi.volumeAnimator.enabled = false;
		}

		[HarmonyPatch(typeof(Baldi_Chase), "OnStateTriggerStay")]
		[HarmonyPrefix]
		static bool MakeSureBaldiCanPickUpOtherApples(Baldi_Chase __instance, Collider other)
		{
			if (other.CompareTag("Player"))
			{
				__instance.baldi.looker.Raycast(other.transform, Vector3.Magnitude(__instance.baldi.transform.position - other.transform.position), out bool flag);
				if (flag)
				{
					PlayerManager component = other.GetComponent<PlayerManager>();
					ItemManager itm = component.itm;
					if (!component.invincible)
					{
						foreach (var apple in appleFuncs)
						{
							if (!applesThatCantBePickedUp.Contains(apple.Key) && itm.Has(apple.Key))
							{
								itm.Remove(apple.Key);
								__instance.baldi.TriggerBaldiApple(apple.Value(__instance.baldi));
								return false;
							}
						}
					}
				}
			}
			return true;
		}
	}
}
