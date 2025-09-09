using System.Collections;
using LotsOfItems.Plugin;
using MTM101BaldAPI;
using MTM101BaldAPI.PlusExtensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.Eatables;

public class ITM_SparklyWater : ITM_GenericZestyEatable
{
    public float effectDuration = 20f;
    public float blindRadius = 60f; // 6 tiles
    public float blindDuration = 5f;
    public float checkInterval = 0.5f;
    EnvironmentController ec;
    int blindedNPCs = 0;

    protected override void VirtualSetupPrefab(ItemObject itemObject)
    {
        base.VirtualSetupPrefab(itemObject);
        audEat = LotOfItemsPlugin.assetMan.Get<SoundObject>("audDrink");
    }

    public override bool Use(PlayerManager pm)
    {
        this.pm = pm;
        ec = pm.ec;
        Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audEat);
        pm.plm.stamina = pm.GetMovementStatModifier().baseStats["staminaMax"];
        StartCoroutine(SparkleAura());
        Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audEat);
        return true;
    }

    private IEnumerator SparkleAura()
    {
        gauge = Singleton<CoreGameManager>.Instance.GetHud(pm.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, effectDuration);
        float timer = effectDuration, blindInterval = 0f;
        while (timer > 0f)
        {
            timer -= Time.deltaTime * pm.PlayerTimeScale;
            gauge.SetValue(effectDuration, timer);

            if (blindInterval <= 0f)
            {
                blindInterval += checkInterval;

                Collider[] hits = Physics.OverlapSphere(pm.transform.position, blindRadius, LotOfItemsPlugin.onlyNpcLayers);
                foreach (Collider hit in hits)
                {
                    if (!hit.CompareTag("NPC")) continue;

                    NPC npc = hit.GetComponent<NPC>();
                    if (npc)
                    {
                        npc.Navigator.Entity.SetBlinded(true);
                        StartCoroutine(UnblindNPC(npc));
                    }
                }
            }
            else blindInterval -= Time.deltaTime * pm.PlayerTimeScale;

            yield return null;
        }
        // Wait until there's no npc blinded
        gauge.Deactivate();
        while (blindedNPCs > 0)
            yield return null;
        Destroy(gameObject);
    }

    private IEnumerator UnblindNPC(NPC npc)
    {
        blindedNPCs++;
        yield return new WaitForSecondsEnvironmentTimescale(ec, blindDuration);
        npc?.Navigator.Entity.SetBlinded(false);
        blindedNPCs--;
    }
}