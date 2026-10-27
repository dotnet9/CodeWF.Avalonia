using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using CodeWF.Avalonia.Controls.Controls;
using CodeWF.Avalonia.Content.Tests.Markdown.Rendering;
using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Controls;

/// <summary>
/// 分段面板的几何契约：轨道宽度由内容决定，指示块始终贴着选中段。
/// 这两条在窄窗口里最容易破——面板被压缩时若不等比让位，指示块停在旧位置，看上去就是
/// 「选中 A 却高亮 B」；末段选中时若把指示块自己的 Margin 算进 DesiredSize，轨道又会被撑宽一节。
/// </summary>
[Collection("AvaloniaPlatform")]
public sealed class SegmentedPanelTests(AvaloniaPlatformFixture platform)
{
    [Fact]
    public void TrackWidthIsTheSumOfTheItemsPlusSpacing() => platform.Run(() =>
    {
        var panel = new SegmentedPanel { Spacing = 4 };
        Add(panel, 40, 40, 40);
        panel.Measure(new Size(400, 20));
        Assert.Equal(128, panel.DesiredSize.Width);
        Assert.Equal(20, panel.DesiredSize.Height);
    });

    [Fact]
    public void HiddenItemLeavesNoSlotInTheTrack() => platform.Run(() =>
    {
        var panel = new SegmentedPanel { Spacing = 0 };
        Add(panel, 80, 30);
        ((Control)panel.Children[1]).IsVisible = false;
        panel.Measure(new Size(200, 20));
        Assert.Equal(30, panel.DesiredSize.Width);
    });

    [Fact]
    public void ThumbSitsExactlyOverTheActiveItem() => platform.Run(() =>
    {
        var panel = new SegmentedPanel { Spacing = 2 };
        var items = Add(panel, 30, 50, 20);
        var window = Show(panel, 200);
        SegmentedPanel.SetIsActive(items[1], true);

        // 指示块是 Children[0]，主题按它的 Width/Margin 画底色。
        var thumb = (Border)panel.Children[0];
        Assert.Equal(items[1].Bounds.X, thumb.Margin.Left);
        Assert.Equal(items[1].Bounds.Width, thumb.Width);
        Assert.Equal(1, thumb.Opacity);
        window.Close();
    });

    [Fact]
    public void SelectingTheLastItemDoesNotWidenTheTrack() => platform.Run(() =>
    {
        var panel = new SegmentedPanel { Spacing = 0 };
        var items = Add(panel, 30, 30);
        var window = Show(panel, 120);
        SegmentedPanel.SetIsActive(items[^1], true);
        // 指示块自己的 Width/Margin 不算进轨道：末段选中后再量一次，宽度仍是两段之和。
        panel.Measure(new Size(300, 20));
        Assert.Equal(60, panel.DesiredSize.Width);
        Assert.Equal(30, ((Border)panel.Children[0]).Width);
        window.Close();
    });

    [Fact]
    public void CompressedTrackShrinksEveryItemSoTheThumbStaysAttached() => platform.Run(() =>
    {
        var panel = new SegmentedPanel { Spacing = 0 };
        var items = Add(panel, 60, 60);
        var window = Show(panel, 50);
        SegmentedPanel.SetIsActive(items[^1], true);

        Assert.True(items[^1].Bounds.Width < 60, "可用宽度不足时段要等比让位，否则指示块会跑出选中段。");
        var thumb = (Border)panel.Children[0];
        Assert.Equal(items[^1].Bounds.X, thumb.Margin.Left);
        Assert.Equal(items[^1].Bounds.Width, thumb.Width);
        window.Close();
    });

    [Fact]
    public void NoActiveItemFadesTheThumbOut() => platform.Run(() =>
    {
        var panel = new SegmentedPanel { Spacing = 0 };
        var items = Add(panel, 30, 30);
        var window = Show(panel, 120);
        SegmentedPanel.SetIsActive(items[0], true);
        Assert.Equal(1, ((Border)panel.Children[0]).Opacity);
        // 切到「谁都没选中」（宿主清空选择）时指示块淡出，不能留在第一段上。
        SegmentedPanel.SetIsActive(items[0], false);
        Assert.Equal(0, ((Border)panel.Children[0]).Opacity);
        window.Close();
    });

    [Fact]
    public void InvisibleActiveItemDoesNotLightTheThumb() => platform.Run(() =>
    {
        var panel = new SegmentedPanel { Spacing = 0 };
        var items = Add(panel, 80, 30);
        var window = Show(panel, 120);
        items[0].IsVisible = false;
        SegmentedPanel.SetIsActive(items[0], true);
        Assert.Equal(0, ((Border)panel.Children[0]).Opacity);
        window.Close();
    });

    [Fact]
    public void FillItemsGivesEverySegmentTheSameShareOfTheTrack() => platform.Run(() =>
    {
        var panel = new SegmentedPanel { Spacing = 0, FillItems = true };
        var items = Add(panel, 30, 50);
        panel.Measure(new Size(200, 20));
        Assert.Equal(200, panel.DesiredSize.Width);
        var window = Show(panel, 200);
        Assert.Equal(100, items[0].Bounds.Width);
        Assert.Equal(100, items[1].Bounds.Width);
        window.Close();
    });

    [Fact]
    public void CornerRadiusReachesTheThumb() => platform.Run(() =>
    {
        var panel = new SegmentedPanel();
        panel.CornerRadius = new CornerRadius(4);
        Assert.Equal(new CornerRadius(4), ((Border)panel.Children[0]).CornerRadius);
        panel.CornerRadius = new CornerRadius(0);
        Assert.Equal(new CornerRadius(0), ((Border)panel.Children[0]).CornerRadius);
    });

    [Fact]
    public void IsActiveDrivesTheClassTheThemePaintsFrom() => platform.Run(() =>
    {
        var panel = new SegmentedPanel();
        var item = new Border { MinWidth = 30, MinHeight = 20 };
        panel.Children.Add(item);
        SegmentedPanel.SetIsActive(item, true);
        Assert.Contains("active", item.Classes);
        SegmentedPanel.SetIsActive(item, false);
        Assert.DoesNotContain("active", item.Classes);
    });

    private static List<Control> Add(SegmentedPanel panel, params double[] widths)
    {
        var items = new List<Control>();
        foreach (var width in widths)
        {
            // 宽度只由内容给出（不是 MinWidth）：MinWidth 会在 Arrange 时再把段撑回原宽，
            // 窄窗口里就看不出「等比让位」到底有没有发生。
            var item = new Border { Height = 20, Child = new Border { Width = width } };
            items.Add(item);
            panel.Children.Add(item);
        }
        return items;
    }

    private static Window Show(SegmentedPanel panel, double width)
    {
        var window = new Window { Width = width, Height = 60, Content = panel };
        window.Show();
        window.UpdateLayout();
        return window;
    }
}
