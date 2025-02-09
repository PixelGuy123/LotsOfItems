using System.Collections;
using UnityEngine;
using LotsOfItems.Components;
using UnityEngine.UI;
using LotsOfItems.ItemPrefabStructures;

namespace LotsOfItems.CustomItems.Boots;
public class ITM_ShinyCleanGloves : ITM_Boots, IItemPrefab
{
	public void SetupPrefab(ItemObject itm) =>
		GetComponentInChildren<Image>().sprite = this.GetSprite("ShinyCleanGloves_canvas.png", 1f);
	

	public void SetupPrefabPost() { }

	public override bool Use(PlayerManager pm)
	{
		this.pm = pm;
		pm.GetAttributes().SetDoorOpeningSilent(true);
		canvas.worldCamera = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).canvasCam;
		StartCoroutine(NewTimer());
		return true;
	}

	private IEnumerator NewTimer()
	{
		float time = setTime;
		while (time > 0f)
		{
			time -= Time.deltaTime * pm.PlayerTimeScale;
			yield return null;
		}
		pm.GetAttributes().SetDoorOpeningSilent(false);
		animator.Play("Up", -1, 0f);
		time = 2f;
		while (time > 0f)
		{
			time -= Time.deltaTime;
			yield return null;
		}
		Destroy(gameObject);
	}
}
