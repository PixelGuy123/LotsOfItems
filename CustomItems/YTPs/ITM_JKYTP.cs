using System.Collections.Generic;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.YTPs;

public class ITM_JKYTP : Item, IItemPrefab
{
    public static JK_VisualEffect effectPrefab;
    internal static List<ItemObject> potentialItems;

    [SerializeField]
    internal SoundObject audLaugh;
    public void SetupPrefab(ItemObject itm)
    {
        var visual = ObjectCreationExtensions.CreateSpriteBillboard(itm.itemSpriteLarge);
        visual.transform.SetParent(transform, false);
        effectPrefab = gameObject.AddComponent<JK_VisualEffect>();
        effectPrefab.renderer = visual;
        audLaugh = this.GetSoundNoSub("JKYTP_Laugh.wav", SoundType.Effect);
    }
    public void SetupPrefabPost() =>
        potentialItems = ItemObjectExtensions.GetAllShoppingItems();

    public override bool Use(PlayerManager pm)
    {
        Singleton<CoreGameManager>.Instance.AddPoints(1, pm.playerNumber, true);
        pm.ec.MakeNoise(pm.transform.position, 48);
        Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audLaugh);
        return true;
    }
}

public class ITM_JKYTP_PickupBehavior : MonoBehaviour
{
    public Pickup pickup;
    public SpriteRenderer ren;

    public void Initialize(Pickup p)
    {
        pickup = p;
        ren = p.itemSprite;

        // Transform into random item large icon
        ItemObject randomItem = ITM_JKYTP.potentialItems[Random.Range(0, ITM_JKYTP.potentialItems.Count)];
        ren.sprite = randomItem.itemSpriteLarge;
        pickup.transform.localScale = new Vector3(Random.Range(0.5f, 2f), Random.Range(0.5f, 2f), 1f);
        pickup.OnItemCollected += OnCollect;
    }

    void OnCollect(Pickup pickup, int player)
    {
        Destroy(this);
        Instantiate(ITM_JKYTP.effectPrefab, pickup.transform.position, Quaternion.identity);
    }

    void OnDestroy()
    {
        if (pickup)
        {
            pickup.OnItemCollected -= OnCollect;
            pickup.itemSprite.transform.localScale = Vector3.one;
        }
    }
}

public class JK_VisualEffect : MonoBehaviour
{
    public float timer = 0f;
    public SpriteRenderer renderer;

    void Start()
    {
        transform.position += Vector3.up * 2f;
    }

    void Update()
    {
        timer += Time.deltaTime;

        transform.localScale = new Vector3(MoreOrLess, MoreOrLess, 1f);
        renderer.SetSpriteRotation(Random.Range(-45f, 45f));

        if (timer > 2f) // Disappear after a bit
        {
            Destroy(gameObject);
        }
    }

    float MoreOrLess => Random.value >= 0.55f ? Random.Range(1.65f, 2f) : Random.Range(0.25f, 0.65f);
}