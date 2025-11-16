using System.Collections;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.BSODAs;

public class ITM_CreepyBaldiBalloon : Item, IItemPrefab, IEntityTrigger
{
    public float speed = 3.5f;
    public float lifetime = 20f;
    public float pushForce = 30f;
    public float finalScale = 2.5f;
    bool dead = false;

    EnvironmentController ec;

    [SerializeField]
    internal Entity entity;

    [SerializeField]
    internal SphereCollider collider;

    [SerializeField]
    internal SoundObject audInflate, audHit;

    [SerializeField]
    internal QuickExplosion pop;

    [SerializeField]
    internal AudioManager audMan;
    [SerializeField]
    SpriteRenderer renderer;

    Vector3 direction;

    public void SetupPrefab(ItemObject itm)
    {
        var rendererBase = ObjectCreationExtensions.CreateSpriteBillboard(this.GetSprite("CreepyBaldiBalloon_World.png", 25f)).AddSpriteHolder(out renderer, 0f);
        rendererBase.transform.SetParent(transform);
        rendererBase.transform.localPosition = Vector3.zero;
        rendererBase.name = "CreepyBaldiBalloon_Sprite";

        entity = gameObject.CreateEntity(2f, 2f, rendererBase.transform);

        collider = gameObject.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = 5f;

        audMan = gameObject.CreatePropagatedAudioManager(45f, 95f);
        audInflate = this.GetSound("CreepyBaldiBalloon_Inflate.wav", "LtsOItems_Vfx_Inflate", SoundType.Effect, Color.white);
        audHit = this.GetSoundNoSub("CreepyBaldiBalloon_Hit.wav", SoundType.Effect);

        pop = LotOfItemsPlugin.assetMan.Get<QuickExplosion>("quickPop");
    }
    public void SetupPrefabPost() { }

    public override bool Use(PlayerManager pm)
    {
        this.pm = pm;
        ec = pm.ec;
        entity.Initialize(ec, pm.transform.position);
        StartCoroutine(InflationAndMovement());
        entity.OnEntityMoveInitialCollision += (hit) => direction = Vector3.Reflect(direction, hit.normal);
        return true;
    }

    private IEnumerator InflationAndMovement()
    {
        Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audInflate);
        float t = 0f;
        while (t < audInflate.subDuration)
        {
            t += ec.EnvironmentTimeScale * Time.deltaTime;
            renderer.transform.localScale = Vector3.one * Mathf.Lerp(0f, finalScale, Mathf.Clamp01(t / audInflate.subDuration));
            transform.position = pm.transform.position + pm.transform.forward * 0.85f;
            yield return null;
        }

        direction = pm.transform.forward;

        float acceleration = 0f;
        float speed = 0f;
        float timer = lifetime;
        while (timer > 0f)
        {
            if (speed < this.speed)
            {
                acceleration += ec.EnvironmentTimeScale * Time.deltaTime;
                speed = Mathf.Min(this.speed, speed + acceleration);
            }
            timer -= ec.EnvironmentTimeScale * Time.deltaTime;
            entity.UpdateInternalMovement(direction * speed);
            yield return null;
        }

        // Pop!
        Instantiate(pop, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    public void EntityTriggerEnter(Collider other, bool hasCollided)
    {
        if (dead || !other.CompareTag("NPC") || other.gameObject == pm.gameObject) return;

        Entity hitEntity = other.GetComponent<Entity>();
        if (hitEntity && hitEntity != entity)
        {
            Vector3 direction = (hitEntity.transform.position - transform.position).normalized;
            hitEntity.AddForce(new Force(direction, pushForce, -pushForce * 0.5f));

            float reverseForce = pushForce * 0.35f;
            entity.AddForce(new Force(-direction, reverseForce, -reverseForce * 0.5f));

            audMan.PlaySingle(audHit);
        }
    }

    public void EntityTriggerStay(Collider other, bool hasCollided) { }

    public void EntityTriggerExit(Collider other, bool hasCollided) { }
}