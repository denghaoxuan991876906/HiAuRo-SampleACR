
namespace SampleACR.SampleJob;

public class SampleSettings : AcrSettings
{
    //这些设置宿主自动会通过接口保存到对应的json文件中，注意只有public且不是static的才行。
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
}