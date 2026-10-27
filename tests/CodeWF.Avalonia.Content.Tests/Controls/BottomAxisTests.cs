using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using CodeWF.Avalonia.Controls.Controls;
using CodeWF.Avalonia.Content.Tests.Markdown.Rendering;
using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Controls;

/// <summary>
/// 底部悬浮件同轴：左右折叠钮、居中胶囊、右下角缩放坞的中心线要落在同一条水平线上。
/// CSS 靠 bottom + translateY(50%) 表达，Avalonia 只能换算成外边距——所以真正的风险是
/// 「自身高度变了却没重算」：切密度、换语言、字号变化都会把中心线推离基线。
/// </summary>
[Collection("AvaloniaPlatform")]
public sealed class BottomAxisTests(AvaloniaPlatformFixture platform)
{
    [Fact]
    public void AxisConvertsCentreDistanceIntoBottomMargin() => platform.Run(() =>
    {
        var (window, floating) = Host(24, new Thickness(6, 7, 8, 99));
        BottomAxis.SetCenterFromBottom(floating, 30);
        // 30 - 24/2 = 18；其余三边是宿主自己的定位，只动底边。
        Assert.Equal(new Thickness(6, 7, 8, 18), floating.Margin);
        window.Close();
    });

    [Fact]
    public void HeightChangeRecomputesTheSameCentreLine() => platform.Run(() =>
    {
        var (window, floating) = Host(24);
        BottomAxis.SetCenterFromBottom(floating, 30);
        Assert.Equal(18, floating.Margin.Bottom);

        // 密度变一档：件自己长高，中心线不能跟着走。
        floating.Height = 40;
        window.UpdateLayout();
        Assert.Equal(10, floating.Margin.Bottom);
        window.Close();
    });

    [Fact]
    public void SwitchingAxisReplacesTheOldListener() => platform.Run(() =>
    {
        var (window, floating) = Host(20);
        BottomAxis.SetCenterFromBottom(floating, 30);
        Assert.Equal(20, floating.Margin.Bottom);
        // 从外层基线切到内层基线：旧监听若没退订，两条基线会互相覆盖成谁后写谁。
        BottomAxis.SetCenterFromBottom(floating, 24);
        Assert.Equal(14, floating.Margin.Bottom);
        floating.Height = 30;
        window.UpdateLayout();
        Assert.Equal(9, floating.Margin.Bottom);
        window.Close();
    });

    [Fact]
    public void NaNReleasesTheControlFromTheAxis() => platform.Run(() =>
    {
        var (window, floating) = Host(20);
        BottomAxis.SetCenterFromBottom(floating, 30);
        Assert.Equal(20, floating.Margin.Bottom);
        BottomAxis.SetCenterFromBottom(floating, double.NaN);
        // 解除跟踪后宿主自己写的边距不再被改动。
        floating.Margin = new Thickness(0, 0, 0, 5);
        floating.Height = 40;
        window.UpdateLayout();
        Assert.Equal(5, floating.Margin.Bottom);
        window.Close();
    });

    [Fact]
    public void RecomputeThatLandsOnTheSameValueDoesNotRewriteTheMargin() => platform.Run(() =>
    {
        // Bounds←Margin←Bounds 是一条会自激的回路：底边算出来还是原来那个数时就必须住手，
        // 否则每次布局都再写一遍边距，永远排不完。
        var (window, floating) = Host(20, new Thickness(3, 4, 5, 99));
        BottomAxis.SetCenterFromBottom(floating, 30);
        Assert.Equal(new Thickness(3, 4, 5, 20), floating.Margin);
        var applied = floating.Margin;

        floating.Width = 90;
        window.UpdateLayout();
        Assert.Equal(applied, floating.Margin);
        Assert.Equal(20, floating.Bounds.Height);
        window.Close();
    });

    private (Window Window, Border Floating) Host(double height, Thickness margin = default)
    {
        var floating = new Border { Height = height, Width = 40, Margin = margin };
        var panel = new StackPanel();
        panel.Children.Add(floating);
        var window = new Window { Width = 200, Height = 200, Content = panel };
        window.Show();
        window.UpdateLayout();
        return (window, floating);
    }
}
