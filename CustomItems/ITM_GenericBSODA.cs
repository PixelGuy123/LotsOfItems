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

	public override bool Use(PlayerManager pm)
	{
		Quaternion myOgRot = transform.rotation;
		bool val = base.Use(pm);
		transform.rotation = myOgRot; // To avoid changing directions if not needed

		return val;
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
		moveMod.movementAddend = entity.ExternalActivity.Addend + transform.forward * speed * ec.EnvironmentTimeScale;
		entity.UpdateInternalMovement(transform.forward * speed * ec.EnvironmentTimeScale);
		time -= Time.deltaTime * ec.EnvironmentTimeScale;

		if (time <= 0f)
		{
			VirtualEnd();
		}
	}

	public virtual bool VirtualEntityTriggerEnter(Collider other) =>
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
	static bool EntityTriggerEnterOverride(ITM_BSODA __instance, Collider other)
	{
		if (__instance is ITM_GenericBSODA generic)
		{
			return generic.VirtualEntityTriggerEnter(other);
		}
		return true;
	}
}