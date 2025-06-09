using System.Collections;
using System.Collections.Generic;
using LotsOfItems.ItemPrefabStructures;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.PrincipalWhistles
{
	public class ITM_PrincipalOEleporter : Item, IItemPrefab
	{
		public void SetupPrefab(ItemObject itm)
		{
			audMan = gameObject.CreatePropagatedAudioManager(45f, 75f);
			audWhistle = GenericExtensions.FindResourceObjectByName<SoundObject>("PriWhistle");
			audTeleport = GenericExtensions.FindResourceObjectByName<SoundObject>("Teleport");
		}

		public void SetupPrefabPost() { }
		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audWhistle, audTeleport;

		[SerializeField]
		internal float chanceToRandom = 0.25f;

		public override bool Use(PlayerManager pm)
		{
			this.pm = pm;
			transform.position = pm.transform.position;

			List<NPC> candidates = [];
			foreach (NPC npc in pm.ec.Npcs)
			{
				if (npc.Navigator.isActiveAndEnabled && npc.Character != Character.Baldi)
					candidates.Add(npc);
			}

			bool worked = false;

			if (candidates.Count != 0 && Random.value <= chanceToRandom)
			{
				int avg = candidates.Count / 2;
				for (int i = 0; i < avg; i++)
				{
					if (candidates.Count == 0)
						break;

					int idx = Random.Range(0, candidates.Count);
					candidates[idx].Navigator.Entity.Teleport(pm.transform.position);
					candidates.RemoveAt(idx);
					audMan.PlaySingle(audTeleport);
				}
				worked = true;
			}
			else
			{
				foreach (NPC npc in pm.ec.Npcs)
				{
					if (IsAPrincipal(npc.Character)) // method for, uhm, Times
					{
						npc.Navigator.Entity.Teleport(pm.transform.position);
						audMan.PlaySingle(audTeleport);
						worked = true;
					}
				}
			}
			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audWhistle);
			if (worked)
				StartCoroutine(WaitForAudioFinish());
			else
				Destroy(gameObject);

			return true;
		}

		private bool IsAPrincipal(Character c) =>
			c == Character.Principal;

		IEnumerator WaitForAudioFinish()
		{
			while (audMan.AnyAudioIsPlaying)
				yield return null;

			Destroy(gameObject);
		}
	}
}
