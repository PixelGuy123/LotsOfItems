using LotsOfItems.Components;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.NanaPeels
{
    public class ITM_GoldenBanana : ITM_GenericNanaPeel, IClickable<int>
    {
        [SerializeField]
        private AnimationComponent animationComponent;
        [SerializeField]
        private SpriteRenderer arrowRenderer;
        [SerializeField]
        private Vector2 arrowOffset = new(4f, -0.99f);

        Direction preSelectedDirection = Direction.North;

        protected override void VirtualSetupPrefab(ItemObject itm)
        {
            base.VirtualSetupPrefab(itm);

            // ***** Visual Setup here ******

            animationComponent = gameObject.AddComponent<AnimationComponent>();
            animationComponent.animation = this.GetSpriteSheet("GoldenBanana_World.png", 6, 2, 25f);
            animationComponent.speed = 10f;
            animationComponent.renderers = [GetComponentInChildren<SpriteRenderer>()];
            animationComponent.renderers[0].sprite = animationComponent.animation[0];

            this.CreateClickableLink<int>()
            .CopyColliderAttributes(GetComponent<CapsuleCollider>());

            endHeight = 1f;
            startHeight = 6f;
            speed = 60f;

            entity.colliderRadius *= 0.925f;

            // ***** Arrow Thing *****

            arrowRenderer = ObjectCreationExtensions.CreateSpriteBillboard(this.GetSprite("GoldenBanana_Arrow.png", 25f), false);
            arrowRenderer.name = "ArrowRenderer";
            arrowRenderer.transform.SetParent(entity.rendererBase);
            arrowRenderer.enabled = false;


            // ***** Audio Setup *****
            audSplat = this.GetSound("GoldenBanana_KongSplat.wav", "LtsOItems_Vfx_OoohBanana_1", SoundType.Effect, Color.yellow);
            audSplat.additionalKeys = [
                new() { key = "LtsOItems_Vfx_OoohBanana_2", time = 0.744f },
                new() { key = "LtsOItems_Vfx_OoohBanana_3", time = 1.64f },
                new() { key = "LtsOItems_Vfx_OoohBanana_4", time = 1.944f },
                new() { key = "LtsOItems_Vfx_OoohBanana_5", time = 2.293f }
            ];
        }

        public override bool Use(PlayerManager pm)
        {
            preSelectedDirection = Directions.DirFromVector3(Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, 45f);
            return base.Use(pm);
        }

        internal override void AdditionalSpawnContribute()
        {
            base.AdditionalSpawnContribute();
            animationComponent.Initialize(ec);
            arrowRenderer.enabled = true;
            UpdateArrow(0);
        }

        public void Clicked(int player)
        {
            if (!ready || slipping) return;
            UpdateArrow(1);
        }

        public bool ClickableHidden() => !ready || slipping;
        public bool ClickableRequiresNormalHeight() => false;
        public void ClickableSighted(int player) { }
        public void ClickableUnsighted(int player) { }

        private void UpdateArrow(int offset)
        {
            int i = (((int)preSelectedDirection + offset) % Directions.Count + Directions.Count) % Directions.Count; // really weird logic, but should account for offset being negative
            int z = i; // to avoid an infinite while loop
            var cell = ec.CellFromPosition(transform.position);

            while (cell.HasWallInDirection((Direction)i))
            {
                i = (i + 1) % Directions.Count;
                if (i == z) // reached the original point
                    break; // Breaks and stays as it is
            }

            preSelectedDirection = (Direction)i;

            arrowRenderer.transform.position = entity.rendererBase.position + (preSelectedDirection.ToVector3() * arrowOffset.x + Vector3.up * arrowOffset.y);
            arrowRenderer.transform.rotation = Quaternion.Euler(90f, preSelectedDirection.ToDegrees(), 0f);
        }

        internal override bool EntityTriggerStayOverride(Collider other) // Copy paste from NanaPeel, to have a direction selection :()
        {
            if (!ready || slipping) return false;
            Entity component = other.GetComponent<Entity>();
            if (component != null && component.Grounded && component.Velocity.magnitude > 0f)
            {
                arrowRenderer.enabled = false;
                entity.Teleport(component.transform.position);
                component.ExternalActivity.moveMods.Add(moveMod);
                slippingEntity = component;
                slipping = true;
                ready = false;
                direction = preSelectedDirection.ToVector3();
                audioManager.FlushQueue(endCurrent: true);
                audioManager.QueueAudio(audSlipping);
                audioManager.SetLoop(val: true);
                if (!force.Dead)
                {
                    entity.RemoveForce(force);
                }
            }
            return false;
        }
    }
}
