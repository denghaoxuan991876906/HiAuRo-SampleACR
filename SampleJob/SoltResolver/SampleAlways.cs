

namespace SampleACR.SampleJob.SoltResolver;

public class SampleAlways:ISlotResolver
{
    public int Check()
    {
        _ = GameHelper.IsInCombat();
        if (Data.Target.Current.DistanceToPlayer() < 10)
            return 1;
        return -1;
    }

    public void Build(Slot slot)
    {
        slot.Add(Spell.Idle);
    }
}
