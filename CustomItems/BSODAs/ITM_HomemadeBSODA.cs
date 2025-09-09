using System.Collections;
using LotsOfItems.Components;
using LotsOfItems.Plugin;
using MTM101BaldAPI;
using UnityEngine;

namespace LotsOfItems.CustomItems.BSODAs;

public class ITM_HomemadeBSODA : ITM_GenericBSODA
{
    public float disableDuration = 30f;
    public QuickExplosion explosionPrefab;

    protected override void VirtualSetupPrefab(ItemObject itm)
    {
        base.VirtualSetupPrefab(itm);
        explosionPrefab = LotOfItemsPlugin.assetMan.Get<QuickExplosion>("genericExplosionPrefab");
    }

    public override bool VirtualEntityTriggerEnter(Collider other)
    {
        if (other.isTrigger && other.CompareTag("NPC"))
        {
            NPC npc = other.GetComponent<NPC>();
            if (npc)
            {
                npc.StartCoroutine(DisableNPC(npc));
                return false;
            }
        }
        return true;
    }

    private IEnumerator DisableNPC(NPC npc)
    {
        npc.Navigator.Entity.SetFrozen(true);
        npc.Navigator.Entity.SetVisible(false);
        npc.Navigator.Entity.SetInteractionState(false);
        npc.Navigator.Entity.SetBlinded(true);
        npc.Navigator.Entity.propagatedAudioManager.gameObject.AddComponent<Marker_AudioManagerMute>();

        Instantiate(explosionPrefab, npc.spriteRenderer[0].transform);

        yield return new WaitForSecondsEnvironmentTimescale(ec, disableDuration);

        // --- Re-enable NPC ---
        npc.Navigator.Entity.SetFrozen(false);
        npc.Navigator.Entity.SetVisible(true);
        npc.Navigator.Entity.SetInteractionState(true);
        npc.Navigator.Entity.SetBlinded(false);
        Destroy(npc.Navigator.Entity.propagatedAudioManager.GetComponent<Marker_AudioManagerMute>());
    }
}