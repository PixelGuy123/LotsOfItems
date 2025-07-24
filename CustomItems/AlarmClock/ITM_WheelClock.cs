using LotsOfItems.ItemPrefabStructures;
using UnityEngine;

namespace LotsOfItems.CustomItems.AlarmClock;

public class ITM_WheelClock : ITM_GenericAlarmClock
{
	[SerializeField]
	private float speed = 24f; // movement speed

	private Vector3 direction; // current movement direction
	bool hasRinged = false;

	protected override void VirtualSetupPrefab(ItemObject itm)
	{
		base.VirtualSetupPrefab(itm);
		spriteRenderer.sprite = this.GetSprite("WheelClock_World.png", spriteRenderer.sprite.pixelsPerUnit);
		clockSprite = [spriteRenderer.sprite];
		spriteRenderer.transform.localPosition = Vector3.down * 0.1f;
	}

	public override bool Use(PlayerManager pm)
	{
		direction = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward.normalized;
		entity.OnEntityMoveInitialCollision += (hit) =>
		{
			time = 0f; // Instant start from there
		};

		return base.Use(pm);
	}

	public override bool AllowClickable() => false; // disable click adjustments

	void Update() =>
		entity.UpdateInternalMovement(hasRinged ? Vector3.zero : direction * speed);

	protected override void OnClockRing()
	{
		base.OnClockRing();
		hasRinged = true;
	}

}
