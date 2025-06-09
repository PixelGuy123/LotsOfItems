using LotsOfItems.Components;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.BSODAs
{
    public class ITM_LoudSpeaker : Item, IItemPrefab
    {
        [SerializeField]
        private float noiseInterval = 5f, lifeTime = 30f;

        [SerializeField]
        private int noiseVal = 18;
        [SerializeField]
        private SoundObject bassNoise;
        [SerializeField]
        Entity entity;

        [SerializeField]
        internal ITM_BSODA foamPrefab;

        EnvironmentController ec;

        private float noiseTimer;

        public void SetupPrefab(ItemObject itm)
        {
            var worldSpeakerSheet = this.GetSpriteSheet("LoudSpeaker_World.png", 3, 3, 20f).Take(8);
            var rendererBase = ObjectCreationExtensions.CreateSpriteBillboard(worldSpeakerSheet[0]).AddSpriteHolder(out var renderer, 0f);
            rendererBase.transform.SetParent(transform);
            rendererBase.transform.localPosition = Vector3.zero;
            rendererBase.name = "Speaker";
            renderer.name = "SpeakerSprite";

            entity = gameObject.CreateEntity(2f, 1.5f, rendererBase.transform);
            gameObject.layer = LayerStorage.standardEntities;

            renderer.CreateAnimatedSpriteRotator(
                GenericExtensions.CreateRotationMap(8, worldSpeakerSheet)
            );

            bassNoise = this.GetSound("LoudSpeaker_Bass.wav", "LtsOItems_Vfx_LowBass", SoundType.Effect, Color.white);
        }

        public void SetupPrefabPost() { }

        public override bool Use(PlayerManager pm)
        {
            this.pm = pm;
            ec = pm.ec;
            transform.position = pm.transform.position;
            transform.forward = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward;
            entity.Initialize(ec, transform.position);

            noiseTimer = noiseInterval;
            return true;
        }

        void Update()
        {
            noiseTimer -= Time.deltaTime * ec.EnvironmentTimeScale;
            if (noiseTimer <= 0f)
            {
                Singleton<CoreGameManager>.Instance.audMan.PlaySingle(bassNoise);

                ec.MakeNoise(transform.position, noiseVal);
                var foam = Instantiate(foamPrefab, transform.position + transform.forward * 2f, Quaternion.identity);
                foam.entity.IgnoreEntity(entity, true); // Ignores the Loud Speaker
                foam.IndividuallySpawn(ec, foam.transform.position, transform.forward, false);

                noiseTimer += noiseInterval;
            }
            lifeTime -= Time.deltaTime * ec.EnvironmentTimeScale;
            if (lifeTime <= 0f)
            {
                Destroy(gameObject);
            }
        }

    }
}
