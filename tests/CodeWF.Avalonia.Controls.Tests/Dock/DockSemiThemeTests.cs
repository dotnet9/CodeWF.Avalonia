using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Semi.Avalonia;
using System.Globalization;
using CodeWF.Avalonia.Dock.Themes;
using Xunit;

namespace CodeWF.Avalonia.Controls.Tests.Dock;

public sealed class DockSemiThemeTests
{
    static DockSemiThemeTests()
    {
        AppBuilder.Configure<Application>()
            .UsePlatformDetect()
            .AfterSetup(_ => Application.Current!.Styles.Add(new SemiTheme()))
            .SetupWithoutStarting();
    }

    [Fact]
    public void UnsupportedLocaleFallsBackToEnglish()
    {
        var theme = new DockSemiTheme
        {
            Locale = new CultureInfo("fr-FR")
        };

        Assert.Equal("en-US", theme.Locale?.Name);
    }

    [Fact]
    public void SupportedLocaleIsPreserved()
    {
        var theme = new DockSemiTheme
        {
            Locale = new CultureInfo("zh-CN")
        };

        Assert.Equal("zh-CN", theme.Locale?.Name);
    }

    [Fact]
    public void NullLocaleFallsBackToEnglish()
    {
        var theme = new DockSemiTheme
        {
            Locale = null
        };

        Assert.Equal("en-US", theme.Locale?.Name);
    }

    [Fact]
    public void ButtonFeedbackRetainsTheFullSemiButtonSurface()
    {
        var theme = new DockSemiTheme();
        Assert.True(theme.TryGetResource(typeof(Button), ThemeVariant.Light, out var resource));
        var button = new Button
        {
            Theme = Assert.IsType<ControlTheme>(resource), Width = 95, Height = 80, Content = "Demo"
        };
        button.ApplyTemplate();
        button.Measure(new Size(95, 80));
        button.Arrange(new Rect(0, 0, 95, 80));
        var surface = Assert.IsType<ContentPresenter>(Assert.Single(button.GetVisualChildren()));
        Assert.Equal(button.Bounds.Size, surface.Bounds.Size);
        Assert.True(theme.TryGetResource(typeof(Window), ThemeVariant.Light, out var windowTheme));
        Assert.Equal(typeof(Window), Assert.IsType<ControlTheme>(windowTheme).TargetType);
    }
}
