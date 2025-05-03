using System.Collections;
using System.Collections.Generic;
using LotsOfItems.ItemPrefabStructures;
using MTM101BaldAPI;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.Eatables
{
    public class ITM_IceCreamSandwich : ITM_GenericZestyEatable
    {
        [SerializeField]
        private float eatingDuration = 60f;

        [SerializeField]
        private float minStaminaInterval = 0.25f, maxStaminaInterval = 2f;

        [SerializeField]
        private float staminaPerTick = 50f;

        [SerializeField]
        private float crumbLifetime = 10f;

        [SerializeField]
        private float crumbSlowFactor = 0.65f; // Multiplier for speed

        [SerializeField]
        private IceCreamSandwich_Crumb crumbPrefab;

        private readonly HashSet<Cell> cellsWithCrumbs = [];

        protected override void VirtualSetupPrefab(ItemObject itemObject)
        {
            base.VirtualSetupPrefab(itemObject); // Call base setup first

            // Create Crumb Prefab
            var crumbObject = ObjectCreationExtensions.CreateSpriteBillboard(
                this.GetSprite("IceCreamSandwich_Crumb.png", 6.5f),
                false
            ).AddSpriteHolder(out var crumbSprite, 0.05f, LayerStorage.ignoreRaycast);
            crumbSprite.name = "CrumbSprite";
            crumbSprite.gameObject.layer = 0;
            crumbSprite.transform.Rotate(90, 0, 0);

            crumbPrefab = crumbObject.gameObject.AddComponent<IceCreamSandwich_Crumb>();
            crumbPrefab.slowModifier = new(Vector3.zero, crumbSlowFactor);

            var collider = crumbPrefab.gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(4.75f, 1f, 4.75f); // Small trigger area
            collider.center = Vector3.up * 0.5f;

            crumbPrefab.gameObject.ConvertToPrefab(true);
            crumbPrefab.name = "CrumbPrefab";
        }

        // Prevent the base class from destroying the item immediately or applying its own timed effect
        protected override bool CanBeDestroyed() => false;

        public override bool Use(PlayerManager pm)
        {
            this.pm = pm;

            // Start the eating coroutine
            gauge = Singleton<CoreGameManager>.Instance.GetHud(pm.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, eatingDuration);
            StartCoroutine(EatingCoroutine());

            pm.RuleBreak("Eating", 2f);
            CreateCrumb(pm.ec.CellFromPosition(pm.transform.position));

            return base.Use(pm);
        }

        private IEnumerator EatingCoroutine()
        {
            float timer = eatingDuration;
            float staminaTickTimer = Random.Range(minStaminaInterval, maxStaminaInterval);

            while (timer > 0f)
            {
                float deltaTime = pm.ec.EnvironmentTimeScale * Time.deltaTime;
                timer -= deltaTime;
                staminaTickTimer -= deltaTime;

                if (staminaTickTimer <= 0f)
                {
                    // Add stamina
                    pm.plm.stamina = Mathf.Min(pm.plm.stamina + staminaPerTick, pm.plm.staminaMax); // Clamp to max

                    // Place crumbs
                    Cell currentCell = pm.ec.CellFromPosition(pm.transform.position);
                    if (!cellsWithCrumbs.Contains(currentCell)) // Check if cell exists and no recent crumbs
                    {
                        pm.RuleBreak("Eating", 2f);
                        CreateCrumb(currentCell);
                    }

                    // Reset tick timer
                    staminaTickTimer += Random.Range(minStaminaInterval, maxStaminaInterval);
                }

                gauge.SetValue(eatingDuration, timer);

                yield return null;
            }

            gauge.Deactivate();
            Destroy(gameObject);
        }

        public void CrumbExpired(Cell cell)
        {
            cellsWithCrumbs.Remove(cell);
        }

        void CreateCrumb(Cell cell)
        {
            Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audEat);
            var newCrumb = Instantiate(crumbPrefab, cell.FloorWorldPosition, Quaternion.identity);
            newCrumb.Initialize(this, cell); // Pass reference to this item and cell
            newCrumb.lifetime = crumbLifetime;
            cellsWithCrumbs.Add(cell); // Mark cell as having crumbs
        }
    }

    // Component for the crumbs left behind
    public class IceCreamSandwich_Crumb : MonoBehaviour
    {
        internal float lifetime;
        EnvironmentController ec;
        [SerializeField]
        internal MovementModifier slowModifier;
        private ITM_IceCreamSandwich ownerItem;
        private Cell myCell;
        private readonly HashSet<Entity> affectedEntities = [];

        public void Initialize(ITM_IceCreamSandwich owner, Cell cell)
        {
            ownerItem = owner;
            myCell = cell;
            ec = owner.pm.ec;
        }

        void OnTriggerEnter(Collider other)
        {
            Entity entity = other.GetComponent<Entity>();
            if (entity != null && !affectedEntities.Contains(entity))
            {
                entity.ExternalActivity.moveMods.Add(slowModifier);
                affectedEntities.Add(entity);
            }
        }

        void OnTriggerExit(Collider other)
        {
            Entity entity = other.GetComponent<Entity>();
            if (entity != null && affectedEntities.Contains(entity))
            {
                entity.ExternalActivity.moveMods.Remove(slowModifier);
                affectedEntities.Remove(entity);
            }
        }

        void Update()
        {
            lifetime -= ec.EnvironmentTimeScale * Time.deltaTime;
            if (lifetime < 0f)
                Destroy(gameObject);
        }

        void OnDestroy()
        {
            // Remove modifier from any remaining entities when destroyed
            foreach (Entity entity in affectedEntities)
            {
                entity?.ExternalActivity.moveMods.Remove(slowModifier);
            }
            affectedEntities.Clear();
            ownerItem?.CrumbExpired(myCell);
        }
    }
}