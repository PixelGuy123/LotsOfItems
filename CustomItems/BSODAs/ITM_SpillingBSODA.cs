using UnityEngine;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Components;
using MTM101BaldAPI;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using System.Collections;
using LotsOfItems.Plugin;
using MTM101BaldAPI.Registers;

namespace LotsOfItems.CustomItems.BSODAs;

public class ITM_SpillingBSODA : Item, IItemPrefab
{
	[SerializeField]
	private float duration = 15f, spawnSpeed = 3.5f;

	[SerializeField]
	internal BoxCollider collider;

	[SerializeField]
	internal SoundObject audUse;

	Cell spawnCell;

	public void SetupPrefab(ItemObject itm)
	{
		var puddleSprite = ObjectCreationExtensions.CreateSpriteBillboard(
				((ITM_BSODA)ItemMetaStorage.Instance.FindByEnum(Items.Bsoda).value.item).spriteRenderer.sprite,
				false
			).gameObject;

		puddleSprite.transform.SetParent(transform);
		puddleSprite.transform.localPosition = Vector3.up * 0.05f;
		puddleSprite.name = "Sprite";
		puddleSprite.layer = 0;
		puddleSprite.transform.Rotate(90, 0, 0); // Face downward

		collider = gameObject.AddComponent<BoxCollider>();
		collider.isTrigger = false;
		collider.size = new Vector3(5f, 5f, 5f);
		collider.center = Vector3.up * 5f;

		audUse = GenericExtensions.FindResourceObjectByName<SoundObject>("BsodaSpray");
	}

	public void SetupPrefabPost() { }

	public override bool Use(PlayerManager pm)
	{
		this.pm = pm;
		if (!pm.ec.ContainsCoordinates(pm.transform.position))
		{
			Destroy(gameObject);
			return false;
		}

		spawnCell = pm.ec.CellFromPosition(pm.transform.position);
		if (!spawnCell.Null)
		{
			Physics.IgnoreCollision(collider, pm.plm.Entity.collider, true);
			transform.position = spawnCell.FloorWorldPosition;
			StartCoroutine(WaitForPlayerToLeaveArea());
			StartCoroutine(Timer());

			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audUse);
			return true;
		}
		Destroy(gameObject);
		return false;
	}

	IEnumerator WaitForPlayerToLeaveArea()
	{
		while (pm.ec.CellFromPosition(pm.transform.position) == spawnCell)
			yield return null;
		Physics.IgnoreCollision(collider, pm.plm.Entity.collider, false);
	}

	IEnumerator Timer()
	{
		spawnCell.BlockAll(pm.ec, true);

		float scale = 0f;
		while (true)
		{
			scale += pm.ec.EnvironmentTimeScale * Time.deltaTime * spawnSpeed;
			if (scale >= 1f)
				break;
			transform.localScale = scale * Vector3.one;
			yield return null;
		}

		transform.localScale = Vector3.one;

		float timer = duration;
		while (timer > 0f)
		{
			timer -= Time.deltaTime;
			yield return null;
		}

		spawnCell.BlockAll(pm.ec, false);
		collider.enabled = false;

		scale = 1f;
		while (true)
		{
			scale -= pm.ec.EnvironmentTimeScale * Time.deltaTime * spawnSpeed;
			if (scale <= 0f)
				break;
			transform.localScale = scale * Vector3.one;
			yield return null;
		}

		Destroy(gameObject);
	}
}

			   