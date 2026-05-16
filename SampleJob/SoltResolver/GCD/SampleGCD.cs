using HiAuRo.ACR;

namespace SampleACR.SampleJob.SoltResolver.GCD;

public class SampleGCD:ISlotResolver
{
    public int Check()
    {
        return 0;
    }

    public void Build(Slot slot)
    {
        slot.Add(new Spell(1, SpellTargetType.Target));
    }
}