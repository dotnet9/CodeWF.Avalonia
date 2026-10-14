using DemoResources = global::CodeWF.Avalonia.Demo.I18n.ResourceKeys;
using L = global::CodeWF.Avalonia.Lang.I18nManager;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using CodeWF.Avalonia.Controls.Controls;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class CodeWFWindowPage : DemoPageBase
{
    private const string IdTitleHeight = "cw.titleHeight";
    private const string IdCornerRadius = "cw.cornerRadius";
    private const string IdExtends = "cw.extends";

    public static string XamlSnippet => L.Instance.GetResource(DemoResources.Content.CodeWFWindow.Source.Xaml);

    public static string CSharpSnippet => L.Instance.GetResource(DemoResources.Content.CodeWFWindow.Source.CSharp);

    private double _titleBarHeight = 36d;
    private double _cornerRadius = 8d;
    private bool _extends = true;

    public CodeWFWindowPage()
    {
        InitializeComponent();
    }

    public override string TitleKey => DemoResources.Page.CodeWFWindow.Title;

    public override string DescriptionKey => DemoResources.Page.CodeWFWindow.Desc;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    public override void OnPropertyCommitted(PropertyValueChange change)
    {
        switch (change.Item.Id)
        {
            case IdTitleHeight when change.NewValue is decimal height:
                _titleBarHeight = (double)height;
                break;
            case IdCornerRadius when change.NewValue is decimal radius:
                _cornerRadius = (double)radius;
                break;
            case IdExtends when change.NewValue is bool extends:
                _extends = extends;
                break;
        }
    }

    protected override IReadOnlyList<PropertyItem> BuildProperties()
    {
        var titleHeight = new PropertyItem
        {
            Id = IdTitleHeight,
            Name = L.Instance.GetResource(DemoResources.Content.CodeWFWindow.Properties.TitleBarHeight),
            Kind = PropertyItemKind.Number,
            Value = (decimal)_titleBarHeight,
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
            Value = (decimal)_cornerRadius,
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
            Value = _extends,
            DefaultValue = false,
            Description = L.Instance.GetResource(DemoResources.Content.CodeWFWindow.Properties.ExtendTheWindowContentUnderneathTheTitleBar),
        };

        return [titleHeight, cornerRadius, extends];
    }

    private void OnOpenDefault(object? sender, RoutedEventArgs e) =>
        ShowSample(L.Instance.GetResource(DemoResources.Content.CodeWFWindow.OnOpenDefault.DefaultConfiguration), extends: false, fullScreenButton: false, customTitleContent: false);

    private void OnOpenCustom(object? sender, RoutedEventArgs e) =>
        ShowSample(L.Instance.GetResource(DemoResources.Content.CodeWFWindow.OnOpenCustom.MinimalButtonsAndCustomTitleBar), extends: false, fullScreenButton: false, customTitleContent: true);

    private void OnOpenExtends(object? sender, RoutedEventArgs e) =>
        ShowSample(L.Instance.GetResource(DemoResources.Content.CodeWFWindow.OnOpenExtends.ExtendContentIntoTitleBar), extends: true, fullScreenButton: true, customTitleContent: false);

    private void ShowSample(string name, bool extends, bool fullScreenButton, bool customTitleContent)
    {
        var owner = TopLevel.GetTopLevel(this) as Window;
        var window = new CodeWFWindow
        {
            Title = string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.CodeWFWindow.ShowSample.CodeWFWindowExampleFormat), name),
            Width = 640,
            Height = 420,
            TitleBarHeight = _titleBarHeight,
            WindowCornerRadius = new CornerRadius(_cornerRadius),
            ContentExtendsIntoTitleBar = extends,
            IsFullScreenButtonVisible = fullScreenButton,
        };

        if (customTitleContent)
        {
            window.LeftContent = new TextBlock
            {
                Text = "◆",
                Foreground = Brushes.DodgerBlue,
                VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0),
            };
            var search = new TextBox { Width = 160, Height = 26, Watermark = L.Instance.GetResource(DemoResources.Content.CodeWFWindow.ShowSample.Search) };
            DockPanel.SetDock(search, global::Avalonia.Controls.Dock.Right);
            window.RightContent = search;
        }

        var contentPanel = new Panel();
        if (extends)
        {
            var hero = new Border
            {
                Height = 72,
                Background = Brushes.DodgerBlue,
                Opacity = 0.85,
                Child = new TextBlock
                {
                    Text = L.Instance.GetResource(DemoResources.Content.CodeWFWindow.ShowSample.ThisContentExtendsIntoTheTitleBarContentExtendsIntoTitleBar),
                    Foreground = Brushes.White,
                    VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Center,
                    HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Center,
                },
            };
            contentPanel.Children.Add(hero);
        }

        contentPanel.Children.Add(new TextBlock
        {
            Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.CodeWFWindow.ShowSample.ConfigurationCloseThisWindowToTryAnotherConfigurationFormat), name),
            HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Center,
            TextAlignment = TextAlignment.Center,
        });
        window.Content = contentPanel;

        window.Show(owner);
    }
}
