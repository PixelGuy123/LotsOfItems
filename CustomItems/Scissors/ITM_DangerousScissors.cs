using System.Collections;
using UnityEngine;
using LotsOfItems.ItemPrefabStructures;
using PixelInternalAPI.Extensions;

namespace LotsOfItems.CustomItems.Scissors
{
	public class ITM_DangerousScissors : ITM_Scissors, IItemPrefab
	{
		[SerializeField]
		internal Items item;  // Should be set to the appropriate enum value
		[SerializeField]
		internal PropagatedAudioManager audMan;
		[SerializeField]
		internal float stabCooldown = 20f;

		readonly MovementModifier moveMod = new(Vector3.zero, 0f);

		public void SetupPrefab(ItemObject itm)
		{
			audMan = gameObject.CreatePropagatedAudioManager(65f, 85f);
			item = itm.itemType;
		}
		public void SetupPrefabPost() { }

		public override bool Use(PlayerManager pm)
		{
			this.pm = pm;
			if (Physics.Raycast(pm.transform.position,
				Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward,
				out RaycastHit hit, pm.pc.reach))
			{
				if (hit.transform.CompareTag("NPC"))
				{
					NPC target = hit.transform.GetComponent<NPC>();
					if (target != null)
					{
						transform.position = target.transform.position;
						audMan.PlaySingle(audSnip);

						pm.RuleBreak("Bullying", 2f, 0.6f);

						IItemAcceptor acceptor = target.GetComponent<IItemAcceptor>();

						if (acceptor != null && acceptor.ItemFits(item))
							acceptor.InsertItem(pm, pm.ec);

						StartCoroutine(Timer(target));

						return true;
					}
				}
			}
			return base.Use(pm);
		}

		private IEnumerator Timer(NPC tar)
		{
			tar.Navigator.Entity.IgnoreEntity(pm.plm.Entity, true);
			tar.Navigator.Entity.ExternalActivity.moveMods.Add(moveMod);
			float cooldown = stabCooldown;
			while (cooldown > 0f)
			{
				cooldown -= pm.ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}
			tar.Navigator.Entity.IgnoreEntity(pm.plm.Entity, false);
			tar.Navigator.Entity.ExternalActivity.moveMods.Remove(moveMod);
			Destroy(gameObject);
			yield break;
		}
	}
}
