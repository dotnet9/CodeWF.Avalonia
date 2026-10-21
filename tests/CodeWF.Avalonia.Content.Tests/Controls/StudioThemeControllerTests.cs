using CodeWF.Avalonia.Controls.Themes;
using System.Reflection;
using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Media;
using global::Avalonia.Platform;
using CodeWF.Avalonia.Content.Tests.Markdown.Rendering;
using Semi.Avalonia;
using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Controls;

[Collection("AvaloniaPlatform")]
public sealed class StudioThemeControllerTests(AvaloniaPlatformFixture platform)
{
    [Fact]
    public void EveryPaletteHasIndependentLightAndDarkResources() => platform.Run(() =>
    {
        var application = Application.Current!;
        var previous = application.RequestedThemeVariant;
        var semi = new SemiTheme();
        application.Styles.Add(semi);
        try
        {
            using var controller = new StudioThemeController(application);
            var palettes = StudioPalettes.All;
            Assert.Equal(9, palettes.Count);
            foreach (var palette in palettes)
            {
                controller.PaletteKey = palette.Key;
                controller.Mode = StudioThemeMode.Light;
                Assert.Equal(StudioTheme.GetVariant(palette.Key, false), application.ActualThemeVariant);
                var light = ResourceColor("StudioBackground");
                controller.Mode = StudioThemeMode.Dark;
                Assert.Equal(StudioTheme.GetVariant(palette.Key, true), application.ActualThemeVariant);
                var dark = ResourceColor("StudioBackground");
                Assert.True(light.R + light.G + light.B > dark.R + dark.G + dark.B, palette.Key);
                Assert.NotEqual(ResourceColor("StudioText"), dark);
            }
        }
        finally
        {
            application.RequestedThemeVariant = previous;
            application.Styles.Remove(semi);
        }
    });

    [Fact]
    public void SystemModeFollowsPlatformChangesAndDisposeReleasesSubscription() => platform.Run(() =>
    {
        var application = Application.Current!;
        var settings = application.PlatformSettings!;
        var emit = typeof(DefaultPlatformSettings).GetMethod("OnColorValuesChanged", BindingFlags.Instance | BindingFlags.NonPublic)!;
        void SetTheme(PlatformThemeVariant theme) => emit.Invoke(settings, [new PlatformColorValues { ThemeVariant = theme }]);
        var previous = application.RequestedThemeVariant;
        try
        {
            using var controller = new StudioThemeController(application) { PaletteKey = "teal" };
            Assert.Equal(StudioTheme.GetVariant("teal", false), application.RequestedThemeVariant);
            SetTheme(PlatformThemeVariant.Dark);
            Assert.Equal(StudioTheme.GetVariant("teal", true), application.RequestedThemeVariant);
            controller.Mode = StudioThemeMode.Light;
            SetTheme(PlatformThemeVariant.Dark);
            Assert.Equal(StudioTheme.GetVariant("teal", false), application.RequestedThemeVariant);
            controller.Mode = StudioThemeMode.System;
            Assert.Equal(StudioTheme.GetVariant("teal", true), application.RequestedThemeVariant);
            controller.Dispose();
            SetTheme(PlatformThemeVariant.Light);
            Assert.Equal(StudioTheme.GetVariant("teal", true), application.RequestedThemeVariant);
        }
        finally { application.RequestedThemeVariant = previous; }
    });

    private static Color ResourceColor(string key)
    {
        var application = Application.Current!;
        Assert.True(application.TryFindResource(key, application.ActualThemeVariant, out var resource));
        return Assert.IsType<SolidColorBrush>(resource).Color;
    }

}
