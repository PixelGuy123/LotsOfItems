using System.Collections;
using System.Collections.Generic;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Patches;
using UnityEngine;

namespace LotsOfItems.CustomItems.WD40;

public class ITM_WDYesSquee : ITM_NoSquee, IItemPrefab // Inherits NoSquee to reuse its particles
{
    private readonly List<Cell> amplifiedCells = [];

    public void SetupPrefab(ItemObject itm)
    {
        var renderer = sparkleParticlesPre.GetComponentInChildren<ParticleSystemRenderer>();
        var newMat = new Material(renderer.material)
        {
            name = "ReverseSparkle",
            mainTexture = this.GetTexture("WDYesSquee_Sparkle.png")
        };
        renderer.material = newMat;
    }
    public void SetupPrefabPost() { }

    public override bool Use(PlayerManager pm)
    {
        // Find all cells in range
        DijkstraMap dijkstraMap = new(pm.ec, PathType.Nav, int.MaxValue);
        dijkstraMap.Calculate(distance + 1, true, IntVector2.GetGridPosition(pm.transform.position));
        amplifiedCells.AddRange(dijkstraMap.FoundCells());
        amplifiedCells.Add(pm.ec.CellFromPosition(pm.transform.position));

        foreach (Cell cell in amplifiedCells)
        {
            // Unmute all directions in the cell
            cell.SetSilence(false);

            sparkleParticelEmitters.Add(Instantiate(sparkleParticlesPre, cell.ObjectBase));

            // Add to the static patch list to amplify sound
            AmplifiedCellsPatch.amplifiedCells.Add(cell);
        }

        Singleton<CoreGameManager>.Instance.audMan.PlaySingle(sound);
        StartCoroutine(Timer(pm.ec, time));
        return true;
    }

    private IEnumerator NewTimer(EnvironmentController ec, float time)
    {
        while (time > 0f)
        {
            time -= Time.deltaTime * ec.EnvironmentTimeScale;
            yield return null;
        }

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        foreach (Cell cell in amplifiedCells)
        {
            cell.SetSilence(true); // Revert the unsilence performed
            AmplifiedCellsPatch.amplifiedCells.Remove(cell);
        }

        foreach (Transform sparkleParticelEmitter in sparkleParticelEmitters)
        {
            Destroy(sparkleParticelEmitter.gameObject);
        }
    }
}
