using System.Collections;
using LotsOfItems.ItemPrefabStructures;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.SwingingDoorLocks
{
    public class ITM_StudentLock : Item, IItemPrefab
    {
        public void SetupPrefab(ItemObject itm)
        {
            lockedMaterial = new(GenericExtensions.FindResourceObject<SwingDoor>().overlayLocked[1])
            {
                name = "StudentLockOverlayShut",
                mainTexture = this.GetTexture("SwingDoor_StudentLock.png")
            };
        }

        public void SetupPrefabPost() { }

        [SerializeField]
        internal Material lockedMaterial;

        private RaycastHit hit;

        public override bool Use(PlayerManager pm)
        {
            this.pm = pm;
            if (Physics.Raycast(pm.transform.position,
                Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward,
                out hit, pm.pc.reach, pm.pc.ClickLayers))
            {
                var swingDoor = hit.transform.GetComponent<SwingDoor>();
                if (swingDoor != null && swingDoor.ItemFits(Items.DoorLock))
                {
                    // Copy overlayShut array
                    var originalOverlay = (Material[])swingDoor.overlayLocked.Clone();

                    for (int i = 0; i < swingDoor.overlayLocked.Length; i++)
                        swingDoor.overlayLocked[i] = lockedMaterial;

                    swingDoor.InsertItem(pm, pm.ec); // Locks the door

                    StartCoroutine(IgnorePlayerWhileLocked(swingDoor, pm));

                    swingDoor.overlayLocked = originalOverlay;
                    return true;
                }
            }
            Destroy(gameObject);
            return false;
        }

        private IEnumerator IgnorePlayerWhileLocked(SwingDoor door, PlayerManager pm)
        {
            var entity = pm.plm.Entity as PlayerEntity;
            foreach (var col in door.colliders)
                Physics.IgnoreCollision(col, entity.characterController, true);

            while (!door.locked)
                yield return null;

            foreach (var col in door.colliders)
                Physics.IgnoreCollision(col, entity.characterController, false);

            Destroy(gameObject);
        }
    }
}
