using UnityEngine;
using System.Collections;
using LotsOfItems.ItemPrefabStructures;

namespace LotsOfItems.CustomItems.BSODAs
{
	public class ITM_RootBeer : ITM_GenericBSODA
	{
		[SerializeField]
		internal TemporaryBsoda bsodaPrefab;

		protected override void VirtualSetupPrefab(ItemObject itm)
		{
			base.VirtualSetupPrefab(itm);
			spriteRenderer.sprite = this.GetSprite("RootBeer_soda.png", spriteRenderer.sprite.pixelsPerUnit);
			Destroy(GetComponentInChildren<ParticleSystem>().gameObject);
		}

		public override bool Use(PlayerManager pm)
		{
			base.Use(pm);

			// Create angled projectiles
			CreateAngledProjectile(pm, 30f);
			CreateAngledProjectile(pm, -30f);

			return true;
		}

		private void CreateAngledProjectile(PlayerManager pm, float angleOffset)
		{
			// Calculate rotated direction
			Quaternion rotation = Quaternion.Euler(0f, angleOffset, 0f);
			Vector3 spawnPos = pm.transform.position;

			// Instantiate and configure projectile
			var projectile = Instantiate(bsodaPrefab, spawnPos, Quaternion.identity);
			projectile.Use(pm);
			projectile.transform.rotation = Quaternion.LookRotation(rotation * transform.forward);
		}
	}

	internal class TemporaryBsoda : ITM_GenericBSODA
	{
		float originalSpeed;

		public override bool Use(PlayerManager pm)
		{
			base.Use(pm);
			originalSpeed = speed;
			StartCoroutine(DecayRoutine());
			return true;
		}
		private IEnumerator DecayRoutine()
		{
			float elapsed = 0f;
			while (elapsed < time)
			{
				// Gradually reduce speed
				speed = Mathf.Lerp(originalSpeed, 0f, elapsed / time);
				elapsed += Time.deltaTime * ec.EnvironmentTimeScale;
				yield return null;
			}
			Destroy(gameObject);
		}
	}
}