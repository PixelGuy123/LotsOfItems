﻿using UnityEngine;
using System.Collections;
using PixelInternalAPI.Classes;
using LotsOfItems.ItemPrefabStructures;
using System.Collections.Generic;

namespace LotsOfItems.CustomItems.BSODAs
{
	public class ITM_SadSoda : ITM_GenericBSODA
	{
		[SerializeField]
		private NPC targetNpc;

		[SerializeField]
		private MomentumNavigator momentumNav;

		[SerializeField]
		private float detectionDistance = 9999f, nearbyNpcDistance = 12f;

		private bool hasDetectedTarget;
		readonly HashSet<NPC> touchedNPCs = [];
		DijkstraMap map;

		protected override void VirtualSetupPrefab(ItemObject itm)
		{
			base.VirtualSetupPrefab(itm);
			momentumNav = gameObject.AddComponent<MomentumNavigator>();
			momentumNav.maxSpeed = speed;

			spriteRenderer.sprite = this.GetSprite("SadSoda_drink.png", spriteRenderer.sprite.pixelsPerUnit);
			Destroy(GetComponentInChildren<ParticleSystem>().gameObject);
		}

		public override bool Use(PlayerManager pm)
		{
			base.Use(pm);

			map = new(ec, PathType.Nav, [transform]);

			// NPC detection logic
			if (Physics.Raycast(pm.transform.position, pm.transform.forward, out var hit, detectionDistance, LayerStorage.principalLookerMask))
			{
				targetNpc = hit.collider.GetComponent<NPC>();
				if (targetNpc != null)
				{
					hasDetectedTarget = true;
				}
			}
			if (!hasDetectedTarget)
				FindNearestNPCToTarget();
			momentumNav.Initialize(ec);
			return true;
		}

		public override void VirtualUpdate()
		{
			if (!hasDetectedTarget)
			{
				// Fallback to normal BSODA behavior
				base.VirtualUpdate();
				return;
			}

			if (targetNpc && targetNpc.gameObject.activeSelf)
			{
				if (Vector3.Distance(targetNpc.transform.position, transform.position) < nearbyNpcDistance)
				{
					hasDetectedTarget = false;
					momentumNav.ClearDestination();
					FindNearestNPCToTarget();
				}
				else
					momentumNav.FindPath(targetNpc.transform.position);
			}
			else
			{
				hasDetectedTarget = false;
				momentumNav.ClearDestination();
				FindNearestNPCToTarget();
			}


			moveMod.movementAddend = transform.forward * speed * ec.EnvironmentTimeScale;
			time -= Time.deltaTime * ec.EnvironmentTimeScale;

			if (time <= 0f)
				VirtualEnd();
		}

		protected override void VirtualEnd()
		{
			hasDetectedTarget = false;
			targetNpc = null;
			base.VirtualEnd();
		}

		void FindNearestNPCToTarget()
		{
			map.Calculate();

			float distanceToIt = -1f;
			targetNpc = null;
			for (int i = 0; i < ec.Npcs.Count; i++)
			{
				if (!ec.Npcs[i].Navigator.isActiveAndEnabled || !ec.Npcs[i].Navigator.Entity.InBounds || touchedNPCs.Contains(ec.Npcs[i]))
					continue;

				float value = map.Value(IntVector2.GetGridPosition(ec.Npcs[i].transform.position));
				if (distanceToIt == -1f || value < distanceToIt)
				{
					distanceToIt = value;
					targetNpc = ec.Npcs[i];
				}
			}

			if (distanceToIt != -1f)
			{
				hasDetectedTarget = true;
			}
		}

		public override bool VirtualEntityTriggerEnter(Collider other)
		{
			if (other.isTrigger && other.CompareTag("NPC") && other.TryGetComponent<NPC>(out var npc))
				touchedNPCs.Add(npc);
			
			return true;
		}

		public override bool VirtualEntityTriggerExit(Collider other)
		{
			if (other.isTrigger && other.CompareTag("NPC") && other.TryGetComponent<NPC>(out var npc))
				touchedNPCs.Remove(npc);
			return true;
		}
	}
}