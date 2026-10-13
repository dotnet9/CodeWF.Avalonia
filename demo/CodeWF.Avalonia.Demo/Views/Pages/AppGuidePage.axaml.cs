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
    public static string XamlSnippet => L.Instance.GetResource("Demo.Text.AppGuidePageaxaml.XamlSnippet001");

    public static string CSharpSnippet => L.Instance.GetResource("Demo.Text.AppGuidePageaxaml.CSharpSnippet002");

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
                Description = target is null ? description + L.Instance.GetResource("Demo.Text.AppGuidePageaxaml.OnStartGuide003") : description,
                Target = target,
                Placement = placement,
            });
        }

        AddStep(L.Instance.GetResource("Demo.Text.AppGuidePageaxaml.OnStartGuide004"), L.Instance.GetResource("Demo.Text.AppGuidePageaxaml.OnStartGuide005"), "RibbonRoot", GuidePlacementMode.Bottom);
        AddStep(L.Instance.GetResource("Demo.Text.AppGuidePageaxaml.OnStartGuide006"), L.Instance.GetResource("Demo.Text.AppGuidePageaxaml.OnStartGuide007"), "NavTree", GuidePlacementMode.Right);
        AddStep(L.Instance.GetResource("Demo.Text.AppGuidePageaxaml.OnStartGuide008"), L.Instance.GetResource("Demo.Text.AppGuidePageaxaml.OnStartGuide009"), "DocumentTabs", GuidePlacementMode.Top);
        AddStep("④ PropertyGrid", L.Instance.GetResource("Demo.Text.AppGuidePageaxaml.OnStartGuide010"), "PropertyPanelHost", GuidePlacementMode.Left);
        AddStep(L.Instance.GetResource("Demo.Text.AppGuidePageaxaml.OnStartGuide011"), L.Instance.GetResource("Demo.Text.AppGuidePageaxaml.OnStartGuide012"), "LogPanelHost", GuidePlacementMode.Top);

        Logger.Info(L.Instance.GetResource("Demo.Text.AppGuidePageaxaml.OnStartGuide013"));
        AppGuide.Show();
    }

    private void OnStepOpened(object? sender, GuideStepEventArgs e) =>
        Logger.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.AppGuidePageaxaml.OnStepOpened014"), e.Index + 1, AppGuide.StepCount, AppGuide.CurrentTitle));

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
            Name = L.Instance.GetResource("Demo.Text.AppGuidePageaxaml.BuildProperties015"),
            Kind = PropertyItemKind.ReadOnly,
            Value = "RibbonRoot / NavTree / DocumentTabs / PropertyPanelHost / LogPanelHost",
            Description = L.Instance.GetResource("Demo.Text.AppGuidePageaxaml.BuildProperties016"),
        };

        return [note];
    }
}
