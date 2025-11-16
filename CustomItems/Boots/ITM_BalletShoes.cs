using System.Collections;
using LotsOfItems.Components;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.PlusExtensions;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.Boots;

public class ITM_BalletShoes : ITM_Boots, IItemPrefab
{
	[SerializeField]
	internal float staminaDropMultiplier = 2f, distanceAccumulation = 5f;

	[SerializeField]
	internal int noiseVal = 45;

	[SerializeField]
	internal SoundObject audWalk;

	[SerializeField]
	internal float shakeForce = 22f; // How strong the tripping/shaking is

	[SerializeField]
	internal float npcPushRadius = 35f, npcPushForce = 25f; // Dust pan effect

	[SerializeField]
	internal LayerMask collisionLayer = LotOfItemsPlugin.onlyNpcLayers;

	ValueModifier staminaMod;
	float distanceAccumulated;
	Vector3 lastPosition;

	public void SetupPrefab(ItemObject itm)
	{
		setTime = 12.5f;
		audWalk = GenericExtensions.FindResourceObjectByName<SoundObject>("CartoonKnock_Trimmed");
		//GetComponentInChildren<Image>().sprite = this.GetSprite("BalletShoes_canvas.png", 1f); Unused since gauge exists
		gaugeSprite = itm.itemSpriteSmall;
	}
	public void SetupPrefabPost() { }

	public override bool Use(PlayerManager pm)
	{
		this.pm = pm;
		pm.GetAttributes().SetAddendOnlyImmunity(true);
		gauge = Singleton<CoreGameManager>.Instance.GetHud(pm.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, setTime);

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

		yield return null;

		float time = setTime;
		while (time > 0f)
		{
			for (int i = 0; i < pm.ec.Npcs.Count; i++) // Inside the while loop to make sure recently spawned npcs are included as well
			{
				if (!pm.ec.Npcs[i].Navigator.Entity.IsIgnoring(pm.plm.Entity))
					pm.ec.Npcs[i].Navigator.Entity.IgnoreEntity(pm.plm.Entity, true);
			}

			// Track movement distance
			Vector3 currentPos = pm.transform.position;
			distanceAccumulated += Vector3.Distance(lastPosition, currentPos);

			lastPosition = currentPos;

			// --- NOISE ON WALKING/RUNNING ---
			if (distanceAccumulated >= distanceAccumulation)
			{
				pm.ec.MakeNoise(currentPos, noiseVal); // Medium noise value
				Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audWalk);
				distanceAccumulated = 0f;

				// --- SHAKING/TRIPPING EFFECT WHEN RUNNING ---
				if (pm.plm.running && pm.plm.stamina > 0f & pm.plm.Entity.Grounded)
				{
					// Apply a random sideways force to the player
					float shakeForce = Random.value * this.shakeForce;
					Vector2 unit = Random.insideUnitCircle;
					pm.plm.Entity.AddForce(new(new(unit.x, 0f, unit.y), shakeForce, -shakeForce));

					// --- NPC PUSH (DUST PAN EFFECT) ---
					transform.position = pm.transform.position;
					ItemExtensions.Explode(
							this,
							npcPushRadius,
							collisionLayer,
							npcPushForce,
							-npcPushForce
						);
				}
			}

			time -= Time.deltaTime * pm.PlayerTimeScale;
			gauge.SetValue(setTime, time);
			yield return null;
		}

		// Cleanup

		for (int i = 0; i < pm.ec.Npcs.Count; i++) // Revert npc ignoring thing
		{
			if (pm.ec.Npcs[i].Navigator.Entity.IsIgnoring(pm.plm.Entity))
				pm.ec.Npcs[i].Navigator.Entity.IgnoreEntity(pm.plm.Entity, false);
		}
		pm.GetMovementStatModifier().RemoveModifier(staminaMod);
		pm.GetAttributes().SetAddendOnlyImmunity(false);

		//animator.Play("Up", -1, 0f);
		gauge.Deactivate();

		Destroy(gameObject);
	}
}