using System.Collections;
using LotsOfItems.Components;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using MTM101BaldAPI;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.BSODAs
{
    public class ITM_RetroFireFlower : ITM_GenericBSODA
    {
        [SerializeField]
        private float bounceForce = 15f, gravity = 35f, bounceMinHeight = 1.5f;

        [SerializeField]
        private float disableDuration = 10f;

        [SerializeField]
        private float flickerDuration = 5f, flickerInterval = 0.1f;

        [SerializeField]
        private Vector2 goombaDeathForce = new(8.5f, 2.5f);

        [SerializeField]
        SpriteRenderer goombaDeathPrefab, respawnPrefab;

        [SerializeField]
        AnimationComponent animComp;

        [SerializeField]
        internal ItemObject nextItem = null;

        private float verticalSpeed = 0f;
        bool letalTouch = true;

        protected override void VirtualSetupPrefab(ItemObject itm)
        {
            base.VirtualSetupPrefab(itm);

            animComp = gameObject.AddComponent<AnimationComponent>();
            animComp.renderers = [spriteRenderer];
            animComp.animation = this.GetSpriteSheet("RetroFireFlower_Fireball.png", 2, 2, 20f);
            animComp.speed = 9.5f;

            this.DestroyParticleIfItHasOne();
            sound = this.GetSoundNoSub("RetroFireFlower_Shoot.wav", SoundType.Effect);

            entity.collisionLayerMask = LayerStorage.gumCollisionMask;
            speed *= 1.2f;
            time = 15f;

            goombaDeathPrefab = ObjectCreationExtensions.CreateSpriteBillboard(null);
            goombaDeathPrefab.gameObject.ConvertToPrefab(true);
            goombaDeathPrefab.name = "DeathSprite";

            var rb = goombaDeathPrefab.gameObject.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.mass *= 1.75f;

            goombaDeathPrefab.gameObject.CreatePropagatedAudioManager(55f, 120f)
            .AddStartingAudiosToAudioManager(false, this.GetSoundNoSub("RetroFireFlower_Hit.wav", SoundType.Effect));

            respawnPrefab = ObjectCreationExtensions.CreateSpriteBillboard(null);
            respawnPrefab.gameObject.ConvertToPrefab(true);
            respawnPrefab.name = "RespawnSprite";

            breaksRuleWhenUsed = false;
        }

        public override bool Use(PlayerManager pm)
        {
            bool flag = base.Use(pm);

            animComp.Initialize(ec);
            verticalSpeed = 5f; // Small initial upward velocity
            entity.OnEntityMoveInitialCollision += (hit) => transform.forward = Vector3.Reflect(transform.forward, hit.normal);
            spriteRenderer.SetSpriteRotation(0);
            launching = false; // Since the player is always ignored, this boolean needs to be false on spawn

            if (flag && nextItem)
            {
                pm.itm.SetItem(nextItem, pm.itm.selectedItem);
                return false;
            }

            return flag;
        }

        public override void VirtualUpdate()
        {
            verticalSpeed -= gravity * Time.deltaTime * ec.EnvironmentTimeScale;
            float height = entity.InternalHeight + verticalSpeed * Time.deltaTime * ec.EnvironmentTimeScale;

            if (height <= bounceMinHeight)
            {
                height = bounceMinHeight;
                verticalSpeed = bounceForce; // Apply bounce force upwards
            }
            entity.SetHeight(height);


            base.VirtualUpdate();
        }


        public override bool VirtualEntityTriggerEnter(Collider other)
        {
            if (letalTouch && other.isTrigger && other.CompareTag("NPC"))
            {
                NPC npc = other.GetComponent<NPC>();

                if (npc && npc.isActiveAndEnabled)
                {
                    pm?.RuleBreak("Bullying", 2.5f, 0.35f);
                    KillNpc(npc);
                    letalTouch = false;
                    VirtualEnd();
                    return false; // Prevent default BSODA push
                }
            }
            return false;
        }

        public override bool VirtualEntityTriggerExit(Collider other) => false; // No need for a trigger exit in this case

        private void KillNpc(NPC npc)
        {
            npc.Navigator.Entity.SetFrozen(true);
            npc.Navigator.Entity.SetVisible(false);
            npc.Navigator.Entity.SetInteractionState(false);
            npc.Navigator.Entity.SetBlinded(true);
            npc.Navigator.Entity.propagatedAudioManager.gameObject.AddComponent<Marker_AudioManagerMute>();

            // --- Goomba Death Animation ---
            if (npc.spriteRenderer[0])
            {
                var deathSpriteObject = Instantiate(goombaDeathPrefab, npc.spriteRenderer[0].transform.position, npc.spriteRenderer[0].transform.rotation);
                deathSpriteObject.sprite = npc.spriteRenderer[0].sprite;
                deathSpriteObject.SetSpriteRotation(180f);
                deathSpriteObject.transform.localScale = npc.spriteRenderer[0].transform.localScale; // Match scale

                Rigidbody rb = deathSpriteObject.GetComponent<Rigidbody>();
                rb.AddForce(Vector3.up * goombaDeathForce.y + transform.forward * goombaDeathForce.x, ForceMode.VelocityChange);
                Destroy(deathSpriteObject.gameObject, 6f); // Destroy after a few seconds
            }

            // Add NPC to disabled list and store coroutine reference
            npc.StartCoroutine(RevivalSequence(npc, disableDuration, flickerDuration, flickerInterval));
        }

        private IEnumerator RevivalSequence(NPC npc, float duration, float flickerDuration, float flickerInterval)
        {
            // Small note: since this is running as a coroutine in the npc component, no need for null checks

            // --- Wait for Disable Duration ---
            yield return new WaitForSecondsEnvironmentTimescale(ec, duration);


            if (npc.spriteRenderer[0])
            {
                var flickerRenderer = Instantiate(respawnPrefab, npc.spriteRenderer[0].transform.position, npc.spriteRenderer[0].transform.rotation);
                flickerRenderer.sprite = npc.spriteRenderer[0].sprite;
                flickerRenderer.transform.localScale = npc.spriteRenderer[0].transform.localScale; // Match scale

                float flickerTimer = flickerDuration;
                bool visible = true;
                while (flickerTimer > 0f)
                {
                    visible = !visible;
                    flickerRenderer.color = new(1f, 1f, 1f, visible ? 0.75f : 0.25f);
                    yield return new WaitForSecondsEnvironmentTimescale(ec, flickerInterval);
                    flickerTimer -= flickerInterval;
                }
                Destroy(flickerRenderer.gameObject); // Destroy flicker sprite
            }
            else
            {
                // If flicker sprite couldn't be created, just wait the duration
                yield return new WaitForSecondsEnvironmentTimescale(ec, flickerDuration);
            }

            // --- Re-enable NPC ---
            npc.Navigator.Entity.SetFrozen(false);
            npc.Navigator.Entity.SetVisible(true);
            npc.Navigator.Entity.SetInteractionState(true);
            npc.Navigator.Entity.SetBlinded(false);
            Destroy(npc.Navigator.Entity.propagatedAudioManager.GetComponent<Marker_AudioManagerMute>());
        }
    }
}
