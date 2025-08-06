using System.Collections.Generic;
using LotsOfItems.Components;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using MTM101BaldAPI.Registers;
using UnityEngine;

namespace LotsOfItems.CustomItems.WD40
{
    public class ITM_WODANoSquee : ITM_GenericBSODA
    {
        private readonly HashSet<NPC> affectedNpcs = [];

        protected override void VirtualSetupPrefab(ItemObject itm)
        {
            base.VirtualSetupPrefab(itm);
            speed *= 1.5f; // Faster spray
            this.DestroyParticleIfItHasOne();
            spriteRenderer.sprite = this.GetSprite("WODANoSquee_Spray.png", spriteRenderer.sprite.pixelsPerUnit);
            sound = this.GetSoundNoSub("WODANoSquee_Spray.wav", SoundType.Effect);
            sparkleParticlePre = ((ITM_NoSquee)ItemMetaStorage.Instance.FindByEnum(Items.Wd40).value.item).sparkleParticlesPre;
        }

        public override bool Use(PlayerManager pm)
        {
            entity.OnEntityMoveInitialCollision += OnSprayHit;
            return base.Use(pm);
        }

        private void OnSprayHit(RaycastHit hit)
        {
            var door = hit.transform.GetComponent<StandardDoor>();
            if (door != null && door.audMan != null && !door.GetComponent<Marker_StandardDoorSilenced>())
            {
                door.gameObject.AddComponent<Marker_StandardDoorSilenced>().Initialize(door, silenceCounter);
            }
        }

        public override bool VirtualEntityTriggerEnter(Collider other)
        {
            if (other.isTrigger && other.CompareTag("NPC"))
            {
                NPC npc = other.GetComponent<NPC>();
                if (npc != null && !affectedNpcs.Contains(npc))
                {
                    affectedNpcs.Add(npc);
                    var silencer = npc.gameObject.AddComponent<WODANpcSilencer>();
                    silencer.Initialize(ec, npcSilenceTimer, sparkleParticlePre);
                }
            }
            return true;
        }

        [SerializeField]
        internal int silenceCounter = 3;

        [SerializeField]
        internal float npcSilenceTimer = 15f;

        [SerializeField]
        internal Transform sparkleParticlePre;
    }

    public class WODANpcSilencer : MonoBehaviour
    {
        private EnvironmentController ec;
        private float duration;
        private readonly List<Cell> silencedCells = [];
        private readonly List<Transform> sparks = [];
        private Cell lastCell;
        private Transform sparkleParticlePre;

        public void Initialize(EnvironmentController ec, float duration, Transform particlesPre)
        {
            this.ec = ec;
            this.duration = duration;
            lastCell = ec.CellFromPosition(transform.position);
            sparkleParticlePre = particlesPre;
        }

        private void Update()
        {
            if (ec == null) return;

            duration -= Time.deltaTime * ec.EnvironmentTimeScale;
            if (duration <= 0f)
            {
                Destroy(this);
                return;
            }

            Cell currentCell = ec.CellFromPosition(transform.position);
            if (currentCell != lastCell && !currentCell.Null)
            {
                lastCell = currentCell;
                if (!currentCell.Silent)
                {
                    currentCell.SetSilence(true);
                    silencedCells.Add(currentCell);

                    var sparkle = Instantiate(sparkleParticlePre, currentCell.ObjectBase);
                    sparks.Add(sparkle);
                }
            }
        }

        private void OnDestroy()
        {
            foreach (var cell in silencedCells)
            {
                cell?.SetSilence(false);
            }
            foreach (var spark in sparks)
            {
                if (spark)
                    Destroy(spark.gameObject);
            }
            silencedCells.Clear();
        }
    }
}