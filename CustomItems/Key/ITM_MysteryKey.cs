using System.Collections.Generic;
using LotsOfItems.ItemPrefabStructures;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.Key;

public class ITM_MysteryKey : Item, IItemPrefab
{
    public ItemObject nextItem; // The 1-use version

    [SerializeField]
    [Range(0f, 1f)]
    internal float failChance = 0.25f, teleportRandomNPCChance = 0.25f;

    [SerializeField]
    internal SoundObject audTeleport;

    [SerializeField]
    private Items item;

    public void SetupPrefab(ItemObject itm)
    {
        audTeleport = GenericExtensions.FindResourceObjectByName<SoundObject>("Teleport");
        item = itm.itemType;
    }
    public void SetupPrefabPost() { }


    public override bool Use(PlayerManager pm)
    {
        Destroy(gameObject);
        if (Physics.Raycast(pm.transform.position, Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, out RaycastHit hit, pm.pc.Reach))
        {
            var acceptor = hit.transform.GetComponent<IItemAcceptor>();
            if (acceptor != null && acceptor.ItemFits(item))
            {
                float roll = Random.value;
                // If roll is above 75%, it implies a 25% of npc getting there. The other 25% below is just straight fail. The middle is success
                if (roll <= failChance) // 25% fail
                { } // Do nothing
                else if (roll >= (1f - teleportRandomNPCChance)) // 25% tp
                    TeleportRandomNPC(pm);
                else // Must be between 25% and 75%
                    acceptor.InsertItem(pm, pm.ec);


                if (nextItem != null)
                {
                    pm.itm.SetItem(nextItem, pm.itm.selectedItem);
                    return false;
                }

                return true;
            }
        }
        return false;
    }

    private void TeleportRandomNPC(PlayerManager pm)
    {
        List<NPC> potentialTargets = [];
        foreach (NPC npc in pm.ec.Npcs)
        {
            if (npc.Character != Character.Baldi)
                potentialTargets.Add(npc);
        }

        if (potentialTargets.Count != 0)
        {
            NPC target = potentialTargets[Random.Range(0, potentialTargets.Count)];
            target.Navigator.Entity.Teleport(pm.transform.position);
            Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audTeleport);
        }
    }
}