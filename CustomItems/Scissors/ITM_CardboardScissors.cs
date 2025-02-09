using UnityEngine;
using System.Collections;

namespace LotsOfItems.CustomItems.Scissors;

public class ITM_CardboardScissors : ITM_Scissors
{
	[SerializeField]
	internal ItemObject nextItem = null;

	[SerializeField]
	internal float useChance = 0.25f;

	public override bool Use(PlayerManager pm)
	{
		this.pm = pm;
		bool flag = false;
		// Determine if this use will actually work.
		bool performAction = Random.value <= useChance;

		// Copy of ITM_Scissors functionality:
		if (pm.jumpropes.Count > 0)
		{
			if (performAction)
			{
				while (pm.jumpropes.Count > 0)
					pm.jumpropes[0].End(false);
			}
			flag = true;
		}
		if (Gum.playerGum.Count > 0)
		{
			if (performAction)
			{
				foreach (Gum gum in Gum.playerGum)
					gum.Cut();
			}

			flag = true;
		}
		if (Physics.Raycast(pm.transform.position,
							Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward,
							out hit, pm.pc.reach, clickMask))
		{
			IItemAcceptor component = hit.transform.GetComponent<IItemAcceptor>();
			if (component != null && component.ItemFits(Items.Scissors))
			{
				if (performAction)
					component.InsertItem(pm, pm.ec);
				flag = true;
			}
		}
		else
		{
			Collider[] array = new Collider[16];
			int num = Physics.OverlapSphereNonAlloc(pm.transform.position, 4f, array, 131072, QueryTriggerInteraction.Collide);
			for (int i = 0; i < num; i++)
			{
				if (array[i].isTrigger && array[i].CompareTag("NPC"))
				{
					FirstPrize component2 = array[i].GetComponent<FirstPrize>();
					if (component2 != null)
					{
						if (performAction)
						{
							component2.CutWires();
						}
						flag = true;
					}
				}
			}
		}		

		if (flag)
		{
			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audSnip);
			if (nextItem)
				StartCoroutine(Delay());
		}
			

		return flag;
	}

	IEnumerator Delay()
	{
		yield return null;

		pm.itm.SetItem(nextItem, pm.itm.selectedItem);
		Destroy(gameObject);
	}
}
