using System;
using System.Collections;
using System.Collections.Generic;
using LotsOfItems.Components;
using LotsOfItems.Plugin;
using PixelInternalAPI.Classes;
using UnityEngine;

namespace LotsOfItems.CustomItems.ChalkErasers
{
    // Chalk Bomb inherits from ITM_GenericNanaPeel to reuse the throwing mechanics.
    public class ITM_ChalkBomb : ITM_GenericNanaPeel
    {
        [SerializeField]
        internal SoundObject audExplode;

        [SerializeField]
        internal CoverCloud fogParticlesPre;

        [SerializeField]
        internal int explosionDistance = 45; // Defines a 5x5 area (radius 2)

        [SerializeField]
        internal float explosionForce = 85f,
            explosionAcceleration = -67.5f,
            explosionDuration = 5f,
            chalkGoAwayDelay = 7.5f,
            gravityIncrease = 9.25f;

        [SerializeField]
        internal SpriteRenderer renderer;

        [SerializeField]
        internal ParticleSystem explodeParticles;

        [SerializeField]
        internal LayerMask collisionLayer = LotOfItemsPlugin.onlyNpcPlayerLayers;

        readonly List<CoverCloud> blockers = [];

        protected override void VirtualSetupPrefab(ItemObject itm)
        {
            audExplode = LotOfItemsPlugin.assetMan.Get<SoundObject>("aud_explode");

            renderer = GetComponentInChildren<SpriteRenderer>();
            renderer.sprite = itm.itemSpriteLarge;
            renderer.transform.localScale = Vector3.one * 2f;
            renderer.name = "ChalkBombSprite";

            gravity = -2.5f;
            throwSpeed *= 1.5f;

            fogParticlesPre = Extensions.GetRawChalkParticleGenerator();
            var render = fogParticlesPre.particles.GetComponent<ParticleSystemRenderer>();

            explodeParticles = renderer.gameObject.AddComponent<ParticleSystem>();
            explodeParticles.GetComponent<ParticleSystemRenderer>().material = new Material(
                render.material
            )
            {
                mainTexture = render.material.mainTexture,
            };

            var main = explodeParticles.main;
            main.gravityModifierMultiplier = 0.05f;
            main.startLifetimeMultiplier = 1.8f;
            main.startSpeedMultiplier = 2f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startSize = new(0.95f, 1.65f);

            var emission = explodeParticles.emission;
            emission.rateOverTimeMultiplier = 225f;
            emission.enabled = false;

            var vel = explodeParticles.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.Local;
            vel.x = new(-15f, 15f);
            vel.y = vel.x;
            vel.z = vel.x;

            var col = explodeParticles.collision;
            col.enabled = true;
            col.type = ParticleSystemCollisionType.World;
            col.enableDynamicColliders = false;
        }

        public override bool Use(PlayerManager pm)
        {
            this.pm = pm;
            return base.Use(pm);
        }

        internal override bool OnCollisionOverride(RaycastHit hit)
        {
            var reflect = Vector3.Reflect(direction, hit.normal);
            direction = reflect;
            force.direction = new(reflect.x, reflect.z);
            return false;
        }

        internal override void OnFloorHit()
        {
            Explode();
        }

        internal override bool EntityTriggerStayOverride(Collider other) => false;

        private void Explode()
        {
            pm?.RuleBreak("Bullying", 3f);
            entity.SetFrozen(true);
            StartCoroutine(StayInPlaceEnumerator(transform.position));
            renderer.enabled = false;
            audioManager.FlushQueue(true);
            audioManager.QueueAudio(audExplode);

            Extensions.Explode(
                this,
                explosionDistance,
                collisionLayer,
                explosionForce,
                explosionAcceleration
            );

            float dist = explosionForce * 2f;
            foreach (var window in FindObjectsOfType<Window>())
            {
                if (Vector3.Distance(window.transform.position, transform.position) <= dist)
                    window.Break(false);
            }

            IntVector2 centerCell = IntVector2.GetGridPosition(transform.position);
            DijkstraMap dMap = new(ec, PathType.Nav, explosionDistance, []);
            dMap.Calculate(explosionDistance + 1, true, [centerCell]);

            foreach (Cell cell in dMap.FoundCells())
                ApplyChalkEffect(cell);
            ApplyChalkEffect(ec.CellFromPosition(centerCell));

            // Start a coroutine to later remove the chalk fog effect.
            StartCoroutine(ChalkFogTimer(explosionDuration));
        }

        private IEnumerator ChalkFogTimer(float duration)
        {
            var emission = explodeParticles.emission;
            emission.enabled = true;

            float timer = audExplode.subDuration - 1f;

            for (int i = 0; i < blockers.Count; i++)
                blockers[i].StartEndTimer(duration + timer - 1.5f); // Start the timer for each fog particle.

            while (timer > 0f)
            {
                timer -= Time.deltaTime * ec.EnvironmentTimeScale;
                yield return null;
            }

            emission.enabled = false;
            timer = duration;

            while (timer > 0f)
            {
                timer -= Time.deltaTime * ec.EnvironmentTimeScale;
                yield return null;
            }


            timer = chalkGoAwayDelay;
            while (timer > 0f)
            {
                timer -= Time.deltaTime * ec.EnvironmentTimeScale;
                yield return null;
            }

            Destroy(gameObject);
        }

        private IEnumerator StayInPlaceEnumerator(Vector3 ogPos)
        {
            while (true)
            {
                transform.position = ogPos;
                yield return null;
            }
        }

        void LateUpdate()
        {
            gravity += gravityIncrease * Time.deltaTime * ec.EnvironmentTimeScale;
            if (height > 10f)
                height = 10f;
        }

        private void ApplyChalkEffect(Cell cell)
        {
            var fog = Instantiate(fogParticlesPre);
            fog.transform.SetParent(transform);
            fog.transform.position = cell.FloorWorldPosition + Vector3.up * transform.position.y;
            fog.Ec = ec;
            blockers.Add(fog);
        }
    }
}
