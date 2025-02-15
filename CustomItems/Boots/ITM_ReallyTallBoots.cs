using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using LotsOfItems.ItemPrefabStructures;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;

namespace LotsOfItems.CustomItems.Boots;
public class ITM_ReallyTallBoots : ITM_Boots, IItemPrefab
{
	readonly EntityOverrider overrider = new();
	readonly MovementModifier moveMod = new(Vector3.zero, 0f);

	[SerializeField]
	internal SoundObject audStomp, audFellFloor;

	[SerializeField]
	internal float squishTime = 10f, timeStuckOnFloor = 5.5f;

	bool canStomp = true;

	public void SetupPrefab(ItemObject itm)
	{
		var collider = gameObject.AddComponent<CapsuleCollider>();
		collider.isTrigger = true;
		collider.radius = 2.5f;
		collider.height = 10f;

		gameObject.layer = LayerStorage.ignoreRaycast;
		GetComponentInChildren<Image>().sprite = this.GetSprite("ReallyTallBoots_canvas.png", 1f);

		audFellFloor = GenericExtensions.FindResourceObjectByName<SoundObject>("Bang");
		audStomp = GenericExtensions.FindResourceObjectByName<SoundObject>("CartoonKnock_Trimmed");
	}
	public void SetupPrefabPost() { }

	public override bool Use(PlayerManager pm)
	{
		if (!pm.plm.Entity.Override(overrider))
		{
			Destroy(gameObject);
			return false;
		}

		overrider.SetHeight(9.5f);
		pm.plm.Entity.SetIgnoreAddend(true);
		this.pm = pm;
		canvas.worldCamera = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).canvasCam;
		StartCoroutine(NewTimer());

		return true;
	}

	private IEnumerator NewTimer()
	{
		float time = setTime;
		while (time > 0f)
		{
			for (int i = 0; i < pm.ec.Npcs.Count; i++)
			{
				if (!pm.ec.Npcs[i].Navigator.Entity.IsIgnoring(pm.plm.Entity)) // To make sure recently spawned npcs are included too
					pm.ec.Npcs[i].Navigator.Entity.IgnoreEntity(pm.plm.Entity, true);
			}
			transform.position = pm.transform.position;
			time -= Time.deltaTime * pm.PlayerTimeScale;
			yield return null;
		}

		pm.plm.Entity.SetIgnoreAddend(false);
		foreach (var npc in pm.ec.Npcs)
			npc.Navigator.Entity.IgnoreEntity(pm.plm.Entity, false);

		canStomp = false;
		animator.Play("Up", -1, 0f);
		Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audFellFloor);
		overrider.SetHeight(1.5f);
		pm.Am.moveMods.Add(moveMod);

		time = timeStuckOnFloor;
		while (time > 0f)
		{
			time -= Time.deltaTime;
			yield return null;
		}
		Destroy(gameObject);
		overrider.Release();
		pm.Am.moveMods.Remove(moveMod);
	}

	void OnTriggerEnter(Collider other)
	{
		if (!canStomp || other.gameObject == pm.gameObject)
			return;

		if (other.isTrigger && other.CompareTag("NPC"))
		{
			var e = other.GetComponent<Entity>();
			if (e)
			{
				e.Squish(squishTime);
				Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audStomp);
			}
		}
	}
}
