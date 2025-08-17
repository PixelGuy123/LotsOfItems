using System.Collections;
using LotsOfItems.Plugin;
using UnityEngine;

namespace LotsOfItems.CustomItems.BSODAs
{
    public class ITM_RGBODA : ITM_GenericBSODA
    {
        [SerializeField]
        internal ITM_BSODA foamPrefab;

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

            if (RGBsodaVariant == 2)
            {
                entity.OnEntityMoveInitialCollision += (hit) =>
                {
                    transform.forward = Vector3.Reflect(transform.forward, hit.normal);
                    time -= 1f;
                };
            }

            else if (RGBsodaVariant == 1)
            {
                entity.OnEntityMoveInitialCollision += (hit) =>
                {
                    if (!hasEnded && hit.transform.CompareTag("Wall"))
                    {
                        hasEnded = true;
                        StartCoroutine(CreateAngleWait());
                    }
                };
            }

            if (flag && nextItem)
            {
                pm.itm.SetItem(nextItem, pm.itm.selectedItem);
                return false;
            }

            return flag;
        }

        IEnumerator CreateAngleWait()
        {
            yield return null; // Waits a frame to avoid the MoveNext() collection issue with PhysicsManager.UpdateAllEntities()
            CreateAngledProjectile(pm, 30f);
            CreateAngledProjectile(pm, -30f);
            VirtualEnd();
        }

        private void CreateAngledProjectile(PlayerManager pm, float angleOffset) // Note that this one does backwards
        {
            // Calculate rotated direction
            Quaternion rotation = Quaternion.Euler(0f, angleOffset, 0f);

            // Instantiate and configure projectile
            var projectile = Instantiate(foamPrefab, transform.position, Quaternion.identity);
            projectile.IndividuallySpawn(pm.ec, transform.position, transform.forward);
            projectile.transform.rotation = Quaternion.LookRotation(rotation * -transform.forward);
        }
    }

}