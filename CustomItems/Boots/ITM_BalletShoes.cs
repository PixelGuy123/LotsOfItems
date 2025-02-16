using MTM101BaldAPI.Components;
using MTM101BaldAPI.PlusExtensions;
using PixelInternalAPI.Extensions;
using System.Collections;
using UnityEngine;
using LotsOfItems.ItemPrefabStructures;
using UnityEngine.UI;

namespace LotsOfItems.CustomItems.Boots;
public class ITM_BalletShoes : ITM_Boots, IItemPrefab
{
	[SerializeField]
	internal float staminaDropMultiplier = 2f, distanceAccumulation = 5f;

	[SerializeField]
	internal int noiseVal = 45;

	[SerializeField]
	internal SoundObject audWalk;

	ValueModifier staminaMod;
	float distanceAccumulated;
	Vector3 lastPosition;

	public void SetupPrefab(ItemObject itm)
	{
		audWalk = GenericExtensions.FindResourceObjectByName<SoundObject>("CartoonKnock_Trimmed");
		GetComponentInChildren<Image>().sprite = this.GetSprite("BalletShoes_canvas.png", 1f);
	}
	public void SetupPrefabPost() { }

	public override bool Use(PlayerManager pm)
	{
		pm.plm.Entity.SetIgnoreAddend(true);
		this.pm = pm;
		canvas.worldCamera = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).canvasCam;

		// Setup stamina drain modifier
		staminaMod = new ValueModifier(staminaDropMultiplier);
		

		lastPosition = pm.transform.position;
		StartCoroutine(EnhancedTimer());
		return true;
	}

	IEnumerator EnhancedTimer()
	{
		var stats = pm.GetMovementStatModifier();
		stats.AddModifier("staminaDrop", staminaMod);

		for (int i = 0; i < pm.ec.Npcs.Count; i++)
		{
			if (!pm.ec.Npcs[i].Navigator.Entity.IsIgnoring(pm.plm.Entity)) // To make sure recently spawned npcs are included too
				pm.ec.Npcs[i].Navigator.Entity.IgnoreEntity(pm.plm.Entity, true);
		}

		float time = setTime;
		while (time > 0f)
		{
			// Track movement distance
			Vector3 currentPos = pm.transform.position;
			distanceAccumulated += Vector3.Distance(lastPosition, currentPos);
			lastPosition = currentPos;

			while (distanceAccumulated >= distanceAccumulation)
			{
				pm.ec.MakeNoise(currentPos, noiseVal); // Medium noise value
				Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audWalk);
				distanceAccumulated -= distanceAccumulation;
			}

			time -= Time.deltaTime * pm.PlayerTimeScale;
			yield return null;
		}

		// Cleanup

		for (int i = 0; i < pm.ec.Npcs.Count; i++)
		{
			if (pm.ec.Npcs[i].Navigator.Entity.IsIgnoring(pm.plm.Entity)) // To make sure recently spawned npcs are included too
				pm.ec.Npcs[i].Navigator.Entity.IgnoreEntity(pm.plm.Entity, false);
		}
		pm.GetMovementStatModifier().RemoveModifier(staminaMod);
		pm.plm.Entity.SetIgnoreAddend(false);

		animator.Play("Up", -1, 0f);
		float destroyTimer = 2f;
		while (destroyTimer > 0f)
		{
			destroyTimer -= Time.deltaTime;
			yield return null;
		}
		Destroy(gameObject);
	}
}