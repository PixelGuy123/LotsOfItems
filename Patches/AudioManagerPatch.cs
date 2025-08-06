using HarmonyLib;
using LotsOfItems.Components;
namespace LotsOfItems.Patches;

[HarmonyPatch]
internal static class AudioManagerPatch
{
    [HarmonyPatch(typeof(AudioManager), nameof(AudioManager.UpdateAudioDeviceVolume))]
    [HarmonyPrefix]
    static bool AutoMuteIfNeeded(AudioManager __instance)
    {
        if (__instance.GetComponent<Marker_AudioManagerMute>())
        {
            __instance.audioDevice.volume = 0f;
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(PropagatedAudioManager), nameof(PropagatedAudioManager.VirtualLateUpdate))]
    [HarmonyPrefix]
    static bool MutePropagationAsNeeded(PropagatedAudioManager __instance, ref float ___propagatedDistance, float ___maxDistance)
    {
        if (__instance.GetComponent<Marker_AudioManagerMute>())
        {
            __instance.audioDevice.volume = 0f;
            ___propagatedDistance = ___maxDistance;
            return false;
        }
        return true;
    }
}