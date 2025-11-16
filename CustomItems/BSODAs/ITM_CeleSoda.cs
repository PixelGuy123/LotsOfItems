using System.Collections;
using LotsOfItems.Components;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using MTM101BaldAPI;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.BSODAs;

public class ITM_CeleSoda : ITM_GenericBSODA
{
    [SerializeField]
    private Balloon[] balloons;

    [SerializeField]
    internal int balloonCount = 10;

    [SerializeField]
    internal float balloonCooldown = 15f;

    protected override void VirtualSetupPrefab(ItemObject itm)
    {
        base.VirtualSetupPrefab(itm);
        sound = this.GetSound("CeleSoda_Surprise.wav", "LtsOItems_Vfx_Surprise", SoundType.Effect, Color.yellow);
        spriteRenderer.sprite = this.GetSprite("CeleSoda_soda.png", spriteRenderer.sprite.pixelsPerUnit);
        this.DestroyParticleIfItHasOne();

        balloons = GenericExtensions.FindResourceObject<PartyEvent>().balloon;
    }

    public override bool Use(PlayerManager pm)
    {
        this.pm = pm;

        foreach (NPC npc in pm.ec.Npcs)
        {
            if (npc.Navigator.isActiveAndEnabled)
                npc.navigationStateMachine.ChangeState(new NavigationState_ForceTargetPosition(npc, 20, pm.transform.position));
        }

        for (int i = 0; i < balloonCount; i++)
        {
            var bal = Instantiate(balloons[Random.Range(0, balloons.Length)]);
            bal.transform.position = pm.transform.position;
            bal.Initialize(pm.ec.CellFromPosition(pm.transform.position).room);
            bal.StartCoroutine(DestroyTimer(bal));
        }

        return base.Use(pm);
    }

    IEnumerator DestroyTimer(Balloon bal)
    {
        yield return new WaitForSecondsEnvironmentTimescale(pm.ec, balloonCooldown);
        Destroy(bal.gameObject);
    }
}