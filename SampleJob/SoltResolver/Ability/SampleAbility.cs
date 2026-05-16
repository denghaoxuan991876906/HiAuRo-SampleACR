
namespace SampleACR.SampleJob.SoltResolver.Ability;

public class SampleAbility : ISlotResolver
{
    public int Check()
    {
        if (Data.Me.Object != null && !Data.Me.Object.IsCasting )
        {
            return 1;
        }

        return -1;
    }

    public void Build(Slot slot)
    {
        slot.Add(new Spell(1, SpellTargetType.Self));
    }
}