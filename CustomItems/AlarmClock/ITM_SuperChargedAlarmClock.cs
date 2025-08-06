using System.Collections;
using System.Collections.Generic;
using LotsOfItems.CustomItems;
using LotsOfItems.ItemPrefabStructures;
using MTM101BaldAPI;
using UnityEngine;

public class ITM_SuperChargedAlarmClock : ITM_GenericAlarmClock
{
    public float powerOffRadius = 250f; // 25 tiles
    public float powerOffDuration = 15f;

    protected override void VirtualSetupPrefab(ItemObject itm)
    {
        base.VirtualSetupPrefab(itm);
        noiseVal = 127;
        spriteRenderer.sprite = this.GetSprite("SuperChargedClock_World.png", spriteRenderer.sprite.pixelsPerUnit);
        clockSprite = [spriteRenderer.sprite];
        initSetTime = 1; // 30 seconds
        audRing = this.GetSound("SuperChargedAlarmClock_Alarm.wav", audRing.soundKey, SoundType.Effect, Color.white);
    }

    protected override void OnClockRing()
    {
        List<RoomController> affectedRooms = [];
        foreach (RoomController room in ec.rooms)
        {
            if (Vector3.Distance(transform.position, room.transform.position) <= powerOffRadius)
            {
                affectedRooms.Add(room);
                room.SetPower(false);
            }
        }
        StartCoroutine(RestorePower(affectedRooms));
    }

    public override bool AllowClickable() => false;

    protected override void Destroy() { } // No destroy after ring, only forcefully later

    private IEnumerator RestorePower(List<RoomController> rooms)
    {
        yield return new WaitForSecondsEnvironmentTimescale(ec, powerOffDuration);
        foreach (RoomController room in rooms)
        {
            room?.SetPower(true);
        }
        Destroy(gameObject);
    }
}