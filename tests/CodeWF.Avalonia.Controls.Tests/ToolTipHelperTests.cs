using Avalonia.Controls;
using Avalonia.Media;
using CodeWF.Avalonia.Controls.Helpers;
using Xunit;

namespace CodeWF.Avalonia.Controls.Tests;

/// <summary>
/// ToolTipHelper 统一文本提示封装的行为测试。
/// 契约：返回显式 ToolTip 实例（MaxWidth 以本地值写入，压过宿主主题的样式值），
/// 内容为折行 TextBlock——长路径不再被宿主主题的默认宽度硬裁。
/// </summary>
public sealed class ToolTipHelperTests
{
    [Fact]
    public void CreateTextTip_EmptyText_ReturnsNull()
    {
        Assert.Null(ToolTipHelper.CreateTextTip(null));
        Assert.Null(ToolTipHelper.CreateTextTip(string.Empty));
        Assert.Null(ToolTipHelper.CreateTextTip("   "));
    }

    [Fact]
    public void CreateTextTip_Text_ReturnsToolTipWithWrappedTextBlock()
    {
        var tip = Assert.IsType<ToolTip>(ToolTipHelper.CreateTextTip("提示内容"));
        var content = Assert.IsType<TextBlock>(tip.Content);
        Assert.Equal("提示内容", content.Text);
        Assert.Equal(TextWrapping.Wrap, content.TextWrapping);
        Assert.Equal(320, tip.MaxWidth);
    }

    [Fact]
    public void CreateTextTip_CustomMaxWidth_AppliesToToolTip()
    {
        var tip = Assert.IsType<ToolTip>(ToolTipHelper.CreateTextTip("提示", 480));
        Assert.Equal(480, tip.MaxWidth);
        var content = Assert.IsType<TextBlock>(tip.Content);
        // 评审修订（2026-11-10）：宿主主题模板（Ursa/Semi）不把 ToolTip.MaxWidth 传导给内容时，
        // 文本块需自带本地 MaxWidth 才能折行——断言随行为更新。
        Assert.Equal(480, content.MaxWidth);
        Assert.Equal(TextWrapping.Wrap, content.TextWrapping);
    }

    [Fact]
    public void SetText_AttachesToolTip()
    {
        var button = new Button();
        ToolTipHelper.SetText(button, "按钮提示");
        Assert.Equal("按钮提示", ToolTipHelper.GetText(button));
        var tip = Assert.IsType<ToolTip>(ToolTip.GetTip(button));
        Assert.Equal("按钮提示", Assert.IsType<TextBlock>(tip.Content).Text);
    }

    [Fact]
    public void SetText_Empty_RemovesToolTip()
    {
        var button = new Button();
        ToolTipHelper.SetText(button, "按钮提示");
        ToolTipHelper.SetText(button, null);
        Assert.Null(ToolTip.GetTip(button));
    }
}
