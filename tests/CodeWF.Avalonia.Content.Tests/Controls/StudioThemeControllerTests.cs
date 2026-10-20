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
        var studio = new StudioTheme();
        application.Styles.Add(semi);
        application.Styles.Add(studio);
        try
        {
            using var controller = new StudioThemeController(application);
            var palettes = Enum.GetValues<StudioPalette>();
            Assert.Equal(16, palettes.Length);
            foreach (var palette in palettes)
            {
                controller.Palette = palette;
                controller.Mode = StudioThemeMode.Light;
                Assert.Equal(StudioTheme.GetVariant(palette, false), application.ActualThemeVariant);
                var light = ResourceColor("StudioBackground");
                controller.Mode = StudioThemeMode.Dark;
                Assert.Equal(StudioTheme.GetVariant(palette, true), application.ActualThemeVariant);
                var dark = ResourceColor("StudioBackground");
                Assert.True(light.R + light.G + light.B > dark.R + dark.G + dark.B, palette.ToString());
                Assert.NotEqual(ResourceColor("StudioText"), dark);
            }
        }
        finally
        {
            application.RequestedThemeVariant = previous;
            application.Styles.Remove(studio);
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
            using var controller = new StudioThemeController(application) { Palette = StudioPalette.Aquatic };
            Assert.Equal(StudioTheme.AquaticLight, application.RequestedThemeVariant);
            SetTheme(PlatformThemeVariant.Dark);
            Assert.Equal(StudioTheme.AquaticDark, application.RequestedThemeVariant);
            controller.Mode = StudioThemeMode.Light;
            SetTheme(PlatformThemeVariant.Dark);
            Assert.Equal(StudioTheme.AquaticLight, application.RequestedThemeVariant);
            controller.Mode = StudioThemeMode.System;
            Assert.Equal(StudioTheme.AquaticDark, application.RequestedThemeVariant);
            controller.Dispose();
            SetTheme(PlatformThemeVariant.Light);
            Assert.Equal(StudioTheme.AquaticDark, application.RequestedThemeVariant);
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
