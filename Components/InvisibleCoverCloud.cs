using System.Collections;
using System.Collections.Generic;
using MTM101BaldAPI;
using PixelInternalAPI.Classes;
using UnityEngine;

namespace LotsOfItems.Components;

public class InvisibleCoverCloud : EnvironmentObject
{
    // Similar to a singleton
    static InvisibleCoverCloud prefab;
    public static InvisibleCoverCloud GetPrefab()
    {
        if (prefab) return prefab;

        var cloud = new GameObject("InvisibleCoverCloud");
        cloud.ConvertToPrefab(true);
        cloud.layer = LayerStorage.ignoreRaycast;

        var comp = cloud.AddComponent<InvisibleCoverCloud>();
        var col = cloud.AddComponent<BoxCollider>();
        col.size = Vector3.one * 10f;
        col.isTrigger = true;

        comp.trigger = col;

        prefab = comp;

        return comp;
    }

    // Actual code below
    public void StartDelay(float time)
    {
        StartCoroutine(StartDelayTimer(time));
    }

    private IEnumerator StartDelayTimer(float time)
    {
        while (time > 0f)
        {
            time -= Time.deltaTime * ec.EnvironmentTimeScale;
            yield return null;
        }
        trigger.enabled = true;
        yield break;
    }

    public void StartEndTimer(float time)
    {
        StartCoroutine(EndTimer(time));
    }

    private IEnumerator EndTimer(float time)
    {
        while (time > 0f)
        {
            time -= Time.deltaTime * ec.EnvironmentTimeScale;
            yield return null;
        }
        Destroy(gameObject);
        yield break;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
        {
            Entity component = other.GetComponent<Entity>();
            if (component != null)
            {
                entities.Add(component);
                component.SetHidden(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.isTrigger)
        {
            Entity component = other.GetComponent<Entity>();
            if (component != null && entities.Contains(component))
            {
                component.SetHidden(false);
                entities.Remove(component);
            }
        }
    }

    private void OnDestroy()
    {
        while (entities.Count > 0)
        {
            entities[0].SetHidden(false);
            entities.RemoveAt(0);
        }
    }

    readonly private List<Entity> entities = [];

    [SerializeField]
    internal Collider trigger;
}