using Avalonia.Controls;
using CodeWF.Avalonia.Content.Tests.Markdown.Rendering;
using CodeWF.Avalonia.Controls.Controls;
using CodeWF.Avalonia.Controls.Models;
using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Controls;

[Collection("AvaloniaPlatform")]
public sealed class ContentExpansionAnimatorTests(AvaloniaPlatformFixture platform)
{
    [Fact]
    public void InitialStateAndDetachedChanges_ReachTheRequestedSize() => platform.Run(() =>
    {
        var control = new ContentExpansionAnimator { ExpandedSize = 240, CollapsedSize = 48 };
        Assert.Equal(1, control.Progress);
        Assert.Equal(240, control.CurrentSize);
        control.IsExpanded = false;
        Assert.Equal(0, control.Progress);
        Assert.Equal(48, control.CurrentSize);
        control.IsExpanded = true;
        Assert.Equal(1, control.Progress);
    });

    [Theory]
    [InlineData(ExpansionOrientation.Horizontal)]
    [InlineData(ExpansionOrientation.Vertical)]
    public Task Animation_CollapsesExpandsAndReversesWithoutStaleCompletion(ExpansionOrientation orientation) =>
        platform.RunAsync(async () =>
        {
            var control = new ContentExpansionAnimator
            {
                Orientation = orientation, Duration = TimeSpan.FromMilliseconds(160),
                Child = new TextBlock { Text = "Visible content" }
            };
            var window = new Window { Width = 500, Height = 500, Content = control };
            try
            {
                window.Show();
                window.UpdateLayout();
                Assert.Equal(1, control.Progress);
                Assert.True(control.Bounds.Width > 0 && control.Bounds.Height > 0);
                control.IsExpanded = false;
                await WaitForProgress(control, 0);
                control.IsExpanded = true;
                await WaitForProgress(control, 1);
                control.IsExpanded = false;
                await Task.Delay(70);
                Assert.InRange(control.Progress, 0.001, 0.999);
                var beforeReverse = control.Progress;
                control.IsExpanded = true;
                Assert.InRange(Math.Abs(control.Progress - beforeReverse), 0, 0.001);
                await WaitForProgress(control, 1);
                await Task.Delay(200);
                Assert.Equal(1, control.Progress);
                control.IsExpanded = false;
                control.Animate = false;
                Assert.Equal(0, control.Progress);
                control.Animate = true;
                control.IsExpanded = true;
                window.Content = null;
                Assert.Equal(1, control.Progress);
                await Task.Delay(200);
                Assert.Equal(1, control.Progress);
            }
            finally { window.Close(); }
        });

    private static async Task WaitForProgress(ContentExpansionAnimator control, double expected)
    {
        for (var attempt = 0; attempt < 40 && control.Progress != expected; attempt++)
            await Task.Delay(25);
        Assert.Equal(expected, control.Progress);
    }
}
