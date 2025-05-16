using System;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using LotsOfItems.Plugin;
using UnityEngine;

namespace LotsOfItems.Patches;

[HarmonyPatch]
internal static class FieldTripRelatedPatch
{

    [HarmonyPatch(typeof(FieldTripEntranceRoomFunction), nameof(FieldTripEntranceRoomFunction.StartFieldTrip))]
    [HarmonyPrefix]
    static bool CheckForAdditionalBusPasses(FieldTripEntranceRoomFunction __instance, PlayerManager player, ref bool ___unlocked, AudioManager ___baldiAudioManager, SoundObject ___audNoPass)
    {
        if (___unlocked || player.itm.Has(Items.BusPass))
            return true;

        SoundObject speech = null;

        foreach (var pass in busPasses)
        {
            if (!player.itm.Has(pass.Key))
                continue;

            var result = pass.Value(player, false); // it is entrance (false)

            if (result.Key)
            {
                if (!___unlocked)
                {
                    player.itm.Remove(pass.Key);
                }

                Singleton<BaseGameManager>.Instance.CallSpecialManagerFunction(1, __instance.gameObject);
                ___unlocked = true;
                return false;
            }
            else if (!speech && result.Value)
                speech = result.Value;
        }

        ___baldiAudioManager.FlushQueue(true);
        ___baldiAudioManager.QueueAudio(!speech ? ___audNoPass : speech);
        return false;
    }

    readonly internal static Dictionary<Items, Func<PlayerManager, bool, KeyValuePair<bool, SoundObject>>> busPasses = [];
    //                                                        1: tells if it is allowed. 2: current soundObject to replace Baldi's speech.
    //                                                        For params: The pm instance; the Items enum used; boolean to tell whether it is entrance or Johnny

    [HarmonyPatch(typeof(ItemAcceptor), nameof(ItemAcceptor.ItemFits))]
    [HarmonyPostfix]
    static void RegisterLastUsedItem(Items item, bool __result)
    {
        if (__result)
            lastUsedPass = item;
    }

    [HarmonyPatch(typeof(StoreRoomFunction), nameof(StoreRoomFunction.GivenBusPass))]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> ChangeSequencerConstructor(IEnumerable<CodeInstruction> i) =>
    new CodeMatcher(i)
    .End()
    .MatchBack(
        false,
        new CodeMatch(CodeInstruction.Call(typeof(StoreRoomFunction), "BusPassSequencer"))
    )
    .SetInstruction(CodeInstruction.Call(typeof(FieldTripRelatedPatch), "AlternativeBusPassSequencer")) // Changes the IEnumerator

    .InstructionEnumeration();

    static IEnumerator AlternativeBusPassSequencer(StoreRoomFunction store)
    {
        Items savedLastUsedPass = lastUsedPass; // To avoid changing the item mid-talk
        yield return null;
        while (store.johnnyAudioManager.QueuedAudioIsPlaying)
        {
            yield return null;
        }
        if (!busPasses.TryGetValue(savedLastUsedPass, out var func)) // Try to get the last bus pass used to be sure it is a custom one
        {
            Singleton<BaseGameManager>.Instance.CallSpecialManagerFunction(3, store.gameObject);
            store.johnnyAudioManager.QueueAudio(store.audTripReturn);
            yield break;
        }
        // Change a bit the function over here (like, putting a manual transition first)
        var result = func(Singleton<CoreGameManager>.Instance.GetPlayer(0), true); // it is johnny (true)
        if (!result.Key && Singleton<BaseGameManager>.Instance is PitstopGameManager pitStopMan)
        {
            pitStopMan.StartCoroutine(pitStopMan.FieldTripTransition(false, false));
            store.johnnyAudioManager.QueueAudio(result.Value);

            while (store.johnnyAudioManager.QueuedAudioIsPlaying)
            {
                yield return null; // Expel from the store!
            }
            Singleton<CoreGameManager>.Instance.audMan.PlaySingle(LotOfItemsPlugin.assetMan.Get<SoundObject>("audBump"));

            var player = Singleton<CoreGameManager>.Instance.GetPlayer(0);
            Vector3 ogPos = player.transform.position,
            elevatorPos = pitStopMan.Ec.elevators[0].InsideCollider.transform.position;
            float t = 0;
            while (true)
            {
                player.plm.Entity.Teleport(Vector3.Lerp(ogPos, elevatorPos, t));
                if (t > 1f)
                    break;
                t += Time.deltaTime * pitStopMan.Ec.EnvironmentTimeScale * 50f;
                yield return null;
            }

            player.plm.Entity.Teleport(elevatorPos);


            yield break;
        }

        Singleton<BaseGameManager>.Instance.CallSpecialManagerFunction(3, store.gameObject);
        store.johnnyAudioManager.QueueAudio(store.audTripReturn);
        yield break;

    }

    static Items lastUsedPass = Items.None;

}