using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using UnityEngine;

namespace LotsOfItems.CustomItems.BSODAs
{
    public class ITM_PSODA : ITM_GenericBSODA
    {
        [SerializeField]
        private float electrocuteDuration = 20f, slowMultiplier = 0.3f, shakeIntensity = 15f;

        protected override void VirtualSetupPrefab(ItemObject itm)
        {
            base.VirtualSetupPrefab(itm);
            spriteRenderer.sprite = this.GetSprite("PSODA_spray.png", spriteRenderer.sprite.pixelsPerUnit);
            this.DestroyParticleIfItHasOne();
            sound = this.GetSoundNoSub("PSODA_Sound.wav", SoundType.Effect);
        }

        public override bool VirtualEntityTriggerEnter(Collider other)
        {
            if (other.isTrigger && other.TryGetComponent<NPC>(out var npc))
            {
                if (!npc.gameObject.GetComponent<PSODA_Electrocute>())
                {
                    npc.gameObject.AddComponent<PSODA_Electrocute>().Init(electrocuteDuration, slowMultiplier, shakeIntensity);
                }
            }
            return true;
        }
    }

    public class PSODA_Electrocute : MonoBehaviour
    {
        private float shakeIntensity;
        private float timer;
        private NPC npc;
        private MovementModifier slowMod;

        public void Init(float duration, float slowMultiplier, float shakeIntensity)
        {
            this.shakeIntensity = shakeIntensity;
            npc = GetComponent<NPC>();
            timer = duration;
            slowMod = new MovementModifier(Vector3.zero, slowMultiplier);
            npc.Navigator.Am.moveMods.Add(slowMod);
        }

        void Update()
        {
            if (npc == null)
            {
                Destroy(this);
                return;
            }
            timer -= Time.deltaTime * npc.ec.EnvironmentTimeScale;
            // Shake effect
            slowMod.movementAddend = Random.insideUnitSphere * shakeIntensity;
            if (timer <= 0f)
            {
                npc.Navigator.Am.moveMods.Remove(slowMod);
                Destroy(this);
            }
        }

        void OnDestroy()
        {
            npc?.Navigator.Am.moveMods.Remove(slowMod);
        }
    }
}
