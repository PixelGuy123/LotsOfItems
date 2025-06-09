using System.Collections;
using LotsOfItems.ItemPrefabStructures;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;
using UnityEngine.UI;

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
		//GetComponentInChildren<Image>().sprite = this.GetSprite("ReallyTallBoots_canvas.png", 1f); Gauge exists

		audFellFloor = GenericExtensions.FindResourceObjectByName<SoundObject>("Bang");
		audStomp = GenericExtensions.FindResourceObjectByName<SoundObject>("CartoonKnock_Trimmed");

		gaugeSprite = itm.itemSpriteSmall;
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
		pm.plm.Entity.SetResistAddend(true);
		gauge = Singleton<CoreGameManager>.Instance.GetHud(pm.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, setTime);
		this.pm = pm;
		StartCoroutine(NewTimer());

		return true;
	}

	private IEnumerator NewTimer()
	{
		float time = setTime;
		yield return null;
		while (time > 0f)
		{
			for (int i = 0; i < pm.ec.Npcs.Count; i++)
			{
				if (!pm.ec.Npcs[i].Navigator.Entity.IsIgnoring(pm.plm.Entity)) // To make sure recently spawned npcs are included too
					pm.ec.Npcs[i].Navigator.Entity.IgnoreEntity(pm.plm.Entity, true);
			}
			transform.position = pm.transform.position;
			time -= Time.deltaTime * pm.PlayerTimeScale;

			gauge.SetValue(setTime, time);

			yield return null;
		}

		pm.plm.Entity.SetResistAddend(false);
		for (int i = 0; i < pm.ec.Npcs.Count; i++)
		{
			if (pm.ec.Npcs[i].Navigator.Entity.IsIgnoring(pm.plm.Entity))
				pm.ec.Npcs[i].Navigator.Entity.IgnoreEntity(pm.plm.Entity, false);
		}

		canStomp = false;
		//animator.Play("Up", -1, 0f); (0.10.x) doesn't use screen effects anymore


		Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audFellFloor);
		overrider.SetHeight(1.5f);
		pm.Am.moveMods.Add(moveMod);

		time = timeStuckOnFloor;
		while (time > 0f)
		{
			time -= Time.deltaTime;
			gauge.SetValue(timeStuckOnFloor, time);
			yield return null;
		}
		gauge.Deactivate();
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
