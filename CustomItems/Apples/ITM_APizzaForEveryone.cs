using System.Collections;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using MTM101BaldAPI;
using MTM101BaldAPI.Registers;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.Apples;

public class ITM_APizzaForEveryone : Item, IItemPrefab
{
    [SerializeField]
    internal ITM_PizzaSlice projectilePre;

    [SerializeField]
    internal ItemObject nextItem = null;

    [SerializeField]
    internal SoundObject audUse;

    public void SetupPrefab(ItemObject itm)
    {
        audUse = ((ITM_NanaPeel)ItemMetaStorage.Instance.FindByEnum(Items.NanaPeel).value.item).audEnd;

        // Setup projectile
        var sliceObj = ObjectCreationExtensions.CreateSpriteBillboard(this.GetSprite("APizzaForEveryone_PizzaSlice.png", 15f)).AddSpriteHolder(out var rend, 0f, LayerStorage.standardEntities);
        sliceObj.name = "PizzaSliceProjectile";
        sliceObj.gameObject.ConvertToPrefab(true);

        projectilePre = sliceObj.gameObject.AddComponent<ITM_PizzaSlice>();
        projectilePre.entity = sliceObj.gameObject.CreateEntity(1f, 1f, rend.transform);

        var eatingParticles = ParticleExtensions.GetNewParticleSystem(ParticleExtensions.GetRawChalkParticleGenerator(true).particles, out projectilePre.particleRendererToEnableForSomeReason);
        Destroy(eatingParticles.GetComponent<CoverCloud>());

        // Setup Dust Cloud visual
        projectilePre.particleRendererToEnableForSomeReason.material.mainTexture = this.GetTexture("APizzaForEveryone_Particles.png");

        var main = eatingParticles.main;
        main.startLifetime = 12.5f;
        main.gravityModifierMultiplier = 2f;
        main.startSpeed = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.duration = 15f;

        var emission = eatingParticles.emission;
        emission.enabled = false;
        emission.rateOverTime = 65f;

        var velocity = eatingParticles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.x = new(-5f, 5f);
        velocity.y = new(0.25f, 2.5f);
        velocity.z = velocity.x;

        var an = eatingParticles.textureSheetAnimation;
        an.enabled = true;
        an.numTilesX = 2;
        an.numTilesY = 2;
        an.animation = ParticleSystemAnimationType.WholeSheet;
        an.fps = 0f;
        an.timeMode = ParticleSystemAnimationTimeMode.FPS;
        an.cycleCount = 1;
        an.startFrame = new(0, 3); // 2x2

        eatingParticles.transform.SetParent(rend.transform, false);

        projectilePre.eatingParticles = eatingParticles;

        projectilePre.audEat = this.GetSoundNoSub("APizzaForEveryone_Eating.wav", SoundType.Voice);
        projectilePre.audMan = projectilePre.gameObject.CreatePropagatedAudioManager(35f, 60f);
    }

    public void SetupPrefabPost() { }

    public override bool Use(PlayerManager pm)
    {
        Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audUse);
        // Throw slice
        var rot = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward;
        var slice = Instantiate(projectilePre, pm.transform.position, Quaternion.LookRotation(rot));
        slice.Initialize(pm, pm.ec, rot);

        Destroy(gameObject);

        // Handle multi-use
        if (nextItem)
        {
            pm.itm.SetItem(nextItem, pm.itm.selectedItem);
            return false;
        }
        return true;
    }
}

public class ITM_PizzaSlice : MonoBehaviour, IEntityTrigger
{
    public Entity entity;
    public float speed = 40f;
    public float eatingTime = 40f;
    public AudioManager audMan;
    public SoundObject audEat;
    private EnvironmentController ec;
    PlayerManager pm;
    Vector3 direction;

    [SerializeField]
    internal ParticleSystem eatingParticles;

    [SerializeField]
    internal ParticleSystemRenderer particleRendererToEnableForSomeReason;

    private bool active = true;

    public void Initialize(PlayerManager pm, EnvironmentController ec, Vector3 dir)
    {
        this.pm = pm;
        this.ec = ec;
        direction = dir;
        entity.Initialize(ec, transform.position);
        entity.UpdateInternalMovement(direction * speed * ec.EnvironmentTimeScale);
        entity.OnEntityMoveInitialCollision += (hit) => Destroy(gameObject); // Die on wall
    }

    void Update()
    {
        if (active)
            entity.UpdateInternalMovement(direction * speed * ec.EnvironmentTimeScale);
        else
            entity.UpdateInternalMovement(Vector3.zero);
    }

    public void EntityTriggerEnter(Collider other, bool validCollision)
    {
        if (!validCollision || !active || !other.isTrigger) return;

        if (other.CompareTag("NPC") && other.TryGetComponent<NPC>(out var npc))
        {
            // Make NPC eat
            active = false;
            StartCoroutine(FreezeNPC(npc));

            // Hides pizza slice entirely
            entity.SetFrozen(true);
            entity.SetInteractionState(false);
            entity.SetVisible(false);
        }
    }
    public void EntityTriggerStay(Collider other, bool validCollision) { }
    public void EntityTriggerExit(Collider other, bool validCollision) { }

    IEnumerator FreezeNPC(NPC npc)
    {
        MovementModifier freezeMod = new(Vector3.zero, 0f);
        npc.Navigator.Am.moveMods.Add(freezeMod);

        npc.Navigator.Entity.IgnoreEntity(pm.plm.Entity, true);

        particleRendererToEnableForSomeReason.enabled = true;
        var emission = eatingParticles.emission;
        emission.enabled = true;

        float timer = eatingTime;
        while (timer > 0f)
        {
            if (!npc)
            {
                Destroy(gameObject);
                yield break;
            }
            if (Time.timeScale != 0f && Random.value <= 0.05f)
            {
                // audMan.pitchModifier = Random.Range(-0.25f, 0.25f);
                audMan.PlaySingle(audEat);
            }

            transform.position = npc.transform.position;
            timer -= Time.deltaTime * ec.EnvironmentTimeScale;
            yield return null;
        }

        npc.Navigator.Entity.IgnoreEntity(pm.plm.Entity, false);

        npc.Navigator.Am.moveMods.Remove(freezeMod);
        Destroy(gameObject);
    }
}
