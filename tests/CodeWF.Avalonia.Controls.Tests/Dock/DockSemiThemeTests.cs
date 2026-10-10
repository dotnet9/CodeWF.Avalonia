using Avalonia;
using global::Avalonia.Themes.Fluent;
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
            .AfterSetup(_ => Application.Current!.Styles.Add(new FluentTheme()))
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
}
