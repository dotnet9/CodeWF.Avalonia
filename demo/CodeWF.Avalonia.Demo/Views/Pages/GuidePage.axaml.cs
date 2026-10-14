using L = global::CodeWF.Avalonia.Lang.I18nManager;
using Avalonia.Interactivity;
using CodeWF.Avalonia.Controls.Controls;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using CodeWF.Toolkit.Logging;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class GuidePage : DemoPageBase
{
    public static string XamlSnippet => L.Instance.GetResource("Demo.Content.Guide.Source.Xaml");

    public static string CSharpSnippet => L.Instance.GetResource("Demo.Content.Guide.Source.CSharp");

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
        Logger.Info(L.Instance.GetResource("Demo.Content.Guide.OnStartGuide.GuideStartingTour"));
        Guide.ResetState();
        Guide.Show();
    }

    private void OnStepOpened(object? sender, GuideStepEventArgs e) =>
        Logger.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Content.Guide.OnStepOpened.GuideStepFormat"), e.Index + 1, Guide.StepCount, Guide.CurrentTitle));

    private void OnGuideCompleted(object? sender, System.EventArgs e) =>
        Logger.Info(L.Instance.GetResource("Demo.Content.Guide.OnGuideCompleted.GuideTourCompleted"));

    protected override IReadOnlyList<PropertyItem> BuildProperties()
    {
        var placement = new PropertyItem
        {
            Id = "guide.info",
            Name = L.Instance.GetResource("Demo.Content.Guide.Properties.PlacementDetails"),
            Kind = PropertyItemKind.ReadOnly,
            Value = "Center / Top / TopLeft / TopRight / Left / LeftTop / LeftBottom / Right / RightTop / RightBottom / Bottom / BottomLeft / BottomRight",
            Description = L.Instance.GetResource("Demo.Content.Guide.Properties.EachStepSetsPlacement13PositionsCoverThe"),
        };

        return [placement];
    }
}
