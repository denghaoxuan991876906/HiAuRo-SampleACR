using HiAuRo.ACR;

namespace SampleACR.SampleJob;

public class SampleEntry : IRotationEntry
{
    public Rotation? Build(string settingFolder)
    {
        throw new NotImplementedException();
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
}