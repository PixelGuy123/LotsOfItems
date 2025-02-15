using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MomentumNavigator : MonoBehaviour // Used deepseek to generate a version of Navigator that doesn't necessarily run on NPC
{
	public EnvironmentController ec;

	[SerializeField] 
	public float radius = 10f;
	[SerializeField] 
	public float maxSpeed = 15f;
	[SerializeField] 
	public float acceleration = 15f;
	[SerializeField] 
	public bool autoRotate = false;

	protected NavMeshPath _navMeshPath;
	protected List<Vector3> destinationPoints = [];
	protected float currentSpeed = 0f;
	protected Vector3 velocity;
	bool initialized = false;

	public bool HasDestination => destinationPoints.Count != 0;
	public Vector3 CurrentDestination => HasDestination ? destinationPoints[destinationPoints.Count - 1] : Vector3.zero;

	private void Awake() =>
		_navMeshPath = new NavMeshPath();

	public void Initialize(EnvironmentController ec, bool useAcceleration)
	{
		this.ec = ec;
		initialized = true;
		if (useAcceleration)
			currentSpeed = 0f;
		else
			currentSpeed = maxSpeed;
	}
	

	private void Update()
	{
		if (!initialized || Time.timeScale <= 0f || Time.deltaTime <= 0f) return;

		UpdateMovement();
		UpdateRotation();
	}

	private void UpdateMovement()
	{
		if (!HasDestination)
		{
			currentSpeed = 0f;
			return;
		}

		currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime * ec.EnvironmentTimeScale, maxSpeed);
		float moveDistance = currentSpeed * Time.deltaTime * ec.EnvironmentTimeScale;

		while (destinationPoints.Count > 0 &&
			   Vector3.Distance(transform.position, destinationPoints[0]) <= moveDistance)
		{
			moveDistance -= Vector3.Distance(transform.position, destinationPoints[0]);
			transform.position = destinationPoints[0];
			destinationPoints.RemoveAt(0);
		}

		if (HasDestination)
		{
			Vector3 direction = (destinationPoints[0] - transform.position).normalized;
			transform.position += direction * moveDistance;
		}
	}

	private void UpdateRotation()
	{
		if (autoRotate && HasDestination)
		{
			Vector3 lookDirection = (destinationPoints[0] - transform.position).normalized;
			if (lookDirection != Vector3.zero)
			{
				transform.rotation = Quaternion.LookRotation(lookDirection);
			}
		}
	}

	public void FindPath(Vector3 targetPosition)
	{
		destinationPoints.Clear();

		if (NavMesh.SamplePosition(transform.position, out NavMeshHit startHit, radius, NavMesh.AllAreas) &&
			NavMesh.SamplePosition(targetPosition, out NavMeshHit targetHit, radius, NavMesh.AllAreas))
		{
			NavMesh.CalculatePath(startHit.position, targetHit.position, NavMesh.AllAreas, _navMeshPath);

			foreach (Vector3 corner in _navMeshPath.corners)
			{
				destinationPoints.Add(corner);
			}
		}
	}

	public void ClearDestination()
	{
		destinationPoints.Clear();
		currentSpeed = 0f;
	}

	public void AddDirectDestination(Vector3 position)
	{
		destinationPoints.Add(position);
	}

	public void StopMovement()
	{
		ClearDestination();
		velocity = Vector3.zero;
	}
}