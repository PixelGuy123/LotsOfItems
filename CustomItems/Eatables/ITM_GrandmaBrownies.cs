namespace LotsOfItems.CustomItems.Eatables;

public class ITM_GrandmaBrownies : ITM_Reusable_GenericZestyEatable
{
    static int bitesUsed = 0;
    bool hasBeenUsed = false;

    protected override void VirtualSetupPrefab(ItemObject itemObject)
    {
        base.VirtualSetupPrefab(itemObject);

        staminaGain = 200;
        speedMultiplier = 0.75f;
        speedAffectorAffectsRunSpeed = true;
        speedAffectorAffectsWalkSpeed = true;
        affectorTime = 30f;
    }

    public override bool Use(PlayerManager pm)
    {
        bitesUsed++;
        hasBeenUsed = true;
        staminaGain /= bitesUsed;
        bool flag = base.Use(pm);
        this.pm = pm;
        pm.RuleBreak("Eating", 2.5f);
        return flag;
    }

    void OnDestroy()
    {
        if (hasBeenUsed)
            --bitesUsed;
    }


}

