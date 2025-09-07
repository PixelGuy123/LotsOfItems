using System.Collections;
using UnityEngine;

namespace LotsOfItems.Components;

public class Playtime_TeddyBearFriend : MonoBehaviour
{
    private Playtime playtime;
    private float timer = 120f;

    public void Initialize(Playtime pt)
    {
        playtime = pt;
        StartCoroutine(FriendTimer());
    }

    private IEnumerator FriendTimer()
    {
        playtime.animator.enabled = false;
        while (timer > 0f)
        {
            timer -= Time.deltaTime * playtime.ec.NpcTimeScale;
            yield return null;
        }
        playtime.animator.enabled = true;
        Destroy(this);
    }
}