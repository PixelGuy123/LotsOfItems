using UnityEngine;

namespace LotsOfItems.CustomItems.Teleporters;

public class ITM_GatewayTeleporter : ITM_GenericTeleporter
{
    protected override void Teleport()
    {
        if (pm.ec.elevators.Count == 0)
        {
            base.Teleport();
            return;
        }

        Elevator targetElevator = null;
        for (int i = 0; i < pm.ec.elevators.Count; i++)
        {
            var el = pm.ec.elevators[i];
            if (el.IsOpen && el.Powered)
            {
                targetElevator = el;
                break;
            }
        }

        if (targetElevator == null)
            targetElevator = pm.ec.elevators[Random.Range(0, pm.ec.elevators.Count)];

        pm.Teleport(pm.ec.CellFromPosition(targetElevator.Door.position + targetElevator.Door.direction.ToIntVector2()).FloorWorldPosition);
        Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audTeleport);
    }
}