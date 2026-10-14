using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using CodeWF.Avalonia.Controls.Controls;
using CodeWF.Avalonia.Demo.ViewModels.Pages;
using CodeWF.Avalonia.TreeDataGrid;
using Ui = CodeWF.Avalonia.Controls.Presentation.StudioUi;

namespace CodeWF.Avalonia.Demo.Views.Pages;

/// <summary>Focused views of existing public controls; interaction state lives in the view model.</summary>
public sealed class ControlApiPage : DemoPageBase
{
    private readonly string _key;
    public ControlApiViewModel ViewModel { get; } = new();

    public ControlApiPage(string api)
    {
        DataContext = ViewModel;
        _key = api switch
        {
            "CodeWFTitleBar" => "TitleBar",
            "LeftTop2RightBottomLine" => "DiagonalLine",
            "TreeDataGridExtension" => "TreeDataGrid",
            "LogNotifications" => "Notifications",
            _ => throw new ArgumentOutOfRangeException(nameof(api)),
        };
        Control preview;
        Control input;
        if (api == "CodeWFTitleBar")
        {
            var bar = new CodeWFTitleBar { Height = 44, LeftContent = Ui.Key("CodeWF"), RightContent = Ui.Key(".NET") };
            bar.Bind(CodeWFTitleBar.TitleProperty, new Binding(nameof(ViewModel.Title)));
            var text = Ui.Input(ViewModel.Title);
            text.Bind(TextBox.TextProperty, new Binding(nameof(ViewModel.Title)) { Mode = BindingMode.TwoWay });
            preview = bar;
            input = Ui.Field("Title", text);
        }
        else if (api == "LeftTop2RightBottomLine")
        {
            var line = new LeftTop2RightBottomLine { Width = 280, Height = 180, BorderBrush = Brushes.MediumPurple };
            line.Bind(LeftTop2RightBottomLine.BorderThicknessProperty, new Binding(nameof(ViewModel.Thickness)));
            var slider = new Slider { Minimum = 1, Maximum = 12, Width = 240 };
            slider.Bind(Slider.ValueProperty, new Binding(nameof(ViewModel.Thickness)) { Mode = BindingMode.TwoWay });
            preview = line;
            input = Ui.Field("BorderThickness", slider);
        }
        else if (api == "TreeDataGridExtension")
        {
            var grid = new global::Avalonia.Controls.TreeDataGrid { Source = ViewModel.Source, Height = 260 };
            grid.EnableDefaults(ViewModel.Source);
            var summary = Ui.Sub("");
            summary.Bind(TextBlock.TextProperty, new Binding(nameof(ViewModel.SelectionSummary)));
            preview = new StackPanel { Spacing = 8, Children = { grid, summary } };
            input = Ui.Hint("@Demo.Catalog.TreeHints");
        }
        else
        {
            var button = new Button { Classes = { "demo-action" } };
            button.Bind(ContentControl.ContentProperty, new Lang.MarkupExtensions.I18nExtension("Demo.Catalog.Notifications").CreateBinding());
            button.Command = ViewModel.NotifyCommand;
            preview = button;
            input = Ui.Key("LogNotifications.Mode = InApp; MinimumLevel = Warning");
        }
        Content = Ui.Page(Ui.Card("@Demo.Catalog.Input", input), Ui.Card("@Demo.Segment.Demo", preview));
    }

    public override string TitleKey => "Demo.Item." + _key;
    public override string DescriptionKey => "Demo.Api." + _key + ".Desc";
    public override string XamlCode => _key switch
    {
        "TitleBar" => "<codewf:CodeWFTitleBar Title=\"{Binding Title}\" />",
        "DiagonalLine" => "<codewf:LeftTop2RightBottomLine BorderThickness=\"{Binding Thickness}\" />",
        "TreeDataGrid" => "<TreeDataGrid Source=\"{Binding Source}\" />",
        _ => "<Application codewf:LogNotifications.Mode=\"InApp\" />",
    };
    public override string CSharpCode => _key switch
    {
        "TitleBar" => "titleBar.Title = viewModel.Title;",
        "DiagonalLine" => "line.BorderThickness = viewModel.Thickness;",
        "TreeDataGrid" => "treeDataGrid.EnableDefaults(source);",
        _ => "LogNotifications.SetMode(app, LogNotificationMode.InApp);\nLogNotifications.SetMinimumLevel(app, LogLevel.Warning);\nLogger.Warn(message);",
    };
}
