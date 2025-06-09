using System.Collections.Generic;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using UnityEngine;

namespace LotsOfItems.CustomItems.YTPs
{
    public class ITM_SomethingYTP : ITM_YTPs, IItemPrefab
    {
        public void SetupPrefab(ItemObject itm) { }

        public void SetupPrefabPost() =>
            potentialItems = ItemObjectExtensions.GetAllShoppingItems();

        public override bool Use(PlayerManager pm)
        {
            value = preSetValues[Random.Range(0, preSetValues.Length)]; // The patch in PickupPatches should do the *giving item function*
            return base.Use(pm);
        }

        [SerializeField]
        internal int[] preSetValues = [25, 50, 100];

        public static float pickupChance = 0.25f;

        static List<ItemObject> potentialItems;
        public static ItemObject GetItem => potentialItems[Random.Range(0, potentialItems.Count)];
    }

    public class SomethingYTP_InflationMarker : MonoBehaviour { }
}
