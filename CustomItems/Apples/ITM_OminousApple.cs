using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using MTM101BaldAPI.Registers;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.Apples
{
    public class ITM_OminousApple : Item, IItemPrefab, IEntityTrigger
    {
        [SerializeField]
        private float gravityScale = 15f, throwForce = 45f, eatTime = 30f, maxHeight = 9f, inAirRotSpeed = 55f;
        [SerializeField]
        private Entity entity;

        [SerializeField]
        private Sprite[] baldiSprites;

        [SerializeField]
        SpriteRenderer renderer;

        [SerializeField]
        SoundObject audThrow;

        EnvironmentController ec;


        Vector3 direction;
        private float verticalSpeed = 0f;

        public void SetupPrefab(ItemObject itm)
        {
            var rendererBase = ObjectCreationExtensions.CreateSpriteBillboard(itm.itemSpriteLarge).AddSpriteHolder(out renderer, 0f);
            rendererBase.transform.SetParent(transform);
            rendererBase.transform.localPosition = Vector3.zero;
            rendererBase.name = "AppleSprite";

            entity = gameObject.CreateEntity(2f, 2f, rendererBase.transform);
            audThrow = ((ITM_NanaPeel)ItemMetaStorage.Instance.FindByEnum(Items.NanaPeel).value.item).audSplat;
            baldiSprites = this.GetSpriteSheet("BaldiSprite_OminousApple.png", 2, 1, NPCMetaStorage.Instance.Get(Character.Baldi).value.spriteRenderer[0].sprite.pixelsPerUnit);
        }
        public void SetupPrefabPost() { }

        public override bool Use(PlayerManager pm)
        {
            Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audThrow);
            renderer.SetSpriteRotation(Random.Range(0f, 360f));
            direction = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward;
            ec = pm.ec;
            // Enable physics
            entity.Initialize(pm.ec, pm.transform.position);
            verticalSpeed = throwForce; // Initial upward speed
            entity.OnEntityMoveInitialCollision += (hit) => direction = Vector3.Reflect(direction, hit.normal);

            return true;
        }

        private void Update()
        {
            renderer.RotateBy(inAirRotSpeed * ec.EnvironmentTimeScale * Time.deltaTime);

            // Apply custom gravity
            verticalSpeed -= gravityScale * Time.deltaTime * ec.EnvironmentTimeScale;
            entity.UpdateInternalMovement(direction * ec.EnvironmentTimeScale * throwForce);
            float height = entity.InternalHeight + verticalSpeed * Time.deltaTime * ec.EnvironmentTimeScale;
            if (height > maxHeight)
            {
                height = maxHeight;
                verticalSpeed = Mathf.Min(verticalSpeed, 0f);
            }
            entity.SetHeight(height);

            if (entity.InternalHeight <= 0f)
            {
                entity.SetHeight(0f);
                verticalSpeed = 0f;
                Destroy(gameObject);
            }
        }

        public void EntityTriggerEnter(Collider other, bool hasTouched)
        {
            // Check if we hit Baldi
            if (other.isTrigger && other.CompareTag("NPC") && other.TryGetComponent<Baldi>(out var baldi) && baldi.Character == Character.Baldi)
            {
                // Create custom apple state
                baldi.TriggerBaldiApple(new Baldi_CustomAppleState(
                    baldi,
                    baldi.behaviorStateMachine.CurrentState,
                    baldiSprites, // Use animation sprites
                    null, // Use default eat sounds
                    eatTime, // Custom eat duration
                    null  // Use default thanks audio
                ));

                Destroy(gameObject);
            }
        }
        public void EntityTriggerExit(Collider other, bool hasTouched) { }
        public void EntityTriggerStay(Collider other, bool hasTouched) { }
    }
}