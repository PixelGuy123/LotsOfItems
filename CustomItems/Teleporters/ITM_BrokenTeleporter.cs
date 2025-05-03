using System.Collections;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.PlusExtensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.Teleporters
{
    public class ITM_BrokenTeleporter : ITM_GenericTeleporter
    {
        [SerializeField]
        internal float slowDuration = 10f;
        [SerializeField]
        internal float slowMultiplier = 0.5f;
        [SerializeField]
        internal float staminaMaxLoss = 40f;
        [SerializeField]
        internal float staminaRecoverRate = 2.5f;

        bool applied = false;

        public override bool Use(PlayerManager pm)
        {
            this.pm = pm;
            return base.Use(pm);
        }

        protected override void Teleport()
        {
            base.Teleport();
            if (applied) return;
            applied = true;
            pm.StartCoroutine(ApplySlowDebuff(pm));
            pm.StartCoroutine(ApplyStaminaDebuff(pm));
        }

        private IEnumerator ApplyStaminaDebuff(PlayerManager pm)
        {
            var statModifier = pm.GetMovementStatModifier();
            ValueModifier staminaMaxReduce = new(addend: -staminaMaxLoss);
            statModifier.AddModifier("staminaMax", staminaMaxReduce);
            while (staminaMaxReduce.addend < 0f)
            {
                if (pm.plm.Entity.InternalMovement.magnitude > 0.1f)
                    staminaMaxReduce.addend += staminaRecoverRate * Time.deltaTime * pm.PlayerTimeScale;
                yield return null;
            }
            statModifier.RemoveModifier(staminaMaxReduce);
        }

        private IEnumerator ApplySlowDebuff(PlayerManager pm)
        {
            MovementModifier slowMod = new(Vector3.zero, slowMultiplier);
            pm.Am.moveMods.Add(slowMod);

            float timer = 0f;
            while (timer < slowDuration)
            {
                timer += Time.deltaTime * pm.PlayerTimeScale;
                yield return null;
            }
            pm.Am.moveMods.Remove(slowMod);

        }
    }
}
