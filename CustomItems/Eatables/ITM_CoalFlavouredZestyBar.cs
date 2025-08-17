using System.Collections;
using LotsOfItems.ItemPrefabStructures;
using MTM101BaldAPI.Components;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.Eatables;

public class ITM_CoalFlavouredZestyBar : ITM_GenericZestyEatable
{
    public float effectDuration = 5f;
    [Range(0f, 1f)]
    public float slowMultiplier = 0.5f;
    public float fovChange = -25f;
    public int noiseValue = 25;
    [SerializeField]
    internal SoundObject audCough;
    MovementModifier slowMod;
    readonly ValueModifier fovMod = new();

    protected override bool CanBeDestroyed() => false;

    public override bool Use(PlayerManager pm)
    {
        this.pm = pm;
        audCough = this.GetSound("CoalFlavouredZestyBar_Cough.wav", "LtsOItems_Vfx_Cough", SoundType.Voice, Color.white);
        slowMod = new(Vector3.zero, slowMultiplier);
        StartCoroutine(CoughingFit());
        return true;
    }

    private IEnumerator CoughingFit()
    {
        Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audCough);

        pm.Am.moveMods.Add(slowMod);
        var customCam = pm.GetCustomCam();
        customCam.SlideFOVAnimation(fovMod, fovChange, 1.25f);

        float fovResetLimit = effectDuration * 0.65f;
        bool resettedFov = false;
        float timer = effectDuration;
        while (timer > 0f)
        {
            timer -= Time.deltaTime * pm.ec.EnvironmentTimeScale;
            pm.ec.MakeNoise(pm.transform.position, noiseValue);

            if (timer <= fovResetLimit && !resettedFov)
            {
                customCam.ResetSlideFOVAnimation(fovMod, 2.5f);
                resettedFov = true;
            }
            yield return null;
        }

        pm.Am.moveMods.Remove(slowMod);

        Destroy(gameObject);
    }
}