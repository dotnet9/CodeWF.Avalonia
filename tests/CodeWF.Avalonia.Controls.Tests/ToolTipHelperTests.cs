using Avalonia.Controls;
using Avalonia.Media;
using CodeWF.Avalonia.Controls.Helpers;
using Xunit;

namespace CodeWF.Avalonia.Controls.Tests;

/// <summary>
/// ToolTipHelper 统一文本提示封装的行为测试。
/// 契约：返回显式 ToolTip 实例（MaxWidth 以本地值写入，压过宿主主题的样式值），
/// 内容为确定性预折行的 TextBlock——长文案生成前按宽度启发式插入 '\n'，
/// 不依赖弹窗首测 re-wrap（该竞态曾导致同一长路径有时单行硬裁、有时只显示盘符）；
/// TextBlock.Width 同时钉成本地值（各行估宽最大值 + 余量，clamp 到 maxWidth），
/// 内容宽度不再依赖宿主主题（Semi/Ursa 覆盖原生 ToolTip 样式）的测量时机；
/// 高度是同一竞态的另一半——TextBlock.Height 按行数 × 保守行高（17px）+ 余量钉死，
/// ToolTip.Height = 内容高 + 垂直 Padding 余量（≥ 内容高 + 14），两行文本不再只剩单行。
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
        // Width 钉成本地值：弹窗宽度首测即终态，不依赖宿主主题的测量/挂树时机。
        AssertWidthPinnedWithinMaxWidth(content, 320);
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
        AssertWidthPinnedWithinMaxWidth(content, 480);
    }

    [Fact]
    public void CreateTextTip_Width_IsNonNegativeLocalValueWithinMaxWidth()
    {
        // Width 必须是非空本地值（未设置时 Avalonia 为 NaN）且不超 maxWidth、不小于最宽行估宽。
        var tip = Assert.IsType<ToolTip>(ToolTipHelper.CreateTextTip("简短提示"));
        var content = Assert.IsType<TextBlock>(tip.Content);
        AssertWidthPinnedWithinMaxWidth(content, 320);
        // 未触 clamp 时 Width = 最宽行估宽 + 16 余量（口径与 MeasureTipWidth 一致）。
        Assert.Equal(ToolTipHelper.MeasureTextWidth("简短提示") + 16, content.Width);
    }

    [Fact]
    public void CreateTextTip_LongPath_PrewrapsWithinBudget()
    {
        const string path = @"E:\github\Apps\Vex\docs\演示文档\数字与化学.md";
        var tip = Assert.IsType<ToolTip>(ToolTipHelper.CreateTextTip(path));
        var content = Assert.IsType<TextBlock>(tip.Content);
        Assert.NotNull(content.Text);
        // 确定性预折行：长路径在生成提示前已插入 '\n'，不再依赖运行时 re-wrap 时机。
        Assert.Contains('\n', content.Text!);
        AssertLinesWithinBudget(content.Text, 320);
        // 只插 '\n' 不丢字符：合并后与原文一致（扩展名等不会被截掉）。
        Assert.Equal(path, content.Text!.Replace("\n", string.Empty));
    }

    [Fact]
    public void CreateTextTip_LongPath_TwoLines_WidthIsReasonable()
    {
        // 两行长路径：Width 钉在「最宽行估宽 + 余量」且不超 maxWidth——
        // 弹窗宽度确定、放得下最宽行，又不浪费整行空白。
        const string path = @"E:\github\Apps\Vex\docs\演示文档\数字与化学.md";
        var tip = Assert.IsType<ToolTip>(ToolTipHelper.CreateTextTip(path));
        var content = Assert.IsType<TextBlock>(tip.Content);
        var lines = content.Text!.Split('\n');
        Assert.True(lines.Length >= 2, $"长路径应预折成至少两行，实际 {lines.Length} 行");
        AssertWidthPinnedWithinMaxWidth(content, 320);
        // 高度与宽度是同一竞态的另一半：两行文本 Height 必须是本地多行值，
        // 否则弹窗首测高度被记成单行，第二行被窗口裁掉（用户截图只见预折行第一行）。
        Assert.True(content.Height >= 2 * 17,
            $"两行文本 TextBlock.Height 应 ≥ 34（保守行高 17px/行），实际 {content.Height}");
        Assert.False(double.IsNaN(tip.Height), "ToolTip.Height 应为本地值而非未设置（NaN）");
        Assert.True(tip.Height >= content.Height + 14,
            $"ToolTip.Height 应覆盖内容高 + 垂直 Padding（14）：{tip.Height} < {content.Height + 14}");
    }

    [Fact]
    public void CreateTextTip_TwoLines_HeightIsPinnedToLocalValue()
    {
        // 显式含 '\n' 的两行文案：Height 按行数 × 保守行高 + 余量钉死，首测即终态。
        const string twoLines = "第一行提示\n第二行提示";
        var tip = Assert.IsType<ToolTip>(ToolTipHelper.CreateTextTip(twoLines));
        var content = Assert.IsType<TextBlock>(tip.Content);
        Assert.Equal(2, content.Text!.Split('\n').Length);
        // 精确口径：TextBlock.Height = 行数 × 17 + 4；ToolTip.Height = 内容高 + 垂直余量 20。
        Assert.Equal(2 * 17 + 4, content.Height);
        Assert.Equal(content.Height + 20, tip.Height);
    }

    [Fact]
    public void CreateTextTip_SingleLine_HeightIsReasonable()
    {
        // 单行短提示：Height = 1 × 17 + 4 = 21，略高于实际行高（宁可略高不裁字）；
        // ToolTip.Height 同步钉死，不依赖弹窗首测。
        var tip = Assert.IsType<ToolTip>(ToolTipHelper.CreateTextTip("简短提示"));
        var content = Assert.IsType<TextBlock>(tip.Content);
        Assert.Single(content.Text!.Split('\n'));
        Assert.False(double.IsNaN(content.Height), "TextBlock.Height 应为本地值而非未设置（NaN）");
        Assert.Equal(17 + 4, content.Height);
        Assert.Equal(content.Height + 20, tip.Height);
    }

    [Fact]
    public void CreateTextTip_LongAsciiPath_ForceBreaksSingleTokenWithinBudget()
    {
        var path = @"E:\github\Apps\Vex\docs\" + new string('n', 120) + ".md";
        var tip = Assert.IsType<ToolTip>(ToolTipHelper.CreateTextTip(path));
        var content = Assert.IsType<TextBlock>(tip.Content);
        Assert.NotNull(content.Text);
        // 无分隔符的超长 ASCII 段按字符强断，同样保证每行不超预算。
        Assert.Contains('\n', content.Text!);
        AssertLinesWithinBudget(content.Text, 320);
        Assert.Equal(path, content.Text!.Replace("\n", string.Empty));
    }

    [Fact]
    public void WrapText_ShortText_ReturnsUnchanged()
    {
        Assert.Equal("提示内容", ToolTipHelper.WrapText("提示内容", 320));
        Assert.Equal(@"E:\github\Apps\Vex\readme.md", ToolTipHelper.WrapText(@"E:\github\Apps\Vex\readme.md", 320));
    }

    [Fact]
    public void WrapText_PreservesExistingLineBreaks()
    {
        const string multiLine = "第一行\n第二行";
        Assert.Equal(multiLine, ToolTipHelper.WrapText(multiLine, 320));
    }

    [Fact]
    public void WrapText_LargeMaxWidth_ReturnsUnchanged()
    {
        const string path = @"E:\github\Apps\Vex\docs\演示文档\数字与化学.md";
        Assert.Equal(path, ToolTipHelper.WrapText(path, 2000));
    }

    /// <summary>断言每行按宽度启发式估算不超「最大宽度 - 安全余量」（与 ToolTipHelper 内部预算口径一致）。</summary>
    private static void AssertLinesWithinBudget(string text, double maxWidth)
    {
        foreach (var line in text.Split('\n'))
        {
            var width = ToolTipHelper.MeasureTextWidth(line);
            Assert.True(width <= maxWidth - 40,
                $"行宽超预算（估算 {width:F1}px > {maxWidth - 40:F0}px）：{line}");
        }
    }

    /// <summary>
    /// 断言 TextBlock.Width 是钉死的本地值：非 NaN（未设置）、非负、≤ maxWidth、≥ 最宽行估宽——
    /// 保证内容宽度不依赖宿主主题模板的测量时机，弹窗首测即终态。
    /// </summary>
    private static void AssertWidthPinnedWithinMaxWidth(TextBlock content, double maxWidth)
    {
        Assert.False(double.IsNaN(content.Width), "Width 应为本地值而非未设置（NaN）");
        Assert.True(content.Width > 0, $"Width 应为正的本地值，实际 {content.Width}");
        Assert.True(content.Width <= maxWidth, $"Width 不应超过 maxWidth：{content.Width} > {maxWidth}");
        var widest = MeasureWidestLine(content.Text!);
        Assert.True(content.Width >= widest,
            $"Width 应不小于最宽行估宽：{content.Width:F1} < {widest:F1}");
    }

    /// <summary>取各行的宽度启发式估宽最大值（与 ToolTipHelper.MeasureTipWidth 口径一致）。</summary>
    private static double MeasureWidestLine(string text)
    {
        var widest = 0d;
        foreach (var line in text.Split('\n')) widest = Math.Max(widest, ToolTipHelper.MeasureTextWidth(line));
        return widest;
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
