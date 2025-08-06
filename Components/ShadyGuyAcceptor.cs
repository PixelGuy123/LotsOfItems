using System.Collections;
using UnityEngine;

namespace LotsOfItems.Components;

public class ShadyGuyAcceptor : MonoBehaviour, IItemAcceptor
{
    public void InsertItem(PlayerManager player, EnvironmentController ec)
    {
        shadyCollider.enabled = false; // The shady guy is no longer negotiable
        Singleton<CoreGameManager>.Instance.AddPoints(-ytpPrice, player.playerNumber, true);
        StartCoroutine(Exchange(player.itm));
    }
    public bool ItemFits(Items item) =>
        busPass == item && Singleton<CoreGameManager>.Instance.GetPoints(0) >= ytpPrice;

    IEnumerator Exchange(ItemManager itm)
    {
        itm.Remove(busPass);
        yield return null;
        itm.AddItem(exchangingPass);

        // Actually be gone
        audMan.PlaySingle(audGone);
        float t = 0f;
        float max = audGone.subDuration * 0.75f;
        while (t < max)
        {
            t += Time.deltaTime * Singleton<BaseGameManager>.Instance.Ec.EnvironmentTimeScale;

            gonerRenderer.color = Color.Lerp(Color.white, Color.clear, Mathf.Clamp01(t / max));
            yield return null;
        }

        while (audMan.AnyAudioIsPlaying)
            yield return null;

        Destroy(gameObject); // Serious be gone
    }

    [SerializeField]
    internal SpriteRenderer gonerRenderer;
    [SerializeField]
    internal AudioManager audMan;
    [SerializeField]
    internal Collider shadyCollider;
    public SoundObject audGone;
    public ItemObject exchangingPass;
    public Items busPass = Items.BusPass;
    public int ytpPrice = 500;
}