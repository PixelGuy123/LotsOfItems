using HarmonyLib;
using UnityEngine;

namespace LotsOfItems.CustomItems.SwingingDoorLocks
{
    public class ITM_WeakLock : Item
    {
        [SerializeField]
        internal float lockDuration = 60f;

        [SerializeField]
        internal int rattlesBeforeBreak = 3;

        public override bool Use(PlayerManager pm)
        {
            if (Physics.Raycast(pm.transform.position,
                Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward,
                out var hit, pm.pc.reach, pm.pc.ClickLayers))
            {
                StandardDoor door = hit.transform.GetComponent<StandardDoor>();
                // Check if it's a standard door, it's not already locked, and doesn't already have our marker
                if (door != null && !door.locked && !door.GetComponent<WeakLockMarker>())
                {
                    door.LockTimed(lockDuration);
                    door.Shut();

                    door.gameObject.AddComponent<WeakLockMarker>().Initialize(pm.ec, door, lockDuration, rattlesBeforeBreak);

                    Destroy(gameObject);
                    return true;
                }
            }
            Destroy(gameObject);
            return false; // Didn't hit a valid door or door was already locked/marked
        }
    }

    public class WeakLockMarker : MonoBehaviour
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
}
