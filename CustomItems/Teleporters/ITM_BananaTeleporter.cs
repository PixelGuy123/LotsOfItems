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
				Vector2 randomRot = Random.insideUnitCircle.normalized;
				Instantiate(nanaPeelPrefab).Spawn(pm.ec, pm.transform.position, new(randomRot.x, 0f, randomRot.y), throwSpeed);
			}
		}
	}
}
