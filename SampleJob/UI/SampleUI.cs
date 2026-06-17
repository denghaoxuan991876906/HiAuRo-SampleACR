

namespace SampleACR.SampleJob.UI;

public class SampleUI:IRotationUI
{
    public void RegisterControls(IAcrUiBuilder builder)
    {
        //使用builder来绘制ACR的UI
        //qt以及hotkey也是使用builder添加
        //有几个所有职业通用的qt
        
        //通用的爆发qt
        builder.AddBuiltinQt(BuiltinQt.Burst, true);
        
        //独有的qt
        builder.AddQtToggle( "示例qt", true, "示例用的QT");
        
        //添加hotkey
        builder.AddQtHotkey("示例hotkey", new HotkeyResolver_技能("示例技能", 123, SpellTargetType.Target, "12"));
        
        //在悬浮窗中添加TAB
        builder.AddTab("示例tab");
    }
}
