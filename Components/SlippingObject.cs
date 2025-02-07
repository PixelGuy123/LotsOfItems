using UnityEngine;

namespace LotsOfItems.Components
{
	public class SlippingObject : MonoBehaviour
	{
		public void SetAnOwner(GameObject owner) =>
			this.owner = owner;
		private void OnTriggerEnter(Collider other)
		{
			if (other.gameObject == owner) return;

			if (other.isTrigger)
			{
				if (SlipObject(other.gameObject, force, -force * antiForceReduceFactor) && audSlip)
					audMan.PlaySingle(audSlip);
			}
		}

		private void OnTriggerExit(Collider other)
		{
			if (forgetOwnerAfterExit && other.gameObject == owner) // To ensure that the owner can slip too after leaving the area
				owner = null;
		}

		public static bool SlipObject(GameObject other, float force, float acceleration)
		{
			var e = other.GetComponent<Entity>();

			if (e)
				return SlipEntity(e, force, acceleration);
			return false;

		}

		public static bool SlipEntity(Entity e, float force, float acceleration)
		{
			if (e.Grounded && !float.IsNaN(e.Velocity.x))
			{
				float maxMagnitude = Mathf.Min(100f, Mathf.Abs(e.Velocity.magnitude));
				e.AddForce(new(e.Velocity.normalized, Mathf.Abs(force + maxMagnitude), -(maxMagnitude + Mathf.Abs(acceleration))));
				return true;
			}
			return false;
		}

		[SerializeField]
		internal float force = 45f;

		[SerializeField]
		[Range(0f, 1f)]
		internal float antiForceReduceFactor = 0.8f;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audSlip;

		[SerializeField]
		internal bool forgetOwnerAfterExit = true;

		GameObject owner;
	}
}
