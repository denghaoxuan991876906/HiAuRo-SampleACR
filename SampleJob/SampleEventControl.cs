

namespace SampleACR.SampleJob;

public class SampleEventControl:IRotationEventHandler
{
    public void OnPreCombat()
    {
        //通常在日常本的道中未进入战斗时，已经一些职业战斗前的准备技能比如画家，镰刀
    }

    public void OnResetBattle()
    {
        //每次战斗重置
    }

    public void OnNoTarget()
    {
        //在战斗中无目标时，比如黑魔转灵极魂
    }

    public void OnSpellCastSuccess(Slot slot, Spell spell)
    {
        //作为一些读条职业判断是否某个技能读条完毕来辅助实现高级循环。
    }

    public Slot? BeforeSpell(Slot slot)
    {
        //一般是在开始执行当前 Slot 之前插入一个更高优先级 Slot；返回 null 表示不插入
        return null;
    }

    public void OnBeforeSpellCast(Slot slot, Spell spell)
    {
        //一般是在开始下一次释放技能之前插入技能
    }

    public void AfterSpell(Slot slot, Spell spell)
    {
        //所有职业的技能释放完毕，在这里可以做一些技能的统计来实现高级循环
    }

    public void OnBattleUpdate(int battleTimeMs)
    {
        //战斗中每帧轮询的方法，可以做任何想实现的功能，但是注意调整到对应的事件回调可以优化性能
    }

    public void OnTerritoryChanged()
    {
        //地图变化时的回调，可以做特定副本的特殊支持，比如减伤规划的开启，输出规划的开启
    }

    public void OnGameEvent(ITriggerCondParams eventParams)
    {
        //这个就是能让ACR发挥很多功能的地方了，看你想象，能做减伤，能做输出控制
    }

    public void OnPhaseChanged(string phaseId, string phaseName)
    {
        //这个回调是从事实轴发来的阶段信息，结合OnTerritoryChanged，OnGameEvent，能够完全实现一个副本的减伤和输出，也就是在ACR里提供轴。
        //还有提供了Data.FactState来细节调整，并且可以前向预测整个阶段，但是都是要有对应副本的事实轴才有数据。
        var nextaoetime = Data.FactState?.NextEventTimeOfType(FactEventType.StartsUsing, 16666);
    }
}
