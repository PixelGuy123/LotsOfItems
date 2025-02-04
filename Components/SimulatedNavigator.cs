using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

namespace LotsOfItems.Components
{
	public static class NavigatorSimulatedClass
	{
		private static Cell _startTile;
		private static Cell _targetTile;
		static List<Cell> _path;
		static NavMeshHit _startHit, _targetHit;
		static NavMeshPath _navMeshPath = null;

		public static List<Vector3> FindNavPath(this EnvironmentController ec, Transform caller, Vector3 startPos, Vector3 targetPos)
		{
			_navMeshPath ??= new();

			List<Vector3> destinationPoints = [];
			_startTile = ec.CellFromPosition(startPos);
			_targetTile = ec.CellFromPosition(targetPos);

			ec.FindPathStatic(out bool flag);

			if (flag)
				ConvertPath(ec, _path, destinationPoints, targetPos, caller);



			return destinationPoints;
		}

		static void FindPathStatic(this EnvironmentController ec, out bool flag) => // In case tweaks+ wanna override this lol
			ec.FindPath(_startTile, _targetTile, PathType.Nav, out _path, out flag);
		


		static void ConvertPath(EnvironmentController ec, List<Cell> path, List<Vector3> destinationPoints, Vector3 targetPos, Transform transform)
		{
			Cell cell = null, cell2 = null;

			bool flag = false;
			destinationPoints.Clear();

			while (path.Count > 0)
			{
				if (path[0].open)
				{
					if (flag)
					{
						if (path[0].openTiles.Contains(cell2))
						{
							cell2 = path[0];
						}
						else
						{
							ec.BuildNavPath(cell, cell2, targetPos, transform, destinationPoints);
							cell = path[0];
							cell2 = path[0];
						}
					}
					else
					{
						flag = true;
						cell = path[0];
						cell2 = path[0];
					}
					if (path.Count == 1)
					{
						ec.BuildNavPath(cell, cell2, targetPos, transform, destinationPoints);
					}
				}
				else
				{
					if (flag)
					{
						flag = false;
						ec.BuildNavPath(cell, cell2, targetPos, transform, destinationPoints);
					}
					destinationPoints.Add(path[0].FloorWorldPosition);
				}
				path.RemoveAt(0);
			}
			if (destinationPoints.Count > 1 && (destinationPoints[1] - transform.position).magnitude < (destinationPoints[1] - destinationPoints[0]).magnitude)
			{
				destinationPoints.RemoveAt(0);
			}

			destinationPoints.Add(Vector3.right * targetPos.x + Vector3.forward * targetPos.z);
			if (destinationPoints.Count > 2 && (destinationPoints[destinationPoints.Count - 3] - destinationPoints[destinationPoints.Count - 1]).magnitude < (destinationPoints[destinationPoints.Count - 3] - destinationPoints[destinationPoints.Count - 2]).magnitude)
			{
				destinationPoints.RemoveAt(destinationPoints.Count - 2);
			}
		}

		static void BuildNavPath(this EnvironmentController ec, Cell firstOpenTile, Cell lastOpenTile, Vector3 targetPosition, Transform transform, List<Vector3> destinationPoints)
		{
			if (ec.CellFromPosition(transform.position) != firstOpenTile || !NavMesh.SamplePosition(transform.position.ZeroOutY(), out _, 1f, -1))
			{
				NavMesh.SamplePosition(firstOpenTile.FloorWorldPosition, out _startHit, 10f, -1);
			}
			else
			{
				NavMesh.SamplePosition(transform.position.ZeroOutY(), out _startHit, 10f, -1);
			}
			if (ec.CellFromPosition(targetPosition) != lastOpenTile || !NavMesh.SamplePosition(targetPosition.ZeroOutY(), out _, 1f, -1))
			{
				NavMesh.SamplePosition(lastOpenTile.FloorWorldPosition, out _targetHit, 10f, -1);
			}
			else
			{
				NavMesh.SamplePosition(targetPosition.ZeroOutY(), out _targetHit, 10f, -1);
			}
			NavMesh.CalculatePath(_startHit.position, _targetHit.position, -1, _navMeshPath);
			foreach (Vector3 vector in _navMeshPath.corners)
			{
				destinationPoints.Add(Vector3.zero + Vector3.right * vector.x + Vector3.forward * vector.z);
			}
		}

	}
}
