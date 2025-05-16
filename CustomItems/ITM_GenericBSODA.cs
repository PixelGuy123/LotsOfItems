using HarmonyLib;
using LotsOfItems.ItemPrefabStructures;
using UnityEngine;

namespace LotsOfItems.CustomItems;

public class ITM_GenericBSODA : ITM_BSODA, IItemPrefab
{
	protected bool hasEnded = false;
	public void SetupPrefab(ItemObject itm) => VirtualSetupPrefab(itm);
	public void SetupPrefabPost() { }
	protected virtual void VirtualSetupPrefab(ItemObject itm) { }

	[Range(0f, 1f)]
	[SerializeField]
	protected float AddendMultiplier = 1f;

	[SerializeField]
	protected bool breaksRuleWhenUsed = true;

	Quaternion rotation;
	internal void SetOriginalRotation(Quaternion rot)
	{
		rotation = rot;
		hasOriginalRotationSet = true;
	}
	bool hasOriginalRotationSet = false;

	public override bool Use(PlayerManager pm)
	{
		this.pm = pm;
		ec = pm.ec;
		transform.position = pm.transform.position;
		transform.forward = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward;
		entity.Initialize(ec, transform.position);
		spriteRenderer.SetSpriteRotation(Random.Range(0f, 360f));
		Singleton<CoreGameManager>.Instance.audMan.PlaySingle(sound);
		if (breaksRuleWhenUsed)
			pm.RuleBreak("Drinking", 0.8f, 0.25f);
		moveMod.priority = 1;
		if (hasOriginalRotationSet)
			transform.rotation = rotation;
		return true;
	}

	public virtual void VirtualUpdate()
	{
		if (hasEnded)
		{
			entity.UpdateInternalMovement(Vector3.zero);
			moveMod.movementAddend = Vector3.zero;
			return;
		}

		// Replicate base Update logic with extensibility points
		moveMod.movementAddend = entity.ExternalActivity.Addend + transform.forward * AddendMultiplier * speed * ec.EnvironmentTimeScale;
		entity.UpdateInternalMovement(transform.forward * speed * ec.EnvironmentTimeScale);
		time -= Time.deltaTime * ec.EnvironmentTimeScale;

		if (time <= 0f)
		{
			VirtualEnd();
		}
	}

	public virtual bool VirtualEntityTriggerEnter(Collider other) =>
		true;
	public virtual bool VirtualEntityTriggerExit(Collider other) =>
		true;


	protected virtual void VirtualEnd()
	{
		// Cleanup logic
		hasEnded = true;
		foreach (ActivityModifier activityMod in activityMods)
			activityMod.moveMods.Remove(moveMod);

		Destroy(gameObject);
	}
}

[HarmonyPatch(typeof(ITM_BSODA))]
static class GenericBSODAPatches
{
	[HarmonyPatch("Update")]
	[HarmonyPrefix]
	static bool UpdateOverride(ITM_BSODA __instance)
	{
		if (__instance is ITM_GenericBSODA generic)
		{
			generic.VirtualUpdate();
			return false; // Skip original Update
		}
		return true;
	}

	[HarmonyPatch("EntityTriggerEnter")]
	[HarmonyPrefix]
	static bool EntityTriggerEnterOverride(ITM_BSODA __instance, Collider other, bool ___launching)
	{
		if ((!___launching || !other.CompareTag("Player")) && __instance is ITM_GenericBSODA generic)
		{
			return generic.VirtualEntityTriggerEnter(other);
		}
		return true;
	}

	[HarmonyPatch("EntityTriggerExit")]
	[HarmonyPrefix]
	static bool EntityTriggerExitOverride(ITM_BSODA __instance, Collider other, ref bool ___launching)
	{
		if (__instance is ITM_GenericBSODA generic)
		{
			bool flag = generic.VirtualEntityTriggerExit(other);
			if (!flag && other.CompareTag("Player"))
				___launching = false;

			return flag;
		}
		return true;
	}
}