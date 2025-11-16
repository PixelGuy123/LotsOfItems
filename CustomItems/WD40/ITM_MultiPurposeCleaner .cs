using System.Collections;
using System.Collections.Generic;
using LotsOfItems.Components;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using UnityEngine;

namespace LotsOfItems.CustomItems.WD40;

public class ITM_MultiPurposeCleaner : ITM_NoSquee, IItemPrefab, ISlipperOwner // Inherits NoSquee to reuse its particles
{
    [SerializeField]
    internal Slipper slipperPre;
    [SerializeField]
    internal SlipperEffector slipperEffectorPre;
    SlipperController stainController;
    EnvironmentController ec;

    // ISlipperOwner
    Slipper ISlipperOwner.slipperPre { get => slipperPre; set => slipperPre = value; }
    SlipperEffector ISlipperOwner.slipperEffectorPre { get => slipperEffectorPre; set => slipperEffectorPre = value; }
    EnvironmentController ISlipperOwner.ec => ec;
    GameObject ISlipperOwner.gameObject => gameObject;

    public void SetupPrefab(ItemObject itm)
    {
        SlipperController.CreateSlipperPackPrefab(this, null); // Invisible clean
        sparkleParticlesPre = sparkleParticlesPre.SafeDuplicatePrefab(true);
        sparkleParticlesPre.name = "CleanerSparkle";
        var renderer = sparkleParticlesPre.GetComponentInChildren<ParticleSystemRenderer>();
        var newMat = new Material(renderer.material)
        {
            name = "CleanerSparkle",
            mainTexture = this.GetTexture("MultiPurposeCleaner_CleanerSparkle.png")
        };
        renderer.material = newMat;
        distance = 4;
    }
    public void SetupPrefabPost() { }

    public override bool Use(PlayerManager pm)
    {
        this.pm = pm;
        ec = pm.ec;
        stainController = SlipperController.CreateSlipperController(this);
        stainController.transform.position = transform.position;
        stainController.Initialize(this);
        // Find all cells in range
        DijkstraMap dijkstraMap = new(pm.ec, PathType.Nav, int.MaxValue);
        dijkstraMap.Calculate(distance + 1, true, IntVector2.GetGridPosition(pm.transform.position));

        foreach (Cell silencedCell in silencedCells)
        {
            silencedCell.SetSilence(true);
            sparkleParticelEmitters.Add(Instantiate(sparkleParticlesPre, silencedCell.ObjectBase));
            stainController.CreateStain(silencedCell);
        }

        Singleton<CoreGameManager>.Instance.audMan.PlaySingle(sound);
        StartCoroutine(NewTimer(pm.ec, time));
        return true;
    }

    public IEnumerator NewTimer(EnvironmentController ec, float time)
    {
        while (time > 0f)
        {
            time -= Time.deltaTime * ec.EnvironmentTimeScale;
            yield return null;
        }

        foreach (Cell silencedCell in silencedCells)
            silencedCell.SetSilence(value: false);


        foreach (Transform sparkleParticelEmitter in sparkleParticelEmitters)
            Destroy(sparkleParticelEmitter.gameObject);

        Destroy(stainController.gameObject);
        Destroy(gameObject);
    }
}