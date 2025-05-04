using System.Collections;
using LotsOfItems.ItemPrefabStructures;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.YTPs;

public class ITM_TeleportingYTP : ITM_YTPs, IItemPrefab
{
    [SerializeField]
    internal SoundObject audPlay;
    [SerializeField]
    internal AudioManager audMan;

    public static int maxTpsPerInstance = 5;

    public void SetupPrefab(ItemObject itm)
    {
        audMan = gameObject.CreatePropagatedAudioManager(45f, 75f);
    }
    public void SetupPrefabPost() { }

    public override bool Use(PlayerManager pm)
    {
        transform.position = pm.transform.position;
        Singleton<CoreGameManager>.Instance.AddPoints(value, pm.playerNumber, true);
        StartCoroutine(WaitToDespawn());
        return true;
    }

    IEnumerator WaitToDespawn()
    {
        audMan.PlaySingle(audPlay);
        while (audMan.AnyAudioIsPlaying)
            yield return null;
        Destroy(gameObject);
    }

}

public class TeleportingYTP_TpMarker : MonoBehaviour
{
    internal int tpTimes = 0;
}