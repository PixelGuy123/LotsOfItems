using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.PrincipalWhistles;

public class ITM_HallwayEchoWhistle : Item, IItemPrefab
{
    public void SetupPrefab(ItemObject itm)
    {
        audWhistle = this.GetSound("HallwayEchoWhistle_Whistle.wav", "Sfx_Items_PriWhistle", SoundType.Effect, Color.white);
        audNotHere = GenericExtensions.FindResourceObjectByName<SoundObject>("ErrorMaybe");
    }

    public void SetupPrefabPost() { }

    [SerializeField]
    private SoundObject audWhistle, audNotHere;

    [SerializeField]
    private int noiseValue = 32;

    [SerializeField]
    internal LayerMask hitLayer = LayerMask.NameToLayer("Wall"); // That's a real layer

    public override bool Use(PlayerManager pm)
    {
        this.pm = pm;
        Transform cameraTransform = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform;

        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, 9999, hitLayer))
        {
            Vector3 noisePosition = pm.ec.CellFromPosition(hit.point).CenterWorldPosition; // hit.point apparently gets the exact point hit, easier than getting transform
            pm.ec.MakeNoise(noisePosition, noiseValue);
            pm.ec.CallOutPrincipals(noisePosition);

            Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audWhistle);
            Destroy(gameObject);
            return true;
        }

        Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audNotHere);
        Destroy(gameObject);

        return false;
    }
}
