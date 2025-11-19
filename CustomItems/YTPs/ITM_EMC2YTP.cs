using System.Collections;
using MTM101BaldAPI;
using UnityEngine;

namespace LotsOfItems.CustomItems.YTPs;

public class ITM_EMC2YTP : ITM_YTPs
{
    EnvironmentController ec;
    public float powerOffDuration = 25f;
    public override bool Use(PlayerManager pm)
    {
        ec = pm.ec;
        Singleton<CoreGameManager>.Instance.AddPoints(value, pm.playerNumber, playAnimation: true);

        // Get closest position to the player, since I can't select directly the Pickup from the item
        float lowestDistance = float.MaxValue;
        RoomController selRoom = ec.CellFromPosition(pm.transform.position).room;
        foreach (RoomController room in ec.rooms)
        {
            if (room.type == RoomType.Hall) continue;

            float distance = Vector3.Distance(pm.transform.position, room.transform.position);
            if (distance < lowestDistance)
            {
                selRoom = room;
                lowestDistance = distance;
            }
        }

        StartCoroutine(RestorePower(selRoom));
        return true;
    }

    private IEnumerator RestorePower(RoomController room)
    {
        room?.SetPower(false);
        yield return new WaitForSecondsEnvironmentTimescale(ec, powerOffDuration);
        room?.SetPower(true);
        Destroy(gameObject);
    }
}