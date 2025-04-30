using System.Collections;
using LotsOfItems.ItemPrefabStructures;
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
	internal float shakeForce = 2.5f; // How strong the tripping/shaking is

	[SerializeField]
	internal float npcPushRadius = 3f, npcPushForce = 12f; // Dust pan effect

	ValueModifier staminaMod;
	float distanceAccumulated;
	Vector3 lastPosition;

	public void SetupPrefab(ItemObject itm)
	{
		setTime = 12.5f;
		audWalk = GenericExtensions.FindResourceObjectByName<SoundObject>("CartoonKnock_Trimmed");
		//GetComponentInChildren<Image>().sprite = this.GetSprite("BalletShoes_canvas.png", 1f); Unused since gauge exists
		gaugeSprite = itm.itemSpriteLarge;
	}
	public void SetupPrefabPost() { }

	public override bool Use(PlayerManager pm)
	{
		pm.plm.Entity.SetResistAddend(true);
		this.pm = pm;
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

			// --- SHAKING/TRIPPING EFFECT WHEN RUNNING ---
			if (pm.plm.running)
			{
				// Apply a random sideways force to the player
				Vector3 right = pm.transform.right;
				float shake = (Random.value - 0.5f) * 2f * shakeForce * Time.deltaTime * pm.PlayerTimeScale;
				pm.plm.Entity.AddForce(new((right * shake).normalized, Mathf.Abs(shake), -Mathf.Abs(shake)));

				// --- NPC PUSH (DUST PAN EFFECT) ---
				for (int i = 0; i < pm.ec.Npcs.Count; i++)
				{
					var npc = pm.ec.Npcs[i];
					if (npc == null || npc.Navigator == null) continue;
					float dist = Vector3.Distance(npc.transform.position, pm.transform.position);
					if (dist <= npcPushRadius)
					{
						Vector3 pushDir = (npc.transform.position - pm.transform.position).normalized;
						npc.Navigator.Entity.AddForce(new((pushDir * npcPushForce).normalized, npcPushForce, -npcPushForce));
					}
				}
			}

			lastPosition = currentPos;

			// --- NOISE ON WALKING/RUNNING ---
			while (distanceAccumulated >= distanceAccumulation)
			{
				pm.ec.MakeNoise(currentPos, noiseVal); // Medium noise value
				Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audWalk);
				distanceAccumulated -= distanceAccumulation;
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
		pm.plm.Entity.SetResistAddend(false);

		//animator.Play("Up", -1, 0f);
		gauge.Deactivate();

		Destroy(gameObject);
	}
}