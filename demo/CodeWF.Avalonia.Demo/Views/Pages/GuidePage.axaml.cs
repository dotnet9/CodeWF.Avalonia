using L = global::CodeWF.Avalonia.Lang.I18nManager;
using Avalonia.Interactivity;
using CodeWF.Avalonia.Controls.Controls;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using CodeWF.Toolkit.Logging;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class GuidePage : DemoPageBase
{
    public static string XamlSnippet => L.Instance.GetResource("Demo.Text.GuidePageaxaml.XamlSnippet001");

    public static string CSharpSnippet => L.Instance.GetResource("Demo.Text.GuidePageaxaml.CSharpSnippet002");

    public GuidePage()
    {
        InitializeComponent();
        Guide.StepOpened += OnStepOpened;
    }


    public override string TitleKey => "Demo.Page.Guide.Title";

    public override string DescriptionKey => "Demo.Page.Guide.Desc";

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    private void OnStartGuide(object? sender, RoutedEventArgs e)
    {
        Logger.Info(L.Instance.GetResource("Demo.Text.GuidePageaxaml.OnStartGuide003"));
        Guide.ResetState();
        Guide.Show();
    }

    private void OnStepOpened(object? sender, GuideStepEventArgs e) =>
        Logger.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.GuidePageaxaml.OnStepOpened004"), e.Index + 1, Guide.StepCount, Guide.CurrentTitle));

    private void OnGuideCompleted(object? sender, System.EventArgs e) =>
        Logger.Info(L.Instance.GetResource("Demo.Text.GuidePageaxaml.OnGuideCompleted005"));

    protected override IReadOnlyList<PropertyItem> BuildProperties()
    {
        var placement = new PropertyItem
        {
            Id = "guide.info",
            Name = L.Instance.GetResource("Demo.Text.GuidePageaxaml.BuildProperties006"),
            Kind = PropertyItemKind.ReadOnly,
            Value = "Center / Top / TopLeft / TopRight / Left / LeftTop / LeftBottom / Right / RightTop / RightBottom / Bottom / BottomLeft / BottomRight",
            Description = L.Instance.GetResource("Demo.Text.GuidePageaxaml.BuildProperties007"),
        };

        return [placement];
    }
}
