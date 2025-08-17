using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LotsOfItems.CustomItems.YTPs;

public class ITM_OutsideYTPS : ITM_YTPs // Does nothing by itself, it's just a generic ytp that bounces via a patch
{
}

public class OutsideYTP : MonoBehaviour
{
    private EnvironmentController ec;
    private PickupBob bob;
    private Pickup pickup;
    [SerializeField]
    private float moveTimer = 30f, teleportDelay = 0.75f;
    internal static WeightedItemObject[] potentialItems;
    private readonly List<Vector3> positionsToGo = [];

    float cooldown = 0f;
    bool initialized = false;

    public void Initialize(EnvironmentController ec, Pickup pickup)
    {
        initialized = true;
        this.ec = ec;
        cooldown = moveTimer;
        this.pickup = pickup;
        pickup.OnItemCollected += OnItemCollected;
        bob = pickup.GetComponentInChildren<PickupBob>();

        foreach (var room in ec.rooms)
        {
            if (room.itemSpawnPoints.Count != 0)
                positionsToGo.AddRange(room.itemSpawnPoints.ConvertAll(x => new Vector3(x.position.x, 0f, x.position.y)));
        }
    }

    void OnItemCollected(Pickup pickup, int _)
    {
        pickup.OnItemCollected -= OnItemCollected;
        Destroy(this);
    }

    void Update()
    {
        if (!initialized)
            return;

        cooldown -= Time.deltaTime * ec.EnvironmentTimeScale;
        if (cooldown < 0f)
        {
            cooldown += moveTimer;
            StartCoroutine(TeleportFromTo(positionsToGo[Random.Range(0, positionsToGo.Count)]));
        }
    }

    IEnumerator TeleportFromTo(Vector3 to)
    {
        float t = 0f, normT;
        bob.enabled = false;
        Vector3 start = transform.position, end = transform.position.ZeroOutY() + Vector3.down * 2f;
        while (t < teleportDelay)
        {
            t += Time.deltaTime * ec.EnvironmentTimeScale;
            normT = Mathf.Clamp01(t / teleportDelay);

            transform.position = Vector3.Lerp(start, end, normT);
            yield return null;
        }
        to.y = end.y;
        start = to; end = to + Vector3.up * 5f;
        pickup.AssignItem(WeightedItemObject.RandomSelection(potentialItems));
        t = 0f;
        while (t < teleportDelay)
        {
            t += Time.deltaTime * ec.EnvironmentTimeScale;
            normT = Mathf.Clamp01(t / teleportDelay);

            transform.position = Vector3.Lerp(start, end, normT);
            yield return null;
        }

        bob.enabled = true;
    }
}