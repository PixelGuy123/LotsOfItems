using System.Collections.Generic;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using UnityEngine;

namespace LotsOfItems.CustomItems.ChalkErasers;

public class ITM_ElectricChalkEraser : ChalkEraser, IItemPrefab
{
    public int radius = 2; // 5x5 area

    public void SetupPrefab(ItemObject itm)
    {
        cloud = ParticleExtensions.GetRawChalkParticleGenerator();
        cloud.name = "ElectricCoverCloud";
        var renderer = cloud.GetComponentInChildren<ParticleSystemRenderer>();

        var newTexture = Instantiate(LotOfItemsPlugin.assetMan.Get<Texture2D>("DustCloudTexture"));
        newTexture.name = "YellowDustCloud";

        renderer.material = new Material(renderer.material)
        {
            name = "ElectricCloud",
            mainTexture = newTexture.ApplyColorLevel(Color.yellow)
        };

        setTime = 15f;
    }

    public void SetupPrefabPost() { }

    public override bool Use(PlayerManager pm)
    {
        ec = pm.ec;
        IntVector2 gridPosition = IntVector2.GetGridPosition(pm.transform.position);
        pos.x = gridPosition.x * 10f + 5f;
        pos.z = gridPosition.z * 10f + 5f;
        pos.y = ec.transform.position.y + 5f;

        foreach (Cell cell in ec.FindCellsInNavigableRange(radius, gridPosition))
        {
            CoverCloud cloud = Instantiate(this.cloud, cell.CenterWorldPosition, Quaternion.identity);
            cloud.Ec = ec;
            cloud.StartEndTimer(setTime);
            cloud.gameObject.AddComponent<ElectricCloudEffect>();
        }

        Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audUse);

        Destroy(gameObject);
        return true;
    }
}

public class ElectricCloudEffect : MonoBehaviour
{
    readonly private MovementModifier shakeMod = new(Vector3.zero, 0.85f, 0);
    readonly private List<Entity> affectedEntities = [];
    public float shockIntensity = 12.5f;

    void OnTriggerEnter(Collider other)
    {
        Entity entity = other.GetComponent<Entity>();
        if (entity && !affectedEntities.Contains(entity))
        {
            entity.ExternalActivity.moveMods.Add(shakeMod);
            affectedEntities.Add(entity);
        }
    }

    void OnTriggerExit(Collider other)
    {
        Entity entity = other.GetComponent<Entity>();
        if (entity && affectedEntities.Contains(entity))
        {
            entity.ExternalActivity.moveMods.Remove(shakeMod);
            affectedEntities.Remove(entity);
        }
    }

    void Update()
    {
        if (Time.timeScale != 0)
            shakeMod.movementAddend = Random.insideUnitSphere * shockIntensity;
    }

    void OnDestroy()
    {
        foreach (var entity in affectedEntities)
            entity?.ExternalActivity.moveMods.Remove(shakeMod);
    }
}