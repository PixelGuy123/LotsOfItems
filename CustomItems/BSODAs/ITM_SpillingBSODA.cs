using System.Collections;
using LotsOfItems.Plugin;
using MTM101BaldAPI;
using MTM101BaldAPI.Registers;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.BSODAs;

public class ITM_SpillingBSODA : ITM_GenericNanaPeel
{
	[SerializeField]
	private float duration = 15f, spawnSpeed = 3.5f;

	[SerializeField]
	private SpillingBSODA_PuddleTimer puddlePrefab;
	protected override void VirtualSetupPrefab(ItemObject itm)
	{
		var bsodaItm = ItemMetaStorage.Instance.FindByEnum(Items.Bsoda).value;
		// Get the original BSODA sprite for the puddle visual
		var puddleSpriteVisual = ((ITM_BSODA)bsodaItm.item).spriteRenderer.sprite;

		// Create the puddle prefab
		var puddleObject = ObjectCreationExtensions.CreateSpriteBillboard(
				puddleSpriteVisual,
				false
			).AddSpriteHolder(out var renderer, 0.1f, LayerStorage.ignoreRaycast);
		puddleObject.name = "SpillingBSODAPuddle";

		renderer.gameObject.layer = 0;
		renderer.transform.Rotate(90, 0, 0); // Face downward
		puddleObject.gameObject.ConvertToPrefab(true);

		// Add collider to the puddle prefab
		var puddleCollider = puddleObject.gameObject.AddComponent<BoxCollider>();
		puddleCollider.isTrigger = false; // Make it solid
		puddleCollider.size = new Vector3(5f, 5f, 5f);
		puddleCollider.center = Vector3.up * 5f; // Adjust center if needed based on visual

		// Add the PuddleTimer component to the prefab
		var timerComponent = puddleObject.gameObject.AddComponent<SpillingBSODA_PuddleTimer>();
		timerComponent.duration = duration;
		timerComponent.spawnSpeed = spawnSpeed;
		timerComponent.audSplash = GenericExtensions.FindResourceObjectByName<SoundObject>("BsodaSpray"); // Assign sound
		timerComponent.audMan = timerComponent.gameObject.CreatePropagatedAudioManager(35f, 50f);

		puddlePrefab = timerComponent;



		GetComponentInChildren<SpriteRenderer>().sprite = bsodaItm.itemSpriteLarge; // Use the item's large icon for the thrown visual

		throwSpeed *= 1.85f;
		endHeight = 1.25f;
		gravity *= 0.65f;
		height = 2.5f;
	}

	internal override void OnFloorHit() // When hitting floor
	{
		// Instantiate the puddle prefab at the hit location
		Instantiate(puddlePrefab, transform.position, Quaternion.identity)
			.Initialize(ec);


		Destroy(gameObject);
	}
}

public class SpillingBSODA_PuddleTimer : MonoBehaviour
{
	[SerializeField]
	internal float duration, spawnSpeed;
	private EnvironmentController ec;
	private Cell spawnCell;
	[SerializeField]
	internal AudioManager audMan;
	[SerializeField]
	internal BoxCollider puddleCollider;
	[SerializeField]
	internal SoundObject audSplash;

	internal void Initialize(EnvironmentController environmentController)
	{
		ec = environmentController;
		puddleCollider = GetComponent<BoxCollider>(); // Get the collider attached to the puddle prefab

		if (ec == null || !ec.ContainsCoordinates(transform.position))
		{
			Destroy(gameObject);
			return;
		}

		spawnCell = ec.CellFromPosition(transform.position);
		if (!spawnCell.Null)
		{
			transform.position = spawnCell.FloorWorldPosition; // Snap to floor center
			audMan.PlaySingle(audSplash);
			StartCoroutine(Timer());
			return;
		}
		Destroy(gameObject);
	}

	IEnumerator Timer()
	{
		if (spawnCell.Null || puddleCollider == null || ec == null)
		{
			Destroy(gameObject);
			yield break;
		}

		spawnCell.BlockAll(ec, true); // Block the cell

		float scale = 0f;
		while (scale < 1f)
		{
			scale += ec.EnvironmentTimeScale * Time.deltaTime * spawnSpeed;
			transform.localScale = Mathf.Min(scale, 1f) * Vector3.one;
			yield return null;
		}
		transform.localScale = Vector3.one;

		float timer = duration;
		while (timer > 0f)
		{
			timer -= Time.deltaTime; // Use standard Time.deltaTime for puddle duration
			yield return null;
		}

		spawnCell.BlockAll(ec, false); // Unblock the cell
		puddleCollider.enabled = false; // Disable collider before shrinking

		scale = 1f;
		while (scale > 0f)
		{
			scale -= ec.EnvironmentTimeScale * Time.deltaTime * spawnSpeed;
			transform.localScale = Mathf.Max(scale, 0f) * Vector3.one;
			yield return null;
		}

		Destroy(gameObject);
	}
}

