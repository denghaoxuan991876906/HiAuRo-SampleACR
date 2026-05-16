using HiAuRo.ACR;

namespace SampleACR.SampleJob;

public class SampleEntry : IRotationEntry, ISettingsProvider<SampleSettings>
{
    public Rotation? Build(string settingFolder)
    {
        
        return new Rotation();
    }

    public IRotationUI? GetRotationUI()
    {
        throw new NotImplementedException();
    }

    public void OnDrawSetting()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public void OnEnterRotation()
    {
        throw new NotImplementedException();
    }

    public void OnExitRotation()
    {
        throw new NotImplementedException();
    }

    public string AuthorName { get; } = "Sample";
    public bool UseCustomUi { get; } = false;
    public IEnumerable<Jobs> TargetJobs { get; } = [Jobs.None];
    public SampleSettings Settings { get; set; } = new SampleSettings();
}