using System.Collections.Generic;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using MTM101BaldAPI;
using MTM101BaldAPI.Registers;
using UnityEngine;

namespace LotsOfItems.CustomItems.BSODAs;

public class ITM_DietDietDietDietBSODA : Item, IItemPrefab
{
    public void SetupPrefab(ItemObject itm)
    {
        var four_diet_soda = ItemExtensions.GetVariantInstance<ITM_BSODA>(Items.Bsoda);
        four_diet_soda.speed = 2.5f;
        four_diet_soda.time = 10f;
        four_diet_soda.DestroyParticleIfItHasOne();
        four_diet_soda.spriteRenderer.sprite = this.GetSprite("DietDietDietDiet_Soda.png", four_diet_soda.spriteRenderer.sprite.pixelsPerUnit);

        sound = four_diet_soda.sound;

        bsodaChain = [
            four_diet_soda,
            (ITM_BSODA)ItemMetaStorage.Instance.FindByEnumFromMod(EnumExtensions.GetFromExtendedName<Items>("DietDietBSODA"), LotOfItemsPlugin.plug.Info).value.item,
            (ITM_BSODA)ItemMetaStorage.Instance.FindByEnumFromMod(EnumExtensions.GetFromExtendedName<Items>("DietDietDietBSODA"), LotOfItemsPlugin.plug.Info).value.item,
        ];
    }
    public void SetupPrefabPost() { }

    public override bool Use(PlayerManager pm)
    {
        this.pm = pm;
        Singleton<CoreGameManager>.Instance.audMan.PlaySingle(sound);
        originalPlayerRotation = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward;
        lastRecordedPositionsOfSodas = [pm.transform.position, pm.transform.position];
        CreateBSODAs();
        return true;
    }

    void CreateBSODAs()
    {
        if (currentChainIndex >= bsodaChain.Length) // Failsafe
        {
            Destroy(gameObject);
            return;
        }

        // Normal Direction
        var bsoda = Instantiate(bsodaChain[currentChainIndex], pm.transform.position, Quaternion.identity);
        bsoda.IndividuallySpawn(pm.ec, lastRecordedPositionsOfSodas[0], originalPlayerRotation);
        activeSodas.Add(bsoda);

        // Inverse direction
        bsoda = Instantiate(bsodaChain[currentChainIndex], pm.transform.position, Quaternion.identity);
        bsoda.IndividuallySpawn(pm.ec, lastRecordedPositionsOfSodas[1], -originalPlayerRotation);
        activeSodas.Add(bsoda);

        if (++currentChainIndex >= bsodaChain.Length)
        {
            Destroy(gameObject); // Basically, after reaching every possible bsoda variation, just destroy the object before it starts throwing errors by itself
        }
    }

    void Update()
    {
        for (int i = 0; i < activeSodas.Count; i++)
        {
            if (activeSodas[i])
                lastRecordedPositionsOfSodas[i] = activeSodas[i].transform.position; // 0 -> forward soda; 1 -> backwards soda
            else
                activeSodas.RemoveAt(i--);
        }

        if (activeSodas.Count == 0)
        {
            CreateBSODAs();
        }
    }

    int currentChainIndex = 0;

    readonly List<ITM_BSODA> activeSodas = [];

    Vector3[] lastRecordedPositionsOfSodas;

    Vector3 originalPlayerRotation;

    [SerializeField]
    internal ITM_BSODA[] bsodaChain;

    [SerializeField]
    internal SoundObject sound;

    // ^^ In summary, when activeSodas count reach 0 (they despawned), the chain index increases by 1 and spawns a new bsoda in the previous positions of those sodas.
}