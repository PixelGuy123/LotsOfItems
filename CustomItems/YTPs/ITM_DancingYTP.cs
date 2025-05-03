using HarmonyLib;
using UnityEngine;

namespace LotsOfItems.CustomItems.YTPs
{
	public class ITM_DancingYTP : ITM_YTPs // Does nothing by itself, it's just a generic ytp that bounces via a patch
	{
	}

	public class DancingPickup : MonoBehaviour
	{
		public void AttachToPickup(Pickup pickup, EnvironmentController ec)
		{
			this.ec = ec;
			this.pickup = pickup;
			var vec = Random.insideUnitCircle.normalized;
			dir = new(vec.x, 0f, vec.y);
			room = ec.CellFromPosition(pickup.transform.position).room.type == RoomType.Null ?
				GetComponentInParent<RoomController>() : ec.CellFromPosition(pickup.transform.position).room; // Usually pickups are children
		}
#pragma warning disable IDE0051 // Remover membros privados não utilizados
		void Update()
#pragma warning restore IDE0051 // Remover membros privados não utilizados
		{
			if (!pickup || pickup.item == Singleton<CoreGameManager>.Instance.NoneItem)
				return;
			Vector3 prevPos = pickup.transform.position;
			pickup.transform.position += dir * speed * Time.deltaTime * ec.EnvironmentTimeScale;

			if (!SuitablePosition(pickup.transform.position))
				pickup.transform.position = prevPos;
			pickup.icon.UpdatePosition(ec.map);

			ray.origin = pickup.transform.position;
			ray.direction = dir;
			if (Physics.Raycast(ray, out hit, maxDistance, layer) || !SuitablePosition(ray.origin))
				dir = Vector3.Reflect(dir, hit.normal);

		}

		bool SuitablePosition(Vector3 position) => ec.ContainsCoordinates(position) && ec.CellFromPosition(position).TileMatches(room);

		RaycastHit hit;
		Ray ray = new();
		Vector3 dir;
		Pickup pickup;
		EnvironmentController ec;
		RoomController room;

		[SerializeField]
		internal float maxDistance = 2f, speed = 20f;

		[SerializeField]
		internal LayerMask layer = LayerMask.GetMask("Default", "Windows");
	}
}
