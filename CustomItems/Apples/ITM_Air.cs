using LotsOfItems.CustomItems;
using UnityEngine;

internal class Baldi_AirState(Baldi baldi, NpcState prevState, SoundObject loopAudio, Sprite[] eatSprites, SoundObject thanksAudio) : Baldi_CustomAppleState(baldi, prevState, eatSprites, eatSounds: [], eatTime: 30f, thanksAudio: thanksAudio)
{
    readonly private MovementModifier pushMoveMod = new(Vector3.zero, 0f)
    {
        forceTrigger = true,
    };
    float directionCooldown = 0f;
    const float minDirectionCooldown = 5f, maxDirectionCooldown = 8f, maxSpeed = 20f, minSpeed = 8f;
    bool startedAirLoop = false, givenMoveMod = false;
    readonly SoundObject loopAudio = loopAudio;

    public override void Enter()
    {
        base.Enter();
        directionCooldown = Random.Range(minDirectionCooldown, maxDirectionCooldown);
    }

    public override void Update()
    {
        base.Update();
        if (!startedAirLoop)
        {
            if (!baldi.AudMan.AnyAudioIsPlaying)
            {
                baldi.AudMan.QueueAudio(loopAudio);
                baldi.AudMan.SetLoop(true);
                startedAirLoop = true;
            }
            return;
        }

        if (!givenMoveMod)
        {
            Vector2 dir = Random.insideUnitCircle;
            pushMoveMod.movementAddend = new Vector3(dir.x, 0f, dir.y) * Mathf.Max(Random.value * maxSpeed, minSpeed);
            baldi.Navigator.Am.moveMods.Add(pushMoveMod);
            givenMoveMod = true;
        }

        if (directionCooldown <= 0f)
        {
            Vector2 dir = Random.insideUnitCircle;
            pushMoveMod.movementAddend = new Vector3(dir.x, 0f, dir.y) * Mathf.Max(Random.value * maxSpeed, minSpeed);
            directionCooldown += Random.Range(minDirectionCooldown, maxDirectionCooldown);
        }
        directionCooldown -= baldi.ec.EnvironmentTimeScale * Time.deltaTime;

    }

    public override void Exit()
    {
        base.Exit();
        baldi.Navigator.Am.moveMods.Remove(pushMoveMod);
        baldi.audMan.FlushQueue(true);
    }
}