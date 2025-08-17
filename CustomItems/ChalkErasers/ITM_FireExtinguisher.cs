using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using UnityEngine;

namespace LotsOfItems.CustomItems.ChalkErasers;

public class ITM_FireExtinguisher : Item, IItemPrefab
{
    public CoverCloud coverCloudPrefab;
    public float cloudLifetime = 8f;
    public LayerMask layerHit = LayerMask.NameToLayer("Wall");
    public float maxDistance = 15f;
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

        // Convert start and end to grid coordinates
        var startGrid = IntVector2.GetGridPosition(pm.transform.position);
        var endGrid = IntVector2.GetGridPosition(endPoint);

        // Bresenham-like stepping between two grid cells (integer grid)
        int x0 = startGrid.x;
        int y0 = startGrid.z;
        int x1 = endGrid.x;
        int y1 = endGrid.z;

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true) // Literally a raycasting algorithm to compensate a super-optimized 3D raycasting
        {
            var cell = ec.CellFromPosition(new IntVector2(x0, y0));
            if (!cell.Null)
            {
                CoverCloud cloud = Instantiate(coverCloudPrefab, cell.CenterWorldPosition, Quaternion.identity, ec.transform);
                cloud.Ec = ec;
                cloud.StartEndTimer(cloudLifetime);
            }

            if (x0 == x1 && y0 == y1) break;
            int e2 = err * 2;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }

        Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audExtinguish);
        Destroy(gameObject);
        return true;
    }
}