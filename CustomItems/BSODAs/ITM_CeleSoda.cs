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
    internal float balloonCooldown = 45f, minBalloonForce = 25f, maxBalloonForce = 45f;

    [SerializeField]
    QuickExplosion pop;
    [SerializeField]
    SoundObject additionalSound;

    protected override void VirtualSetupPrefab(ItemObject itm)
    {
        base.VirtualSetupPrefab(itm);
        additionalSound = this.GetSound("CeleSoda_Surprise.wav", "LtsOItems_Vfx_Surprise", SoundType.Effect, Color.yellow);
        spriteRenderer.sprite = this.GetSprite("CeleSoda_soda.png", spriteRenderer.sprite.pixelsPerUnit);
        this.DestroyParticleIfItHasOne();

        var balloonRef = GenericExtensions.FindResourceObject<PartyEvent>().balloon;

        balloons = new Balloon[balloonRef.Length];
        for (int i = 0; i < balloons.Length; i++)
        {
            var bal = balloonRef[i];
            bal = bal.SafeDuplicatePrefab(true);
            bal.name += "_Celebrative";
            bal.gameObject.AddComponent<CeleSoda_PushingBalloons>(); // Add pushing balloon feature
            balloons[i] = bal;
        }

        pop = LotOfItemsPlugin.assetMan.Get<QuickExplosion>("quickPop");
    }

    public override bool Use(PlayerManager pm)
    {
        this.pm = pm;

        Singleton<CoreGameManager>.Instance.audMan.PlaySingle(additionalSound);

        foreach (NPC npc in pm.ec.Npcs)
        {
            if (npc.Navigator.isActiveAndEnabled)
                npc.navigationStateMachine.ChangeState(new NavigationState_ForceTargetPosition(npc, 20, pm.transform.position));
        }

        for (int i = 0; i < balloonCount; i++)
        {
            var bal = Instantiate(balloons[Random.Range(0, balloons.Length)]);
            bal.Entity.IgnoreEntity(entity, true); // The balloon automatically ignores the BSODA
            bal.GetComponent<CeleSoda_PushingBalloons>().Initialize(gameObject, pm.ec, pop, Random.Range(minBalloonForce, maxBalloonForce), balloonCooldown);
            bal.Initialize(pm.ec.CellFromPosition(pm.transform.position).room);
            bal.transform.position = pm.transform.position;
        }

        return base.Use(pm);
    }
}

public class CeleSoda_PushingBalloons : MonoBehaviour, IEntityTrigger
{
    QuickExplosion quickPop;
    float pushForce;
    GameObject owner;
    EnvironmentController ec;
    bool initialized = false;
    public void Initialize(GameObject owner, EnvironmentController ec, QuickExplosion explosionPrefab, float pushForce, float cooldown)
    {
        this.ec = ec;
        this.owner = owner;
        quickPop = explosionPrefab;
        this.pushForce = pushForce;
        StartCoroutine(DestroyTimer(ec, cooldown));
        initialized = true;
    }
    public void EntityTriggerEnter(Collider other, bool validCollision)
    {
        if (!initialized || !validCollision) return;

        if (other.gameObject != owner && other.isTrigger && other.CompareTag("NPC") && other.TryGetComponent<Entity>(out var e))
        {
            // Debug.Log($"Touched something: {other.name}");
            Destroy(gameObject);
            e.AddForce(new((other.transform.position - transform.position).normalized, pushForce, -pushForce * 0.35f));
        }
    }

    public void EntityTriggerExit(Collider other, bool validCollision) { }

    public void EntityTriggerStay(Collider other, bool validCollision) { }

    private void OnDestroy()
    {
        if (!ec.CellFromPosition(transform.position).Null)
            Instantiate(quickPop, transform.position, Quaternion.identity);
    }

    IEnumerator DestroyTimer(EnvironmentController ec, float balloonCooldown)
    {
        yield return new WaitForSecondsEnvironmentTimescale(ec, balloonCooldown);
        Destroy(gameObject);
    }
}