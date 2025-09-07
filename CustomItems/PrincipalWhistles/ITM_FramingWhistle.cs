using UnityEngine;
namespace LotsOfItems.CustomItems.PrincipalWhistles;

public class ITM_FramingWhistle : ITM_PrincipalWhistle
{
    public override bool Use(PlayerManager pm)
    {
        base.Use(pm); // Summons the principal

        NPC closestNpc = null;
        float closestDist = float.MaxValue;

        foreach (NPC npc in pm.ec.Npcs)
        {
            float dist = Vector3.Distance(pm.transform.position, npc.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestNpc = npc;
            }
        }

        closestNpc?.SetGuilt(5f, "Bullying");

        return true;
    }
}