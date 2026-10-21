using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using CodeWF.Avalonia.Content.Tests.Markdown.Rendering;
using CodeWF.Avalonia.Controls.Animations;
using CodeWF.Avalonia.Controls.Controls;
using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Controls;

[Collection("AvaloniaPlatform")]
public sealed class ReusablePresentationTests(AvaloniaPlatformFixture platform)
{
    [Fact]
    public void EqualHeightRows_RespondToFilteringAndColumnChanges() => platform.Run(() =>
    {
        var panel = new EqualHeightWrapPanel { Columns = 2, ColumnSpacing = 12, RowSpacing = 8 };
        var shortCard = new Border { MinHeight = 20 };
        var tallCard = new Border { MinHeight = 70 };
        var nextRow = new Border { MinHeight = 30 };
        panel.Children.Add(shortCard); panel.Children.Add(tallCard); panel.Children.Add(nextRow);
        void Layout()
        {
            panel.Measure(new Size(212, double.PositiveInfinity));
            panel.Arrange(new Rect(panel.DesiredSize));
        }
        Layout();
        Assert.Equal(new Rect(0, 0, 100, 70), shortCard.Bounds);
        Assert.Equal(new Rect(112, 0, 100, 70), tallCard.Bounds);
        Assert.Equal(new Rect(0, 78, 100, 30), nextRow.Bounds);
        tallCard.IsVisible = false;
        Layout();
        Assert.Equal(new Rect(112, 0, 100, 30), nextRow.Bounds);
        panel.Columns = 1;
        Layout();
        Assert.Equal(new Rect(0, 28, 212, 30), nextRow.Bounds);
        panel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        Assert.True(double.IsFinite(panel.DesiredSize.Width));
    });

    [Fact]
    public Task Entrance_CancellationAndDetachRestoreTheConfiguredOpacity() => platform.RunAsync(async () =>
    {
        var card = new Border { Width = 120, Height = 80, Opacity = .65 };
        EntranceMotion.SetDuration(card, TimeSpan.FromMilliseconds(180));
        EntranceMotion.SetOffsetY(card, 9);
        EntranceMotion.SetInitialScale(card, .99);
        EntranceMotion.SetIsEnabled(card, true);
        var window = new Window { Content = card, Width = 240, Height = 160 };
        try
        {
            window.Show(); window.UpdateLayout();
            var bounds = card.Bounds;
            for (var frame = 0; frame < 4; frame++)
            {
                AvaloniaHeadlessPlatform.ForceRenderTimerTick();
                await Task.Delay(16);
            }
            Assert.InRange(card.Opacity, .001, .649);
            Assert.Equal(bounds, card.Bounds);
            EntranceMotion.SetAnimate(card, false);
            Assert.Equal(.65, card.Opacity);
            card.IsVisible = false; await Task.Delay(20);
            card.IsVisible = true; await Task.Delay(20);
            Assert.Equal(.65, card.Opacity);
            EntranceMotion.SetAnimate(card, true);
            card.IsVisible = false; await Task.Delay(20);
            card.IsVisible = true; await Task.Delay(40);
            window.Content = null;
            Assert.Equal(.65, card.Opacity);
            await Task.Delay(220);
            Assert.Equal(.65, card.Opacity);
        }
        finally { window.Close(); EntranceMotion.SetIsEnabled(card, false); }
    });

    [Fact]
    public void Watermark_IsAnIndependentInputTransparentLayer() => platform.Run(() =>
    {
        var document = new TextBox { Text = "Original document" };
        var watermark = new WatermarkOverlay { Text = "Confidential", Strength = 30, Angle = -30, Spacing = new Size(180, 120) };
        var grid = new Grid(); grid.Children.Add(document); grid.Children.Add(watermark);
        grid.Measure(new Size(500, 300)); grid.Arrange(new Rect(0, 0, 500, 300));
        Assert.False(watermark.IsHitTestVisible);
        Assert.True(watermark.ClipToBounds);
        Assert.Equal(document.Bounds, watermark.Bounds);
        watermark.Text = "Updated watermark";
        Assert.Equal("Original document", document.Text);
    });
}
