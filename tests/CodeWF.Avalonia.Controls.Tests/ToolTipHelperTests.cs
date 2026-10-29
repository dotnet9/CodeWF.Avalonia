using Avalonia.Controls;
using Avalonia.Media;
using CodeWF.Avalonia.Controls.Helpers;
using Xunit;

namespace CodeWF.Avalonia.Controls.Tests;

/// <summary>
/// ToolTipHelper 统一文本提示封装的行为测试。
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
    public void CreateTextTip_Text_ReturnsWrappedTextBlock()
    {
        var content = Assert.IsType<TextBlock>(ToolTipHelper.CreateTextTip("提示内容"));
        Assert.Equal("提示内容", content.Text);
        Assert.Equal(TextWrapping.Wrap, content.TextWrapping);
        Assert.Equal(320, content.MaxWidth);
    }

    [Fact]
    public void CreateTextTip_CustomMaxWidth_AppliesToTextBlock()
    {
        var content = Assert.IsType<TextBlock>(ToolTipHelper.CreateTextTip("提示", 180));
        Assert.Equal(180, content.MaxWidth);
    }

    [Fact]
    public void SetText_AttachesToolTip()
    {
        var button = new Button();
        ToolTipHelper.SetText(button, "按钮提示");
        Assert.Equal("按钮提示", ToolTipHelper.GetText(button));
        Assert.Equal("按钮提示", Assert.IsType<TextBlock>(ToolTip.GetTip(button)).Text);
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
