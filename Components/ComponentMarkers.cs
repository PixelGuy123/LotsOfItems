using System.Collections;
using MTM101BaldAPI;
using UnityEngine;

namespace LotsOfItems.Components;

public class Marker_StandardDoorSilenced : MonoBehaviour // Works with NoSquee variants
{
    StandardDoor door;
    Marker_AudioManagerMute muteComp;
    public void Initialize(StandardDoor door, int silencesCounter)
    {
        this.door = door;
        door.makesNoise = false;
        counter = silencesCounter;
        muteComp = door.gameObject.AddComponent<Marker_AudioManagerMute>(); // To make sure the door is fully silent regardless
    }

    void OnDestroy()
    {
        if (door)
        {
            door.makesNoise = true;
            if (muteComp)
                Destroy(muteComp);
        }
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

public class Marker_PrincipalOccupied : MonoBehaviour
{
    internal static Sprite[] drinkSprs;
    internal static SoundObject drinkSound;
    public void SetTimer(Principal pri, float time) =>
        StartCoroutine(Timer(pri, time));

    IEnumerator Timer(Principal pri, float t)
    {
        MovementModifier moveMod = new(Vector3.zero, 0f);
        pri.Navigator.Am.moveMods.Add(moveMod);
        Sprite normalSprite = pri.spriteRenderer[0].sprite;
        var renderer = pri.spriteRenderer[0];
        renderer.sprite = drinkSprs[0];
        var ec = pri.ec;
        while (t > 0f)
        {
            t -= ec.EnvironmentTimeScale * Time.deltaTime;

            if (Random.value > 0.13f)
            {
                int idx = Random.Range(0, drinkSprs.Length);
                if (idx == 1)
                    pri.audMan.PlaySingle(drinkSound);

                renderer.sprite = drinkSprs[idx];
            }

            yield return null;
        }
        pri.Navigator.Am.moveMods.Remove(moveMod);
        renderer.sprite = normalSprite;
        Destroy(this); // Destroys the marker
    }
}