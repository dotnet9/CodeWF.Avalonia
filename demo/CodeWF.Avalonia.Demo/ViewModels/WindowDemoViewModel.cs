using DemoResources = global::CodeWF.Avalonia.Demo.I18n.ResourceKeys;
using L = global::CodeWF.Toolkit.Lang.I18nManager;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.ViewModels;

/// <summary>维护示例窗口参数；窗口创建与显示留在视图的界面接线中。</summary>
public sealed class WindowDemoViewModel
{
    private const string IdTitleHeight = "cw.titleHeight";
    private const string IdCornerRadius = "cw.cornerRadius";
    private const string IdExtends = "cw.extends";

    private const string IdCaptionWidth = "cw.captionWidth";

    public double TitleBarHeight { get; private set; } = 36;
    public double CaptionButtonWidth { get; private set; } = 32;
    public double CornerRadius { get; private set; } = 8;
    public bool ExtendsIntoTitleBar { get; private set; }

    public void Commit(PropertyValueChange change)
    {
        switch (change.Item.Id)
        {
            case IdTitleHeight when change.NewValue is decimal height:
                TitleBarHeight = (double)height;
                break;
            case IdCaptionWidth when change.NewValue is decimal width:
                CaptionButtonWidth = (double)width;
                break;
            case IdCornerRadius when change.NewValue is decimal radius:
                CornerRadius = (double)radius;
                break;
            case IdExtends when change.NewValue is bool extends:
                ExtendsIntoTitleBar = extends;
                break;
        }
    }

    public IReadOnlyList<PropertyItem> BuildProperties()
    {
        var titleHeight = new PropertyItem
        {
            Id = IdTitleHeight,
            Name = L.Instance.GetResource(DemoResources.Content.CodeWFWindow.Properties.TitleBarHeight),
            Kind = PropertyItemKind.Number,
            Value = (decimal)TitleBarHeight,
            DefaultValue = 32m,
            Minimum = 24,
            Maximum = 64,
            Unit = "px",
            Description = L.Instance.GetResource(DemoResources.Content.CodeWFWindow.Properties.TitleBarHeightForTheNextExampleWindow),
        };
        var cornerRadius = new PropertyItem
        {
            Id = IdCornerRadius,
            Name = L.Instance.GetResource(DemoResources.Content.CodeWFWindow.Properties.WindowCornerRadius),
            Kind = PropertyItemKind.Number,
            Value = (decimal)CornerRadius,
            DefaultValue = 4m,
            Minimum = 0,
            Maximum = 24,
            Unit = "px",
            Description = L.Instance.GetResource(DemoResources.Content.CodeWFWindow.Properties.WindowCornerRadiusWindowsDWM),
        };
        var extends = new PropertyItem
        {
            Id = IdExtends,
            Name = L.Instance.GetResource(DemoResources.Content.CodeWFWindow.Properties.ExtendContentIntoTitleBar),
            Kind = PropertyItemKind.Boolean,
            Value = ExtendsIntoTitleBar,
            DefaultValue = false,
            Description = L.Instance.GetResource(DemoResources.Content.CodeWFWindow.Properties.ExtendTheWindowContentUnderneathTheTitleBar),
        };

        var captionWidth = new PropertyItem
        {
            Id = IdCaptionWidth,
            Name = L.Instance.GetResource(DemoResources.Content.CodeWFWindow.Properties.CaptionButtonWidth),
            Kind = PropertyItemKind.Number,
            Value = (decimal)CaptionButtonWidth,
            DefaultValue = 32m,
            Minimum = 24,
            Maximum = 64,
            Unit = "px",
            Description = L.Instance.GetResource(DemoResources.Content.CodeWFWindow.Properties.CaptionButtonWidthDescription),
        };
        return [titleHeight, captionWidth, cornerRadius, extends];
    }

}
