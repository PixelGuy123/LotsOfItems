using MTM101BaldAPI;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.Components;

public class ModdedQuickExplosion : MonoBehaviour
{
    public static ModdedQuickExplosion CreatePrefab(float speed, params Sprite[] sprites) =>
        CreatePrefab(speed, null, sprites);
    public static ModdedQuickExplosion CreatePrefab(float speed, SoundObject[] startingSounds, params Sprite[] sprites)
    {
        var quickExplosion = ObjectCreationExtensions.CreateSpriteBillboard(sprites[0]);
        quickExplosion.name = "ModdedQuickExplosion";
        quickExplosion.gameObject.ConvertToPrefab(true);

        var comp = quickExplosion.gameObject.AddComponent<ModdedQuickExplosion>(); // Component added!
        comp.animComp = quickExplosion.gameObject.AddComponent<AnimationComponent>();
        comp.animComp.speed = speed;
        comp.animComp.renderers = [quickExplosion];
        comp.animComp.animation = sprites;

        if (startingSounds != null)
            comp.audMan = quickExplosion.gameObject.CreatePropagatedAudioManager().AddStartingAudiosToAudioManager(false, startingSounds);

        return comp;
    }
    public AnimationComponent animComp;
    public PropagatedAudioManager audMan;
    bool animationEnded = false;

    public void Awake()
    {
        animComp.Initialize(Singleton<BaseGameManager>.Instance.Ec);
        animComp.StopLastFrameMode();
    }

    void Update()
    {
        if (!animationEnded && animComp.Paused) // Paused means the animation ended
        {
            animationEnded = true;
            for (int i = 0; i < animComp.renderers.Length; i++)
                animComp.renderers[i].enabled = false;
        }

        if (animationEnded && !audMan.AnyAudioIsPlaying) // If the animation ended and nothing's playing, destroy this
            Destroy(gameObject);
    }
}