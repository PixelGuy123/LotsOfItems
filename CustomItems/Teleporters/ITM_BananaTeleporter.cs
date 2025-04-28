using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.Teleporters
{
	public class ITM_BananaTeleporter : ITM_GenericTeleporter
	{

		protected override void VirtualSetupPrefab(ItemObject itm)
		{
			base.VirtualSetupPrefab(itm);
			nanaPeelPrefab = GenericExtensions.FindResourceObject<ITM_NanaPeel>();
		}

		[SerializeField]
		internal ITM_NanaPeel nanaPeelPrefab;

		[SerializeField]
		internal float throwSpeed = 14f;

		[SerializeField]
		internal int minBananas = 1, maxBananas = 3;

		public override bool Use(PlayerManager pm)
		{
			this.pm = pm;
			return base.Use(pm);
		}

		protected override void Teleport()
		{
			base.Teleport();

			if (nanaPeelPrefab != null)
			{
				int num = Random.Range(minBananas, maxBananas + 1);
				float angleOffset = 360f / num;
				float startAngle = Random.Range(0f, 360f);

				for (int i = 0; i < num; i++)
				{
					float angle = startAngle + i * angleOffset * Mathf.Deg2Rad; // fixed offsets to make the nanapeels avoid being thrown on top of each other

					Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
					Instantiate(nanaPeelPrefab).Spawn(pm.ec, pm.transform.position, new(direction.x, 0f, direction.y), throwSpeed);
				}
			}
		}
	}
}
