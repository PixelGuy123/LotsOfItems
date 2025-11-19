using System.Collections;
using LotsOfItems.Components;
using UnityEngine;

namespace LotsOfItems.CustomItems.Tapes;

public class ITM_PortalTape : ITM_GenericTape
{
    [SerializeField]
    internal MiniBlackHole blackHolePre;

    [SerializeField]
    internal float spawnInterval = 5f;

    protected override void VirtualSetupPrefab(ItemObject itm)
    {
        base.VirtualSetupPrefab(itm);
        blackHolePre = MiniBlackHole.CreatePrefab(15f, 1);
        useOriginalTapePlayerFunction = true;
    }

    protected override IEnumerator NewCooldown(TapePlayer tapePlayer)
    {
        yield return null;
        float spawnTimer = 0f;

        while (tapePlayer.active)
        {
            spawnTimer -= Time.deltaTime * tapePlayer.ec.EnvironmentTimeScale;

            if (spawnTimer <= 0f)
            {
                spawnTimer = spawnInterval;
                // Pick random cell
                Cell cell = tapePlayer.ec.RandomCell(false, false, true);
                var bh = Instantiate(blackHolePre, cell.CenterWorldPosition, Quaternion.identity);
                bh.Ec = tapePlayer.ec;
            }
            yield return null;
        }
    }
}
