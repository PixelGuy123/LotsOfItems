using UnityEngine;

namespace LotsOfItems.CustomItems.InvisibilityElixir;

public class ITM_SpecialPotion : ITM_InvisibilityElixir
{
    public override bool Use(PlayerManager pm)
    {
        this.pm = pm;
        foreach (var npc in pm.ec.Npcs)
            npc?.Navigator.Entity.IgnoreEntity(pm.plm.Entity, true);
        return base.Use(pm);
    }

    void OnDestroy()
    {
        if (pm != null)
        {
            foreach (var npc in pm.ec.Npcs)
                npc?.Navigator.Entity.IgnoreEntity(pm.plm.Entity, false);
        }
    }
}