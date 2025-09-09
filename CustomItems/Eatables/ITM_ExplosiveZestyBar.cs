using System.Collections;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using MTM101BaldAPI.PlusExtensions;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.Eatables;

public class ITM_ExplosiveZestyBar : ITM_GenericZestyEatable
{
    public float explosionRadius = 50f;
    public float explosionForce = 100f;
    public LayerMask explosionLayer = LotOfItemsPlugin.onlyNpcPlayerLayers;
    public SoundObject audExplode, audHeat;
    public AudioManager audMan;
    EnvironmentController ec;

    protected override void VirtualSetupPrefab(ItemObject itemObject)
    {
        base.VirtualSetupPrefab(itemObject);
        audExplode = LotOfItemsPlugin.assetMan.Get<SoundObject>("aud_explode");
        audHeat = this.GetSoundNoSub("ExplosiveZestyBar_Heat.wav", SoundType.Effect);
        audMan = gameObject.CreateAudioManager(45f, 65f).MakeAudioManagerNonPositional();
    }

    public override bool Use(PlayerManager pm)
    {
        ec = pm.ec;
        this.pm = pm;
        var statModifier = pm.GetMovementStatModifier();

        pm.plm.stamina = statModifier.baseStats["staminaMax"] * 2.75f;
        StartCoroutine(ExplosionDelay());
        Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audEat);
        return true;
    }

    private IEnumerator ExplosionDelay()
    {
        audMan.QueueAudio(audHeat);
        while (audMan.QueuedAudioIsPlaying)
            yield return null;

        transform.position = pm.transform.position;
        ItemExtensions.Explode(
                this,
                explosionRadius,
                explosionLayer,
                explosionForce,
                -explosionForce
            );
        Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audExplode);
        Destroy(gameObject);
    }
}