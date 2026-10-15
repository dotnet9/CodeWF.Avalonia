using DemoResources = global::CodeWF.Avalonia.Demo.I18n.ResourceKeys;
using L = global::CodeWF.Toolkit.Lang.I18nManager;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using CodeWF.Avalonia.Controls.Controls;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class CodeWFWindowPage : DemoPageBase
{
    public static string XamlSnippet => L.Instance.GetResource(DemoResources.Content.CodeWFWindow.Source.Xaml);

    public static string CSharpSnippet => L.Instance.GetResource(DemoResources.Content.CodeWFWindow.Source.CSharp);

    public ViewModels.WindowDemoViewModel ViewModel { get; } = new();

    public CodeWFWindowPage()
    {
        InitializeComponent();
    }

    public override string TitleKey => DemoResources.Page.CodeWFWindow.Title;

    public override string DescriptionKey => DemoResources.Page.CodeWFWindow.Desc;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    public override void OnPropertyCommitted(PropertyValueChange change) => ViewModel.Commit(change);

    protected override IReadOnlyList<PropertyItem> BuildProperties() => ViewModel.BuildProperties();

    private void OnOpenDefault(object? sender, RoutedEventArgs e) =>
        ShowSample(L.Instance.GetResource(DemoResources.Content.CodeWFWindow.OnOpenDefault.DefaultConfiguration), extends: ViewModel.ExtendsIntoTitleBar, fullScreenButton: false, customTitleContent: false);

    private void OnOpenCustom(object? sender, RoutedEventArgs e) =>
        ShowSample(L.Instance.GetResource(DemoResources.Content.CodeWFWindow.OnOpenCustom.MinimalButtonsAndCustomTitleBar), extends: ViewModel.ExtendsIntoTitleBar, fullScreenButton: false, customTitleContent: true);

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
            TitleBarHeight = ViewModel.TitleBarHeight,
            CaptionButtonWidth = ViewModel.CaptionButtonWidth,
            WindowCornerRadius = new CornerRadius(ViewModel.CornerRadius),
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
            var search = new TextBox { Width = 160, Height = 26, PlaceholderText = L.Instance.GetResource(DemoResources.Content.CodeWFWindow.ShowSample.Search) };
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

        if (owner is null) window.Show();
        else window.Show(owner);
    }
}
