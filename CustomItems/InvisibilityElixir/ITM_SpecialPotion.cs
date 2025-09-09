using LotsOfItems.ItemPrefabStructures;

namespace LotsOfItems.CustomItems.InvisibilityElixir;

public class ITM_SpecialPotion : ITM_InvisibilityElixir, IItemPrefab
{
    public void SetupPrefab(ItemObject itm) =>
        gaugeSprite = itm.itemSpriteSmall;
    public void SetupPrefabPost() { }
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