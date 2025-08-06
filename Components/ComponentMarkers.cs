using UnityEngine;

namespace LotsOfItems.Components;

public class Marker_StandardDoorSilenced : MonoBehaviour // Works with NoSquee variants
{
    StandardDoor door;
    public void Initialize(StandardDoor door, int silencesCounter)
    {
        this.door = door;
        door.makesNoise = false;
        counter = silencesCounter;
    }

    void OnDestroy()
    {
        if (door)
            door.makesNoise = true;
    }
    public int counter = 0;
}

public class Marker_BlockedStandardDoor : MonoBehaviour { } // Works with ITM_UniversalLock

[DisallowMultipleComponent] // Only allow one MuteMarker per object
public class Marker_AudioManagerMute : MonoBehaviour // Works with the AudioManagerPatch to mute the AudioManagers when necessary
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

public class Marker_WeakLockedDoor : MonoBehaviour // Works with ITM_WeakLock
{
    internal StandardDoor door;
    private float cooldown;
    int rattleCount = 0, rattlesToUnlock = 3;
    bool initialized = false;
    EnvironmentController ec;

    public void Initialize(EnvironmentController ec, StandardDoor door, float lockTime, int rattlesBeforeBreak)
    {
        initialized = true;
        this.door = door;
        cooldown = lockTime;
        rattlesToUnlock = rattlesBeforeBreak;
        this.ec = ec;
    }

    void Update()
    {
        if (!initialized) return;

        cooldown -= ec.EnvironmentTimeScale * Time.deltaTime;
        if (cooldown <= 0)
        {
            UnlockAndDestroy();
        }
    }

    public bool IncrementRattle()
    {
        if (++rattleCount >= rattlesToUnlock)
        {
            UnlockAndDestroy();
            return true;
        }
        door.audMan.PlaySingle(door.audDoorLocked);
        return false;
    }

    private void UnlockAndDestroy()
    {
        if (door != null && door.locked)
        {
            door.Unlock(); // Calling Unlock destroys this marker anyways.
        }
    }

    public void SelfDestroy()
    {
        Destroy(this);
    }
}