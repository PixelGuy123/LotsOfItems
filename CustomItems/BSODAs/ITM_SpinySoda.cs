using System.Collections;
using LotsOfItems.Components;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using MTM101BaldAPI;
using MTM101BaldAPI.Registers;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.BSODAs
{
    public class ITM_SpinyBSODA : ITM_GenericNanaPeel
    {
        [SerializeField]
        private SpinySpike spikePrefab;
        [SerializeField]
        private SodaPuddle puddlePrefab;
        [SerializeField]
        internal AudioManager audMan;
        [SerializeField]
        internal SoundObject audExplode, audDropPuddle;
        [SerializeField]
        internal SpriteRenderer renderer;
        [SerializeField]
        internal float barrierWaitBeforeExplode = 7f, sodaPoolLifeTime = 10f, pushForce = 20f, spikeSpeed = 40f, puddleSpawnSpeed = 2.75f;

        private bool isActiveBarrier = false;

        protected override void VirtualSetupPrefab(ItemObject itm)
        {
            base.VirtualSetupPrefab(itm);

            renderer = GetComponentInChildren<SpriteRenderer>();
            renderer.sprite = itm.itemSpriteLarge;

            audSplat = this.GetSoundNoSub("SpinyBSODA_Splat.wav", SoundType.Effect);

            audMan = gameObject.CreatePropagatedAudioManager(35f, 120f);
            audDropPuddle = ((ITM_BSODA)ItemMetaStorage.Instance.FindByEnum(Items.Bsoda).value.item).sound;
            audExplode = LotOfItemsPlugin.assetMan.Get<SoundObject>("aud_explode");

            // Setup Spike Prefab
            var spikeObj = ObjectCreationExtensions.CreateSpriteBillboard(this.GetSprite("SpinyBSODA_Spike.png", 25f)).AddSpriteHolder(out var spikeRenderer, 0f);
            spikeObj.name = "SpinySpike";
            spikeObj.gameObject.ConvertToPrefab(true);
            spikePrefab = spikeObj.gameObject.AddComponent<SpinySpike>();
            spikePrefab.entity = spikeObj.gameObject.CreateEntity(1f, 1f, spikeRenderer.transform);
            spikePrefab.audMan = spikePrefab.gameObject.CreatePropagatedAudioManager(25f, 75f);
            spikePrefab.audHit = this.GetSoundNoSub("SpinyBSODA_SpikeHit.wav", SoundType.Effect);
            spikePrefab.renderer = spikeRenderer;

            // Setup Puddle Prefab
            var puddleObject = ObjectCreationExtensions.CreateSpriteBillboard(this.GetSprite("SpinyBSODA_Puddle.png", 10f), false).AddSpriteHolder(out var puddleRenderer, 0.01f);
            puddleObject.name = "GreenPuddle";
            puddleObject.gameObject.layer = LayerStorage.ignoreRaycast;
            puddleRenderer.transform.Rotate(90f, 0f, 0f);

            var puddleCollider = puddleObject.gameObject.AddComponent<BoxCollider>();
            puddleCollider.isTrigger = true;
            puddleCollider.size = new Vector3(4.95f, 2f, 4.95f);
            puddleCollider.center = Vector3.up * 5f;

            puddlePrefab = puddleObject.gameObject.AddComponent<SodaPuddle>();
            puddlePrefab.speedDebuff.movementMultiplier = 0.35f; // 65% slow
            puddleObject.gameObject.ConvertToPrefab(true);
        }

        internal override void OnFloorHit()
        {
            isActiveBarrier = true;
            StartCoroutine(BarrierAndExplode());
        }

        internal override bool EntityTriggerStayOverride(Collider other)
        {
            if (isActiveBarrier && other.isTrigger)
            {
                Entity entity = other.GetComponent<Entity>();
                if (entity != null && entity.Grounded)
                {
                    Vector3 pushDir = (entity.transform.position - transform.position).normalized;
                    entity.AddForce(new Force(pushDir, pushForce, pushForce * -0.5f));
                }
            }
            return false;
        }

        private IEnumerator BarrierAndExplode()
        {
            yield return new WaitForSecondsEnvironmentTimescale(ec, barrierWaitBeforeExplode);

            renderer.enabled = false;
            entity.SetFrozen(true);
            entity.SetInteractionState(false);
            audMan.PlaySingle(audDropPuddle);
            audMan.PlaySingle(audExplode);

            isActiveBarrier = false;
            // Spawn Spikes
            for (int i = 0; i < 8; i++)
            {
                Quaternion rotation = Quaternion.Euler(0, i * 45f, 0);
                SpinySpike spike = Instantiate(spikePrefab, transform.position, rotation);
                spike.Initialize(ec, rotation.eulerAngles, spikeSpeed);
            }

            // Spawn Puddle
            SodaPuddle puddle = Instantiate(puddlePrefab, transform.position.ZeroOutY(), Quaternion.identity);
            puddle.Initialize(sodaPoolLifeTime, ec);

            while (audMan.AnyAudioIsPlaying)
                yield return null;

            Destroy(gameObject);
        }
    }

    public class SpinySpike : MonoBehaviour, IEntityTrigger
    {
        public Entity entity;
        private EnvironmentController ec;
        private int bounces = 2;
        float rotationDegrees = Random.Range(0f, 360f);
        float speed, bounceTime = Random.Range(0f, 5f);
        Vector3 direction;
        bool active = false;

        [SerializeField]
        internal AudioManager audMan;

        [SerializeField]
        internal SoundObject audHit;

        [SerializeField]
        float bouncingHeight = 5f, bounceSpeed = 7.5f;

        [SerializeField]
        internal float slowDownTimer = 3f;

        [SerializeField]
        internal SpriteRenderer renderer;

        [SerializeField]
        internal MovementModifier slowDownMod = new(Vector3.zero, 0.8f);

        public void Initialize(EnvironmentController ec, Vector3 direction, float speed)
        {
            renderer.SetSpriteRotation(rotationDegrees);
            active = true;
            this.ec = ec;
            this.speed = speed;
            this.direction = direction;
            entity.Initialize(ec, transform.position);
            entity.OnEntityMoveInitialCollision += OnHit;
            entity.UpdateInternalMovement(direction * speed * ec.EnvironmentTimeScale);
        }

        private void OnHit(RaycastHit hit)
        {
            if (!active) return;

            if (bounces > 0)
            {
                direction = Vector3.Reflect(direction, hit.normal);
                bounces--;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void EntityTriggerEnter(Collider other)
        {
            if (!active) return;

            if (other.isTrigger && other.TryGetComponent<NPC>(out var npc))
            {
                Vector3 pushDir = (npc.transform.position - transform.position).normalized;
                npc.Navigator.Entity.AddForce(new Force(pushDir, speed * 0.5f, speed * -0.4f));
                Hide();
                audMan.PlaySingle(audHit);
                StartCoroutine(SlowNpc(npc));
            }
        }

        public void EntityTriggerStay(Collider other) { }

        public void EntityTriggerExit(Collider other) { }

        void Update()
        {
            if (!active)
            {
                entity.UpdateInternalMovement(Vector3.zero);
                return;
            }
            entity.UpdateInternalMovement(direction * speed * ec.EnvironmentTimeScale);
            bounceTime += Time.deltaTime * ec.EnvironmentTimeScale;
            entity.SetHeight(Mathf.Abs(Mathf.Sin(bounceTime * bounceSpeed)) * bouncingHeight);

            rotationDegrees = (bounceSpeed * rotationDegrees + Time.deltaTime * ec.EnvironmentTimeScale) % 360;
            renderer.SetSpriteRotation(rotationDegrees);
        }


        void Hide()
        {
            entity.SetFrozen(true);
            entity.SetInteractionState(false);
            entity.SetVisible(false);
            active = false;
        }

        private IEnumerator SlowNpc(NPC npc)
        {
            npc.Navigator.Am.moveMods.Add(slowDownMod);
            yield return new WaitForSecondsEnvironmentTimescale(ec, slowDownTimer);
            npc?.Navigator.Am.moveMods.Remove(slowDownMod);

            Destroy(gameObject);
        }
    }
}