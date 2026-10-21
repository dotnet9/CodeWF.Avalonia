using CodeWF.Avalonia.Markdown;
using global::Avalonia.Controls;
using global::Avalonia.Media;
using global::Avalonia.Styling;

using CodeWF.Avalonia.Markdown.Controls;
using CodeWF.Avalonia.Markdown.Themes;

using CodeWF.Avalonia.Content.Tests.Markdown.Rendering;

using Xunit;


namespace CodeWF.Avalonia.Content.Tests.Markdown.Themes;

[Collection("AvaloniaPlatform")]
public sealed class MarkdownThemesResourceTests
{
	private readonly AvaloniaPlatformFixture _platform;

	public MarkdownThemesResourceTests(AvaloniaPlatformFixture platform) => _platform = platform;

	[Fact]
	public void MarkdownTypographyThemes_DoesNotExposeThemeList() => _platform.Run(() =>
	{
		EnsureThemesLoaded();
		Assert.Null(typeof(MarkdownTypographyThemes).GetProperty("All"));
	});

	[Fact]
	public void OverrideTypographyResources_WhenViewerOverridesOnlySize_DoesNotReuseInheritedDictionary() => _platform.Run(() =>
	{
		EnsureThemesLoaded();
		var parent = new Border();
		var viewer = new MarkdownViewer();
		parent.Child = viewer;

		var exception = Record.Exception(() =>
		{
			MarkdownThemes.OverrideTypographyResources(parent, MarkdownTypographyThemes.Basic, MarkdownTypographySizes.Compact);
			viewer.TypographySize = MarkdownTypographySizes.Normal;
			MarkdownThemes.OverrideTypographyResources(parent, MarkdownTypographyThemes.Simple, MarkdownTypographySizes.Compact);
		});

		Assert.Null(exception);
		Assert.NotEmpty(viewer.Resources.MergedDictionaries);
	});

	[Fact]
	public void OverrideTypographyResources_WhenAppliedToContainer_DoesNotExposeSemiPaletteKeys() => _platform.Run(() =>
	{
		EnsureThemesLoaded();
		var parent = new Border();

		MarkdownThemes.OverrideTypographyResources(parent, MarkdownTypographyThemes.GeekBlack, MarkdownTypographySizes.Normal);

		Assert.True(parent.Resources.TryGetResource(MarkdownStyleKeys.TypographyBaseResourcesResource, ThemeVariant.Light, out _));
		Assert.True(parent.Resources.TryGetResource(MarkdownStyleKeys.ParagraphFontSizeResource, ThemeVariant.Light, out _));
		Assert.False(parent.Resources.TryGetResource(MarkdownStyleKeys.AccentBrushResource, ThemeVariant.Light, out _));
		Assert.False(parent.Resources.TryGetResource(MarkdownStyleKeys.QuoteBackgroundBrushResource, ThemeVariant.Light, out _));
	});

	[Fact]
	public void OverrideTypographyResources_WhenAppliedToContainer_KeepsPaletteOnViewerOnly() => _platform.Run(() =>
	{
		EnsureThemesLoaded();
		var parent = new Border();
		var viewer = new MarkdownViewer();
		parent.Child = viewer;

		MarkdownThemes.OverrideTypographyResources(parent, MarkdownTypographyThemes.GeekBlack, MarkdownTypographySizes.Normal);

		Assert.False(parent.Resources.TryGetResource(MarkdownStyleKeys.AccentBrushResource, ThemeVariant.Light, out _));
		Assert.True(viewer.Resources.TryGetResource(MarkdownStyleKeys.AccentBrushResource, ThemeVariant.Light, out var accentBrush));
		Assert.IsAssignableFrom<ISolidColorBrush>(accentBrush);
	});

	[Theory]
	[InlineData(MarkdownTypographyThemes.Simple, "#343B43", "#F6F7F8", "#F6F7F8", "#F3F3F4")]
	[InlineData(MarkdownTypographyThemes.OrangeHeart, "#874742", "#F6F7F8", "#F6F7F8", "#FEF6F5")]
	public void CreateExportStyle_WhenThemeUsesCommonResources_ResolvesIncludedBaseResources(
		string themeName,
		string accentColor,
		string codeBackgroundColor,
		string inlineCodeBackgroundColor,
		string quoteBackgroundColor) => _platform.Run(() =>
	{
		EnsureThemesLoaded();
		var style = MarkdownThemes.CreateExportStyle(themeName);

		Assert.Equal(accentColor, style.LinkColor);
		Assert.Equal(codeBackgroundColor, style.CodeBackgroundColor);
		Assert.Equal(inlineCodeBackgroundColor, style.InlineCodeBackgroundColor);
		Assert.Equal(quoteBackgroundColor, style.QuoteBackgroundColor);
	});

	[Fact]
	public void CreateExportStyle_WhenWeChatFormatTheme_ResolvesDocumentPalette() => _platform.Run(() =>
	{
		EnsureThemesLoaded();
		var style = MarkdownThemes.CreateExportStyle(MarkdownTypographyThemes.WeChatFormat);

		Assert.Contains(MarkdownTypographyThemes.WeChatFormat, MarkdownTypographyThemeRegistry.ThemeNames);
		Assert.Equal("#30343B", style.BodyColor);
		Assert.Equal(Color.Parse(ArticleTypographyCatalog.Find("WeChatFormat")!.LightText), Color.Parse(style.LinkColor));
		Assert.Equal(2d, style.LineHeightRatio);
	});

	[Fact]
	public void CreateExportStyle_WhenCustomThemeRegistered_UsesThemeResources() => _platform.Run(() =>
	{
		EnsureThemesLoaded();
		var themeName = $"UnitTestTheme{Guid.NewGuid():N}";
		MarkdownTypographyThemeRegistry.Register(
			themeName,
			() => new ResourceDictionary
			{
				[MarkdownStyleKeys.TextBrushResource] = new SolidColorBrush(Color.Parse("#123456")),
				[MarkdownStyleKeys.AccentBrushResource] = new SolidColorBrush(Color.Parse("#0E88EB")),
				[MarkdownStyleKeys.ParagraphFontSizeResource] = 18d,
				[MarkdownStyleKeys.ParagraphLineHeightResource] = 30d,
				[MarkdownStyleKeys.Heading1FontSizeResource] = 36d,
				[MarkdownStyleKeys.CodeBlockFontSizeResource] = 14d
			});

		var style = MarkdownThemes.CreateExportStyle(themeName);

		Assert.Contains(themeName, MarkdownTypographyThemeRegistry.ThemeNames);
		Assert.Equal(18d, style.BodyFontSize);
		Assert.Equal(36d, style.Heading1FontSize);
		Assert.Equal(14d, style.CodeFontSize);
		Assert.Equal(1.667d, style.LineHeightRatio);
		Assert.Equal("#123456", style.BodyColor);
		Assert.Equal("#0E88EB", style.LinkColor);
	});

	/// <summary>
	/// 主题入口的静态构造会加载并注册全部排版主题；必须在平台线程上完成，否则跨线程访问资源会抛异常。
	/// </summary>
	private static void EnsureThemesLoaded()
	{
		_ = ThemesLoaded.Value;
	}

	private static readonly Lazy<MarkdownThemes> ThemesLoaded = new(() => new MarkdownThemes());
}
