using System.Collections.Generic;
using LotsOfItems.ItemPrefabStructures;
using UnityEngine;

namespace LotsOfItems.CustomItems.GrapplingHooks;
public class ITM_BouncyGrapplingHook : ITM_GenericGrapplingHook
{
	private readonly List<Vector3> bouncePositions = [Vector3.zero, Vector3.zero];

	[SerializeField]
	private int maxBounces = 3;

	private int currentBounces = 0;
	private Vector3 currentDirection = Vector3.zero;
	float ogStopDistance;

	public override bool Use(PlayerManager pm)
	{
		ogStopDistance = stopDistance;
		return base.Use(pm);
	}

	public override bool VirtualPreLateUpdate()
	{
		if (snapped || bouncePositions.Count == 0)
			return false;

		positions[0] = transform.position;
		positions[positions.Length - 1] = pm.transform.position - Vector3.up * 1f;

		bouncePositions[0] = positions[0];
		bouncePositions[bouncePositions.Count - 1] = positions[positions.Length - 1];

		lineRenderer.SetPositions(positions);
		return false;
	}

	public override bool OnWallHitOverride(RaycastHit hit)
	{
		if (currentBounces >= maxBounces)
		{
			ForceStop(-hit.normal, motorAudio: true); // Final anchor

			initialDistance = 0f;
			for (int i = 0; i < positions.Length - 1; i++)
				initialDistance += (positions[positions.Length - 1] - positions[i]).magnitude;
			initialDistance /= positions.Length - 1; // Get the real distance between all positions

			return false; // Prevent base behavior
		}

		// Reflect direction and update path
		currentDirection = Vector3.Reflect(currentDirection, hit.normal);
		transform.forward = currentDirection;

		// Record bounce position
		bouncePositions.Insert(1, hit.point); // For some reason, it needs to follow this logic

		lineRenderer.positionCount = bouncePositions.Count;
		positions = [.. bouncePositions];
		lineRenderer.SetPositions(positions);

		currentBounces++;
		return false; // Block original lock-on
	}

	protected override void VirtualSetupPrefab(ItemObject itm)
	{
		base.VirtualSetupPrefab(itm);

		var hookRenderer = GetComponentInChildren<SpriteRenderer>();
		hookRenderer.sprite = this.GetSprite("BouncyGrapplingHook_world.png", hookRenderer.sprite.pixelsPerUnit);
		uses = 0;
		maxPressure = 200f;
		initialForce = 45f;
		forceIncrease = 10f;
	}

	public override bool VirtualPreUpdate()
	{
		stopDistance = currentBounces > 0 ? -1f : ogStopDistance;
		return true;
	}

	public override void VirtualUpdate()
	{
		if (!locked)
			currentDirection = transform.forward; // Track trajectory for reflection
		else if (!snapped)
			moveMod.movementAddend = CalculatePathDirection();
	}

	private Vector3 CalculatePathDirection()
	{
		if (bouncePositions.Count == 0)
			return (transform.position - pm.transform.position).normalized * force;

		Vector3 target = bouncePositions[Mathf.Clamp(currentBounces, 0, bouncePositions.Count - 1)];
		var offset = target - pm.transform.position;
		if (ogStopDistance > offset.magnitude)
		{
			if (--currentBounces > 1)
			{
				bouncePositions.RemoveAt(currentBounces + 1);
				positions = [.. bouncePositions];
				lineRenderer.positionCount = positions.Length;
				lineRenderer.SetPositions(positions);
			}
		}

		return offset.normalized * force;
	}

	public override void VirtualEnd()
	{
		bouncePositions.Clear();
	}
}