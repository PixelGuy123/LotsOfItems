using System.Collections.Generic;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using UnityEngine;

namespace LotsOfItems.CustomItems.ChalkErasers;

public class ITM_FireExtinguisher : Item, IItemPrefab
{
    public CoverCloud coverCloudPrefab;
    public float cloudLifetime = 15f;
    public LayerMask layerHit = LayerMask.NameToLayer("Wall");
    public float maxDistance = 85f;
    [SerializeField]
    internal SoundObject audExtinguish;
    EnvironmentController ec;

    public void SetupPrefab(ItemObject itm)
    {
        coverCloudPrefab = ParticleExtensions.GetRawChalkParticleGenerator();
        audExtinguish = this.GetSoundNoSub("FireExtinguisher_Active.wav", SoundType.Effect);
    }

    public void SetupPrefabPost() { }

    public override bool Use(PlayerManager pm)
    {
        this.pm = pm;
        ec = pm.ec;
        Physics.Raycast(pm.transform.position, pm.transform.forward, out var hitInfo, maxDistance, layerHit);

        // If we didn't hit anything, use the max distance point
        Vector3 endPoint = hitInfo.collider ? hitInfo.point : pm.transform.position + pm.transform.forward * maxDistance;
        float worldDistance = Vector3.Distance(pm.transform.position, endPoint);

        float stepSize = 1.0f; // Adjust this based on your desired cloud density
        int steps = Mathf.CeilToInt(worldDistance / stepSize);

        HashSet<Cell> coveredCells = [];

        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            Vector3 currentWorldPos = Vector3.Lerp(pm.transform.position, endPoint, t); // Lerp from one point to another

            var gridPos = IntVector2.GetGridPosition(currentWorldPos);
            var cell = ec.CellFromPosition(gridPos);

            if (!cell.Null && !coveredCells.Contains(cell))
            {
                coveredCells.Add(cell);
                CoverCloud cloud = Instantiate(coverCloudPrefab, cell.CenterWorldPosition, Quaternion.identity, ec.transform);
                cloud.Ec = ec;
                cloud.StartEndTimer(cloudLifetime);
            }
        }

        Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audExtinguish);
        Destroy(gameObject);
        return true;
    }
}