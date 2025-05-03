using UnityEngine;

namespace LotsOfItems.Components;
[DisallowMultipleComponent] // Only allow one MuteMarker per object
public class AudioManager_MuteMarker : MonoBehaviour // Works with the AudioManagerPatch to mute the AudioManagers when necessary
{
    AudioManager[] audMans;
    void Awake()
    {
        audMans = GetComponents<AudioManager>();
        if (audMans.Length == 0)
        {
            Destroy(this);
            Debug.LogWarning($"{GetType().Name} has been added to an object that has 0 AudioManagers! Destroying component...");
            return;
        }

        for (int i = 0; i < audMans.Length; i++)
            audMans[i].UpdateAudioDeviceVolume();
    }

    void OnDestroy()
    {
        for (int i = 0; i < audMans.Length; i++)
            audMans[i].UpdateAudioDeviceVolume();
    }
}
