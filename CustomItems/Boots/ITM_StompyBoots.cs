using System.Collections;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using UnityEngine;

namespace LotsOfItems.CustomItems.Boots;

public class ITM_StompyBoots : ITM_Boots, IItemPrefab
{
    [SerializeField]
    internal float stompDistance = 8f, stompPushRadius = 16f, stompPushForce = 35f;

    [SerializeField]
    internal SoundObject[] audStomps;

    [SerializeField]
    internal LayerMask collisionLayer = LotOfItemsPlugin.onlyNpcLayers;

    private float distanceAccumulated;
    private Vector3 lastPosition;
    bool stompFlag;

    public void SetupPrefab(ItemObject itm)
    {
        audStomps = [
            this.GetSoundNoSub("StompyBoots_Stomp_0.wav", SoundType.Effect),
            this.GetSoundNoSub("StompyBoots_Stomp_1.wav", SoundType.Effect)
            ];
        gaugeSprite = itm.itemSpriteSmall;
        setTime = 15f;
    }

    public void SetupPrefabPost() { }

    public override bool Use(PlayerManager pm)
    {
        this.pm = pm;
        gauge = Singleton<CoreGameManager>.Instance.GetHud(pm.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, setTime);
        lastPosition = pm.transform.position;
        StartCoroutine(StompyTimer());
        return true;
    }

    private IEnumerator StompyTimer()
    {
        float time = setTime;
        while (time > 0f)
        {
            time -= Time.deltaTime * pm.PlayerTimeScale;
            gauge.SetValue(setTime, time);

            float dist = Vector3.Distance(pm.transform.position, lastPosition);
            distanceAccumulated += dist;
            lastPosition = pm.transform.position;

            if (distanceAccumulated >= stompDistance && pm.plm.Entity.Grounded)
            {
                StompEffect();
                distanceAccumulated -= stompDistance;
            }

            yield return null;
        }
        gauge.Deactivate();
        Destroy(gameObject);
    }

    private void StompEffect()
    {
        // Play stomp sound
        Singleton<CoreGameManager>.Instance.audMan.PlaySingle(stompFlag ? audStomps[1] : audStomps[0]);
        stompFlag = !stompFlag;

        // Push NPCs
        transform.position = pm.transform.position;
        ItemExtensions.Explode(
                            this,
                            stompPushRadius,
                            collisionLayer,
                            stompPushForce,
                            -stompPushForce
                        );
    }
}
