using L = global::CodeWF.Avalonia.Lang.I18nManager;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using CodeWF.Avalonia.Controls.Controls;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using CodeWF.Toolkit.Logging;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class AppGuidePage : DemoPageBase
{
    public static string XamlSnippet => L.Instance.GetResource("Demo.Content.AppGuide.Source.Xaml");

    public static string CSharpSnippet => L.Instance.GetResource("Demo.Content.AppGuide.Source.CSharp");

    public AppGuidePage()
    {
        InitializeComponent();
    }


    public override string TitleKey => "Demo.Page.AppGuide.Title";

    public override string DescriptionKey => "Demo.Page.AppGuide.Desc";

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    public override void OnDocumentOpened()
    {
        AppGuide.StepOpened += OnStepOpened;
    }

    private void OnStartGuide(object? sender, RoutedEventArgs e)
    {
        if (AppGuide.StepCount > 0)
        {
            AppGuide.ResetState();
            AppGuide.Show();
            return;
        }

        var root = TopLevel.GetTopLevel(this);
        if (root is null)
        {
            return;
        }

        void AddStep(string title, string description, string targetName, GuidePlacementMode placement)
        {
            var target = FindDescendant(root, targetName);
            AppGuide.Steps.Add(new GuideStep
            {
                Title = title,
                Description = target is null ? description + L.Instance.GetResource("Demo.Content.AppGuide.OnStartGuide.TargetLookupFailedUsingTheDefaultBehavior") : description,
                Target = target,
                Placement = placement,
            });
        }

        AddStep(L.Instance.GetResource("Demo.Content.AppGuide.OnStartGuide.RibbonCommands"), L.Instance.GetResource("Demo.Content.AppGuide.OnStartGuide.EightRibbonGroupsContainTheDemoCommandsDouble"), "RibbonRoot", GuidePlacementMode.Bottom);
        AddStep(L.Instance.GetResource("Demo.Content.AppGuide.OnStartGuide.Navigation"), L.Instance.GetResource("Demo.Content.AppGuide.OnStartGuide.MirrorsRibbonGroupsClickAnItemToOpen"), "NavTree", GuidePlacementMode.Right);
        AddStep(L.Instance.GetResource("Demo.Content.AppGuide.OnStartGuide.Documents"), L.Instance.GetResource("Demo.Content.AppGuide.OnStartGuide.EachComponentHasADemoDocumentWithDemo"), "DocumentTabs", GuidePlacementMode.Top);
        AddStep("④ PropertyGrid", L.Instance.GetResource("Demo.Content.AppGuide.OnStartGuide.ThePropertyPanelConfiguresTheActiveDemoThe"), "PropertyPanelHost", GuidePlacementMode.Left);
        AddStep(L.Instance.GetResource("Demo.Content.AppGuide.OnStartGuide.Logs"), L.Instance.GetResource("Demo.Content.AppGuide.OnStartGuide.LiveLoggerEventsWithFilteringCopyClearAnd"), "LogPanelHost", GuidePlacementMode.Top);

        Logger.Info(L.Instance.GetResource("Demo.Content.AppGuide.OnStartGuide.AppGuideStartingWorkspaceTour"));
        AppGuide.Show();
    }

    private void OnStepOpened(object? sender, GuideStepEventArgs e) =>
        Logger.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Content.AppGuide.OnStepOpened.AppGuideStepFormat"), e.Index + 1, AppGuide.StepCount, AppGuide.CurrentTitle));

    private static Control? FindDescendant(object root, string name)
    {
        Control? result = null;
        if (root is Visual visual)
        {
            foreach (var descendant in visual.GetVisualDescendants())
            {
                if (descendant is Control control
                    && control.Name == name)
                {
                    result = control;
                    break;
                }
            }
        }

        return result;
    }

    protected override IReadOnlyList<PropertyItem> BuildProperties()
    {
        var note = new PropertyItem
        {
            Id = "appguide.note",
            Name = L.Instance.GetResource("Demo.Content.AppGuide.Properties.TargetLookup"),
            Kind = PropertyItemKind.ReadOnly,
            Value = "RibbonRoot / NavTree / DocumentTabs / PropertyPanelHost / LogPanelHost",
            Description = L.Instance.GetResource("Demo.Content.AppGuide.Properties.NamesOfTheFiveWorkspaceRegionsResolvedControls"),
        };

        return [note];
    }
}
