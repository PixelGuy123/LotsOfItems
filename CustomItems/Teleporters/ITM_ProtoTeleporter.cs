namespace LotsOfItems.CustomItems.Teleporters;

public class ITM_ProtoTeleporter : ITM_GenericTeleporter
{

    protected override void VirtualSetupPrefab(ItemObject itm)
    {
        base.VirtualSetupPrefab(itm);
        increaseFactor = 1f;
        baseTime = 8f;
        minTeleports = 8;
        maxTeleports = 8;
        startTimerAt0 = true;
        freezePlayer = false;
    }
}