using PixelInternalAPI.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LotsOfItems.Components
{
	public class GrowItemAcceptor : MonoBehaviour, IItemAcceptor
	{
		EnvironmentController ec;
		Pickup pickup;
		public bool IsGrowingItem { get; private set; } = false;

		void Start()
		{
			ec = Singleton<BaseGameManager>.Instance.Ec;
			pickup = ec.CreateItem(ec.CellFromPosition(transform.position).room, Singleton<CoreGameManager>.Instance.NoneItem, new(transform.position.x, transform.position.z));
			pickup.gameObject.SetActive(false);
		}

		public static void RegisterExchangingItem(Items itemToGrow, ItemObject toExchangeWith, float timeToGrow) =>
			itemsToGrow.Add(itemToGrow, new(toExchangeWith, timeToGrow));

		public void InsertItem(PlayerManager player, EnvironmentController ec)
		{
			if (IsGrowingItem)
				return;
			IsGrowingItem = true;
			StartCoroutine(GrowItem(player, player.itm.items[player.itm.selectedItem]));
			player.itm.RemoveItem(player.itm.selectedItem);
		}

		IEnumerator GrowItem(PlayerManager pm, ItemObject item)
		{
			var itemKey = itemsToGrow[item.itemType];

			renderer.sprite = item.itemSpriteLarge;
			renderer.transform.localPosition = new(Random.Range(minMaxX.x, minMaxX.y), Random.Range(minMaxY.x, minMaxY.y), -0.5f);
			renderer.enabled = true;

			Vector3 pos = transform.position + (pm.transform.position - transform.position).normalized * 1.5f;
			pos.y = Entity.physicalHeight;
			pickup.transform.position = pos;

			pickup.AssignItem(itemKey.Key); // Exchanging item

			float timer = itemKey.Value;
			while (timer > 0f)
			{
				timer -= ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}

			renderer.enabled = false;
			pickup.gameObject.SetActive(true);

			IsGrowingItem = false;

			if (audEnd)
				audMan.PlaySingle(audEnd);

		}

		public bool ItemFits(Items item) =>
			!IsGrowingItem && !pickup.isActiveAndEnabled && itemsToGrow.ContainsKey(item);


		internal static Dictionary<Items, KeyValuePair<ItemObject, float>> itemsToGrow = [];

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal Vector2 minMaxY = new(2.75f, 5f), minMaxX = new(-0.45f, 0.45f);

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audEnd;
	}
}
