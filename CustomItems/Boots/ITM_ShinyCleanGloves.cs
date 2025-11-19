using System.Collections;
using LotsOfItems.Components;
using LotsOfItems.ItemPrefabStructures;
using UnityEngine;

namespace LotsOfItems.CustomItems.Boots;

public class ITM_ShinyCleanGloves : ITM_Boots, IItemPrefab
{
	public void SetupPrefab(ItemObject itm) =>
		// GetComponentInChildren<Image>().sprite = this.GetSprite("ShinyCleanGloves_canvas.png", 1f);
		gaugeSprite = itm.itemSpriteSmall;


	public void SetupPrefabPost() { }

	public override bool Use(PlayerManager pm)
	{
		this.pm = pm;
		pm.GetAttributes().SetDoorOpeningSilent(true);

		gauge = Singleton<CoreGameManager>.Instance.GetHud(pm.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, setTime);
		StartCoroutine(NewTimer());
		return true;
	}

	private IEnumerator NewTimer()
	{
		float time = setTime;
		while (time > 0f)
		{
			time -= Time.deltaTime * pm.PlayerTimeScale;
			gauge.SetValue(setTime, time);
			yield return null;
		}
		pm.GetAttributes().SetDoorOpeningSilent(false);
		gauge.Deactivate();
		//animator.Play("Up", -1, 0f);
		Destroy(gameObject);
	}
}
