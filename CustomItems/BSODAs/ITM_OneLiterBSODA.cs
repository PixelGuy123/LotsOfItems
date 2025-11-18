using System.Collections;
using LotsOfItems.Plugin;
using MTM101BaldAPI;
using MTM101BaldAPI.Registers;
using UnityEngine;

namespace LotsOfItems.CustomItems.BSODAs;

public class ITM_OneLiterBSODA : Item, IBsodaShooter
{
    private ITM_BSODA bsodaPrefab;

    [SerializeField]
    internal float delay = 1f;

    [SerializeField]
    internal int shootCount = 7;

    public void ShootBsoda(ITM_BSODA bsoda, PlayerManager pm, Vector3 position, Quaternion rotation)
    {
        var soda = Instantiate(bsoda);
        soda.IndividuallySpawn(pm.ec, position, rotation * Vector3.forward);
    }

    public override bool Use(PlayerManager pm)
    {
        bsodaPrefab = (ITM_BSODA)ItemMetaStorage.Instance.FindByEnum(Items.Bsoda).value.item;
        StartCoroutine(ShootSodas(pm));
        return true;
    }

    private IEnumerator ShootSodas(PlayerManager pm)
    {
        for (int i = 0; i < shootCount; i++)
        {
            ShootBsoda(bsodaPrefab, pm, pm.transform.position, Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.rotation);
            yield return new WaitForSecondsEnvironmentTimescale(pm.ec, delay);
        }
        Destroy(gameObject);
    }
}