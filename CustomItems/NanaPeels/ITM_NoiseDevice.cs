using LotsOfItems.CustomItems;
using LotsOfItems.ItemPrefabStructures;
using PixelInternalAPI.Extensions;
using UnityEngine;

public class ITM_NoiseDevice : ITM_GenericNanaPeel
{
    public int noiseValue = 112;
    public Canvas canvasIndicator;
    public Sprite gaugeSprite;
    public SoundObject audBeep;
    private bool armed = false;

    protected override void VirtualSetupPrefab(ItemObject itm)
    {
        base.VirtualSetupPrefab(itm);

        var renderer = GetComponentInChildren<SpriteRenderer>();
        renderer.sprite = itm.itemSpriteLarge;

        audBeep = this.GetSoundNoSub("NoiseDevice_Beep.wav", SoundType.Effect);

        canvasIndicator = ObjectCreationExtensions.CreateCanvas();
        canvasIndicator.transform.SetParent(transform);
        canvasIndicator.name = "NoiseDeviceIndicatorCanvas";
        canvasIndicator.gameObject.SetActive(false);

        var image = ObjectCreationExtensions.CreateImage(canvasIndicator, this.GetSprite("NoiseDevice_indicator.png", 25f), false);
        image.transform.localPosition = Vector3.zero;
    }

    internal override void OnFloorHit()
    {
        armed = true;

        canvasIndicator.gameObject.SetActive(true);
        canvasIndicator.worldCamera = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).canvasCam;

        entity.SetFrozen(true);
        throwSpeed = 0f;
    }

    public void Activate()
    {
        if (!armed) return;
        armed = false;
        Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audBeep);
        ec.MakeNoise(transform.position, noiseValue);

        Destroy(gameObject);
    }

    internal override void VirtualUpdate()
    {
        base.VirtualUpdate();
        if (Time.timeScale != 0f && Singleton<InputManager>.Instance.GetDigitalInput("Interact", true)) // Expects the player to click (interact)
            Activate();
    }

    internal override bool EntityTriggerStayOverride(Collider other) => false;
}