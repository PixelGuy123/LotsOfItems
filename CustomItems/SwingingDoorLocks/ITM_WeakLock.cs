using HarmonyLib;
using LotsOfItems.Components;
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
                if (door != null && !door.locked && !door.GetComponent<Marker_WeakLockedDoor>())
                {
                    door.LockTimed(lockDuration);
                    door.Shut();

                    door.gameObject.AddComponent<Marker_WeakLockedDoor>().Initialize(pm.ec, door, lockDuration, rattlesBeforeBreak);

                    Destroy(gameObject);
                    return true;
                }
            }
            Destroy(gameObject);
            return false; // Didn't hit a valid door or door was already locked/marked
        }
    }
}
