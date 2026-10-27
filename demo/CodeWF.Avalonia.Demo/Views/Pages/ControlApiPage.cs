using DemoResources = global::CodeWF.Avalonia.Demo.I18n.ResourceKeys;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using CodeWF.Avalonia.Controls.Controls;
using CodeWF.Avalonia.Demo.ViewModels.Pages;
using CodeWF.Avalonia.TreeDataGrid;
using Ui = CodeWF.Avalonia.Controls.Presentation.StudioUi;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.Views.Pages;

/// <summary>聚焦单个公开控件，交互状态由视图模型维护。</summary>
public sealed class ControlApiPage : DemoPageBase
{
    private readonly string _key;
    private SegmentedPanel? _segmented;
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
            "SegmentedPanel" => "SegmentedPanel",
            _ => throw new ArgumentOutOfRangeException(nameof(api)),
        };
        Control preview;
        if (api == "CodeWFTitleBar")
        {
            var bar = new CodeWFTitleBar { Height = 44, RightContent = Ui.Key(".NET") };
            bar.Bind(CodeWFTitleBar.TitleProperty, new Binding(nameof(ViewModel.Title)));
            preview = bar;
        }
        else if (api == "LeftTop2RightBottomLine")
        {
            var line = new LeftTop2RightBottomLine { Width = 280, Height = 180 };
            line.Bind(LeftTop2RightBottomLine.BorderBrushProperty, line.GetResourceObservable("StudioAccent"));
            line.Bind(LeftTop2RightBottomLine.BorderThicknessProperty, new Binding(nameof(ViewModel.Thickness)));
            preview = line;
        }
        else if (api == "TreeDataGridExtension")
        {
            var grid = new global::Avalonia.Controls.TreeDataGrid { Source = ViewModel.Source, Height = 260 };
            grid.EnableDefaults(ViewModel.Source);
            grid.Bind(ToolTip.TipProperty, new Lang.MarkupExtensions.I18nExtension(DemoResources.Catalog.TreeHints).CreateBinding());
            var summary = Ui.Sub("");
            summary.Bind(TextBlock.TextProperty, new Binding(nameof(ViewModel.SelectionSummary)));
            preview = new StackPanel { Spacing = 8, Children = { grid, summary } };
        }
        else if (api == "SegmentedPanel")
        {
            // 轨道由宿主的 Border 画，面板只排段并对齐指示块；段文案直接复用界面已有的三个页签名。
            var panel = new SegmentedPanel { Spacing = 2, CornerRadius = new CornerRadius(999) };
            var buttons = new List<Button>();
            foreach (var label in new[] { DemoResources.Segment.Demo, DemoResources.Segment.Xaml, DemoResources.Segment.CSharp })
            {
                var segment = new Button { Height = 26, Padding = new Thickness(10, 0), MinWidth = 0 };
                segment.Bind(ContentControl.ContentProperty, new Lang.MarkupExtensions.I18nExtension(label).CreateBinding());
                segment.Click += (_, _) =>
                {
                    foreach (var other in buttons) SegmentedPanel.SetIsActive(other, ReferenceEquals(other, segment));
                };
                buttons.Add(segment);
                panel.Children.Add(segment);
            }
            SegmentedPanel.SetIsActive(buttons[0], true);
            _segmented = panel;
            preview = new Border { Classes = { "segmented-track" }, Child = panel };
        }
        else
        {
            var button = new Button { Classes = { "demo-action" } };
            button.Bind(ContentControl.ContentProperty, new Lang.MarkupExtensions.I18nExtension(DemoResources.Catalog.Notifications).CreateBinding());
            button.Command = ViewModel.NotifyCommand;
            preview = button;
        }
        Content = Ui.Page(Ui.Card("@Demo.Segment.Demo", preview));
    }

    public override string TitleKey => "Demo.Item." + _key;
    protected override IReadOnlyList<PropertyItem> BuildProperties() => ViewModel.BuildProperties(_key);
    public override void OnPropertyCommitted(PropertyValueChange change)
    {
        ViewModel.Commit(change);
        // 分段轨道的三个属性是控件自己的，直接写回实例：绑定名字一旦拼错，AOT 下只会静默失效。
        if (_segmented is null) return;
        switch (change.Item.Id)
        {
            case nameof(ControlApiViewModel.SegmentedSpacing):
                _segmented.Spacing = ViewModel.SegmentedSpacing;
                break;
            case nameof(ControlApiViewModel.SegmentedFillItems):
                _segmented.FillItems = ViewModel.SegmentedFillItems;
                break;
            case nameof(ControlApiViewModel.SegmentedCornerRadius):
                _segmented.CornerRadius = new CornerRadius((double)ViewModel.SegmentedCornerRadius);
                break;
        }
    }

    public override string DescriptionKey => "Demo.Api." + _key + ".Desc";
    public override string XamlCode => _key switch
    {
        "TitleBar" => "<codewf:CodeWFTitleBar Title=\"{Binding Title}\" />",
        "DiagonalLine" => "<codewf:LeftTop2RightBottomLine BorderThickness=\"{Binding Thickness}\" />",
        "TreeDataGrid" => "<TreeDataGrid Source=\"{Binding Source}\" />",
        "SegmentedPanel" => "<Border Classes=\"segmented-track\">\n  <codewf:SegmentedPanel Spacing=\"2\" FillItems=\"False\" CornerRadius=\"999\">\n    <Button Content=\"Demo\" codewf:SegmentedPanel.IsActive=\"True\" />\n    <Button Content=\"XAML\" />\n    <Button Content=\"C#\" />\n  </codewf:SegmentedPanel>\n</Border>",
        _ => "<Application codewf:LogNotifications.Mode=\"InApp\" />",
    };
    public override string CSharpCode => _key switch
    {
        "TitleBar" => "titleBar.Title = viewModel.Title;",
        "DiagonalLine" => "line.BorderThickness = viewModel.Thickness;",
        "TreeDataGrid" => "treeDataGrid.EnableDefaults(source);",
        "SegmentedPanel" => "SegmentedPanel.SetIsActive(demoButton, true);\npanel.Spacing = 2;\npanel.FillItems = false;\npanel.CornerRadius = new CornerRadius(999);",
        _ => "LogNotifications.SetMode(app, LogNotificationMode.InApp);\nLogNotifications.SetMinimumLevel(app, LogLevel.Warning);\nLogger.Warn(message);",
    };
}
