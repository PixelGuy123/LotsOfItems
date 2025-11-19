using System.Collections;
using LotsOfItems.Patches;
using UnityEngine;

namespace LotsOfItems.CustomItems.YTPs;

public class ITM_BlankYTP : ITM_YTPs { } // Does nothing by itself | Marker

public class BlankYTP_Pickup : MonoBehaviour
{
    bool initialized = false;
    EnvironmentController ec;
    Pickup pickup;
    public void Initialize(Pickup pickup, EnvironmentController ec)
    {
        this.ec = ec;
        this.pickup = pickup;
        initialized = true;
        StartCoroutine(WaitForFirstFrame());

        pickup.OnItemCollected += OnItemSelfCollected;
    }

    IEnumerator WaitForFirstFrame()
    {
        while (Time.timeScale == 0f) // Wait game to unpause
            yield return null;

        // Registers itself to every other pickup available
        foreach (var p in ec.items)
        {
            if (p == pickup) continue;
            p.OnItemCollected += OnItemCollected;
        }
    }

    void OnItemCollected(Pickup pickup, int player)
    {
        // If itemobject exists and it is not itself, then let's copy it
        if (!initialized || !PickupPatches.lastPickedUpItem || PickupPatches.lastPickedUpItem.itemType != Items.Points || PickupPatches.lastPickedUpItem.itemType == blankYtp)
            return;

        this.pickup.AssignItem(PickupPatches.lastPickedUpItem);
    }

    void OnItemSelfCollected(Pickup pickup, int player) => Destroy(this); // If the pickup is collected, then destroy this component

    public Items blankYtp;
}