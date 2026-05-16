using SampleACR.SampleJob.SoltResolver;
using SampleACR.SampleJob.SoltResolver.Ability;
using SampleACR.SampleJob.SoltResolver.GCD;
using SampleACR.SampleJob.UI;

namespace SampleACR.SampleJob;

public class SampleEntry : IRotationEntry, ISettingsProvider<SampleSettings>
{
    private List<SlotResolverData> SlotResolverDatas = 
        [
            new SlotResolverData{Resolver = new SampleAlways(),Mode = SlotMode.Always},
            new SlotResolverData{Resolver = new SampleGCD(), Mode = SlotMode.Gcd},
            new SlotResolverData{Resolver = new SampleAbility(), Mode = SlotMode.OffGcd}
        ];
    public Rotation? Build(string settingFolder)
    {
        var rot = new Rotation
        {
            SlotResolvers =  SlotResolverDatas , AcrType = AcrType.PvE,
            MinLevel = 0,
            MaxLevel = 100,
            TargetJob = Jobs.None,
            Description = "Sample Job 一个ACR示例"
        };
        //注册ACR的事件回调
        rot.EventHandler = new SampleEventControl();
        return rot;
    }

    public IRotationUI? GetRotationUI()
    {
        return new SampleUI();
    }

    public void OnDrawSetting()
    {
        //一般不做
    }

    public void Dispose()
    {
        
    }

    public void OnEnterRotation()
    {
        //可以做一些初始化，一些命令注册
    }

    public void OnExitRotation()
    {
        //对资源的释放，hook释放等
    }

    public string AuthorName { get; } = "Sample";
    public bool UseCustomUi { get; } = false;
    public IEnumerable<Jobs> TargetJobs { get; } = [Jobs.None];
    public SampleSettings Settings { get; set; } = new SampleSettings();
}