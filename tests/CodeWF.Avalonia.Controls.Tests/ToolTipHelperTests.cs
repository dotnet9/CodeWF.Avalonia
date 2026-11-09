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
        Assert.True(double.IsPositiveInfinity(content.MaxWidth), "折行交给 ToolTip 根约束，文本块不再自限宽度");
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
