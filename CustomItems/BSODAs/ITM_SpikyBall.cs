using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.BSODAs;

public class ITM_SpikyBall : ITM_GenericBSODA
{
    [SerializeField]
    private float bounceForce = 15f, gravity = 35f, bounceMinHeight = 2f;
    [SerializeField]
    private int maxHits = 15;
    [SerializeField]
    private float lifeTime = 60f;
    [SerializeField]
    private float pushForce = 50f;
    [SerializeField]
    private float pushAcceleration = -25f;
    [SerializeField]
    private SoundObject audBounce;
    [SerializeField]
    private AudioManager audMan;
    [SerializeField]
    QuickExplosion quickPop;

    private int hits = 0;
    private float verticalSpeed = 0f;

    protected override void VirtualSetupPrefab(ItemObject itm)
    {
        base.VirtualSetupPrefab(itm);
        speed = 40f;
        time = lifeTime;
        audBounce = this.GetSound("SpikyBall_Bounce.wav", "LtsOItems_Vfx_Bounce", SoundType.Effect, Color.white);
        quickPop = LotOfItemsPlugin.assetMan.Get<QuickExplosion>("quickPop");
        sound = LotOfItemsPlugin.assetMan.Get<SoundObject>("audThrow");

        spriteRenderer.sprite = this.GetSprite("SpikyBall_World.png", 30f);
        this.DestroyParticleIfItHasOne();
        entity.collisionLayerMask = LayerStorage.gumCollisionMask;

        GetComponent<CapsuleCollider>().radius *= 0.5f; // Halfen the size of this collision, the ball is way too small for it

        audMan = gameObject.CreatePropagatedAudioManager(25f, 150f);

        breaksRuleWhenUsed = false;
    }

    public override bool Use(PlayerManager pm)
    {
        entity.OnEntityMoveInitialCollision += OnBounce;
        verticalSpeed = 5f;
        return base.Use(pm);
    }

    private void OnBounce(RaycastHit hit)
    {
        if (hasEnded) return;

        transform.forward = Vector3.Reflect(transform.forward, hit.normal);
        audMan.PlaySingle(audBounce);
        hits++;
        if (hits >= maxHits)
        {
            VirtualEnd();
        }
    }

    public override void VirtualUpdate()
    {
        base.VirtualUpdate();
        verticalSpeed -= gravity * Time.deltaTime * ec.EnvironmentTimeScale;
        float height = entity.InternalHeight + verticalSpeed * Time.deltaTime * ec.EnvironmentTimeScale;

        if (height <= bounceMinHeight)
        {
            height = bounceMinHeight;
            verticalSpeed = bounceForce; // Apply bounce force upwards
        }
        entity.SetHeight(height);
    }

    public override bool VirtualEntityTriggerEnter(Collider other, bool validCollision)
    {
        if (!validCollision || !other.isTrigger || (!other.CompareTag("Player") && !other.CompareTag("NPC")) || hasEnded) return true;

        var targetEntity = other.GetComponent<Entity>();
        if (targetEntity)
        {
            Vector3 pushDir = (targetEntity.transform.position - transform.position).normalized;
            targetEntity.AddForce(new Force(pushDir, pushForce, pushAcceleration));
            if (++hits >= maxHits)
            {
                VirtualEnd();
            }
        }
        return false;
    }

    public override bool VirtualEntityTriggerExit(Collider other, bool validCollision) => false;

    protected override void VirtualEnd()
    {
        base.VirtualEnd();
        Instantiate(quickPop, transform.position, Quaternion.identity);
    }
}