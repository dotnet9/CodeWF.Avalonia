using DemoResources = global::CodeWF.Avalonia.Demo.I18n.ResourceKeys;
using L = global::CodeWF.Avalonia.Lang.I18nManager;
using CodeWF.Avalonia.Markdown.Controls;
using CodeWF.Avalonia.Markdown.Themes;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class MarkdownLitePage : DemoPageBase
{
    private const string IdTheme = "lite.theme";
    private const string IdSize = "lite.size";

    public static string XamlSnippet => L.Instance.GetResource(DemoResources.Content.MarkdownLite.Source.Xaml);

    public static string CSharpSnippet => L.Instance.GetResource(DemoResources.Content.MarkdownLite.Source.CSharp);

    public static string SampleMarkdown { get; } = L.Instance.GetResource(DemoResources.Content.MarkdownLite.Samples.Markdown);

    private string _theme = MarkdownTypographyThemes.Basic;
    private string _size = MarkdownTypographySizes.Normal;

    public MarkdownLitePage()
    {
        InitializeComponent();
    }


    public override string TitleKey => DemoResources.Page.MarkdownLite.Title;

    public override string DescriptionKey => DemoResources.Page.MarkdownLite.Desc;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    public override void OnPropertyCommitted(PropertyValueChange change)
    {
        switch (change.Item.Id)
        {
            case IdTheme when change.NewValue is string theme:
                _theme = theme;
                break;
            case IdSize when change.NewValue is string size:
                _size = size;
                break;
        }

        Viewer.TypographyTheme = _theme;
        Viewer.TypographySize = _size;
    }

    protected override IReadOnlyList<PropertyItem> BuildProperties()
    {
        var theme = new PropertyItem
        {
            Id = IdTheme,
            Name = L.Instance.GetResource(DemoResources.Content.MarkdownLite.Properties.TypographyTheme),
            Kind = PropertyItemKind.Choice,
            Options =
            [
                MarkdownTypographyThemes.Basic,
                MarkdownTypographyThemes.Simple,
                MarkdownTypographyThemes.GeekBlack,
                MarkdownTypographyThemes.WeChatFormat,
            ],
            Value = _theme,
            DefaultValue = MarkdownTypographyThemes.Basic,
            Description = L.Instance.GetResource(DemoResources.Content.MarkdownLite.Properties.LiteIncludesTheSame18TypographyThemesFour),
        };
        var size = new PropertyItem
        {
            Id = IdSize,
            Name = L.Instance.GetResource(DemoResources.Content.MarkdownLite.Properties.TypographySize),
            Kind = PropertyItemKind.Choice,
            Options = [MarkdownTypographySizes.Normal, MarkdownTypographySizes.Small],
            Value = _size,
            DefaultValue = MarkdownTypographySizes.Normal,
            Description = "Normal / Small。",
        };

        return [theme, size];
    }
}
