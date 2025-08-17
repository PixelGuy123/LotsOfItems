using System.Collections;
using LotsOfItems.Plugin;
using MTM101BaldAPI;
using MTM101BaldAPI.PlusExtensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.Eatables;

public class ITM_ExplosiveZestyBar : ITM_GenericZestyEatable
{
    public float delay = 3.5f;
    public float explosionRadius = 50f;
    public float explosionForce = 100f;
    public LayerMask explosionLayer = LotOfItemsPlugin.onlyNpcPlayerLayers;
    public SoundObject audExplode;
    EnvironmentController ec;

    protected override void VirtualSetupPrefab(ItemObject itemObject)
    {
        base.VirtualSetupPrefab(itemObject);
        audExplode = LotOfItemsPlugin.assetMan.Get<SoundObject>("aud_explode");
    }

    public override bool Use(PlayerManager pm)
    {
        ec = pm.ec;
        this.pm = pm;
        var statModifier = pm.GetMovementStatModifier();

        pm.plm.stamina = statModifier.baseStats["staminaMax"] * 2.75f;
        StartCoroutine(ExplosionDelay());
        Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audEat);
        Destroy(gameObject);
        return true;
    }

    private IEnumerator ExplosionDelay()
    {
        yield return new WaitForSecondsEnvironmentTimescale(ec, delay);

        ItemExtensions.Explode(
                this,
                explosionRadius,
                explosionLayer,
                explosionForce,
                -explosionForce
            );
        Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audExplode);
    }
}