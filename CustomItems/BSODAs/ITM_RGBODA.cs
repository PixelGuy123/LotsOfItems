using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using UnityEngine;

namespace LotsOfItems.CustomItems.BSODAs
{
    public class ITM_RGBODA : ITM_GenericBSODA
    {
        [SerializeField]
        internal RGBODA_Foam foamPrefab;

        [SerializeField]
        internal ushort RGBsodaVariant = 0; // 0 = blue, 1 = green, 2 = red

        [SerializeField]
        internal ItemObject nextItem;

        protected override void VirtualSetupPrefab(ItemObject itm)
        {
            base.VirtualSetupPrefab(itm);
            this.DestroyParticleIfItHasOne();
        }

        public override bool Use(PlayerManager pm)
        {
            this.pm = pm;
            bool flag = base.Use(pm);

            if (RGBsodaVariant == 1)
            {
                entity.OnEntityMoveInitialCollision += (hit) =>
                {
                    if (hit.transform.CompareTag("Wall"))
                    {
                        CreateAngledProjectile(pm, 30f);
                        CreateAngledProjectile(pm, -30f);
                    }
                    VirtualEnd();
                };
            }

            if (flag && nextItem)
            {
                pm.itm.SetItem(nextItem, pm.itm.selectedItem);
                return false;
            }

            return flag;
        }

        private void CreateAngledProjectile(PlayerManager pm, float angleOffset) // Note that this one does backwards
        {
            // Calculate rotated direction
            Quaternion rotation = Quaternion.Euler(0f, angleOffset, 0f);
            Vector3 spawnPos = pm.transform.position;

            // Instantiate and configure projectile
            var projectile = Instantiate(foamPrefab, spawnPos, Quaternion.identity);
            projectile.Spawn(pm.ec, spawnPos, transform.forward);
            projectile.transform.rotation = Quaternion.LookRotation(rotation * -transform.forward);
        }
    }

    public class RGBODA_Foam : ITM_BSODA
    {
        public void Spawn(EnvironmentController ec, Vector3 position, Vector3 rotation)
        {
            this.ec = ec;
            transform.position = position;
            transform.forward = rotation;
            entity.Initialize(ec, transform.position);
            spriteRenderer.SetSpriteRotation(Random.Range(0f, 360f));
            moveMod.priority = 1;
        }

    }
}