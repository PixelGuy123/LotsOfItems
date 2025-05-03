using LotsOfItems.Components;
using LotsOfItems.ItemPrefabStructures;
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
        private Sprite arrowForward;
        [SerializeField]
        private float fastSpeed = 60f;
        [SerializeField]
        private float arrowYOffset = 1.2f;

        Direction preSelectedDirection = Direction.North;

        protected override void VirtualSetupPrefab(ItemObject itm)
        {
            base.VirtualSetupPrefab(itm);

            // ***** Visual Setup here ******

            animationComponent = gameObject.AddComponent<AnimationComponent>();
            animationComponent.renderers = [GetComponentInChildren<SpriteRenderer>()];

            // ***** Arrow Thing *****
            // TODO: Change to a ObjectCreationExtensions things
            arrowRenderer = new GameObject("DirectionArrow").AddComponent<SpriteRenderer>();
            arrowRenderer.transform.SetParent(transform);
            arrowRenderer.transform.localPosition = new Vector3(0, arrowYOffset, 0);
            arrowForward = this.GetSprite("GoldenBanana_ArrowForward.png", 32);
            arrowRenderer.sprite = arrowForward;
            arrowRenderer.enabled = false;
            speed = fastSpeed;
        }

        public override bool Use(PlayerManager pm)
        {
            preSelectedDirection = Directions.DirFromVector3(Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, 45f);
            return base.Use(pm);
        }

        internal override void AdditionalSpawnContribute()
        {
            base.AdditionalSpawnContribute();
            if (animationComponent != null)
                animationComponent.speed = 15f; // Faster rotation
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
            // TODO: make the arrow cahnge direction based on where the nana peel can go
        }

        internal override bool EntityTriggerStayOverride(Collider other) // Copy paste from NanaPeel, to have a direction selection :()
        {
            if (!ready || slipping) return false;
            Entity component = other.GetComponent<Entity>();
            if (component != null && component.Grounded && component.Velocity.magnitude > 0f)
            {
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
