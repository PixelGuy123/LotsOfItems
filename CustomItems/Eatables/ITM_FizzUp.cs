using LotsOfItems.ItemPrefabStructures;
using UnityEngine;

namespace LotsOfItems.CustomItems.Eatables
{
    public class ITM_FizzUp : ITM_GenericZestyEatable
    {
        [SerializeField]
        internal Vector2 staminaGainMinMax = new(120f, 180f);

        protected override void VirtualSetupPrefab(ItemObject itemObject)
        {
            base.VirtualSetupPrefab(itemObject);
            speedMultiplier = 1.5f;
            affectorTime = 15f;
            speedAffectorAffectsRunSpeed = true;
            audSecondEatNoise = this.GetSoundNoSub("FizzUp_FizzingUp.wav", SoundType.Effect);
            speedAffectorAffectsWalkSpeed = true;
        }

        public override bool Use(PlayerManager pm)
        {
            staminaGain = Random.Range(staminaGainMinMax.x, staminaGainMinMax.y);
            return base.Use(pm);
        }
    }
}
