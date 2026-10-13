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

    public static string XamlSnippet => L.Instance.GetResource("Demo.Text.CodeWFWindowPageaxaml.XamlSnippet001");

    public static string CSharpSnippet => L.Instance.GetResource("Demo.Text.CodeWFWindowPageaxaml.CSharpSnippet002");

    private double _titleBarHeight = 36d;
    private double _cornerRadius = 8d;
    private bool _extends = true;

    public CodeWFWindowPage()
    {
        InitializeComponent();
    }

    public override string TitleKey => "Demo.Page.CodeWFWindow.Title";

    public override string DescriptionKey => "Demo.Page.CodeWFWindow.Desc";

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
            Name = L.Instance.GetResource("Demo.Text.CodeWFWindowPageaxaml.BuildProperties003"),
            Kind = PropertyItemKind.Number,
            Value = (decimal)_titleBarHeight,
            DefaultValue = 32m,
            Minimum = 24,
            Maximum = 64,
            Unit = "px",
            Description = L.Instance.GetResource("Demo.Text.CodeWFWindowPageaxaml.BuildProperties004"),
        };
        var cornerRadius = new PropertyItem
        {
            Id = IdCornerRadius,
            Name = L.Instance.GetResource("Demo.Text.CodeWFWindowPageaxaml.BuildProperties005"),
            Kind = PropertyItemKind.Number,
            Value = (decimal)_cornerRadius,
            DefaultValue = 4m,
            Minimum = 0,
            Maximum = 24,
            Unit = "px",
            Description = L.Instance.GetResource("Demo.Text.CodeWFWindowPageaxaml.BuildProperties006"),
        };
        var extends = new PropertyItem
        {
            Id = IdExtends,
            Name = L.Instance.GetResource("Demo.Text.CodeWFWindowPageaxaml.BuildProperties007"),
            Kind = PropertyItemKind.Boolean,
            Value = _extends,
            DefaultValue = false,
            Description = L.Instance.GetResource("Demo.Text.CodeWFWindowPageaxaml.BuildProperties008"),
        };

        return [titleHeight, cornerRadius, extends];
    }

    private void OnOpenDefault(object? sender, RoutedEventArgs e) =>
        ShowSample(L.Instance.GetResource("Demo.Text.CodeWFWindowPageaxaml.OnOpenDefault009"), extends: false, fullScreenButton: false, customTitleContent: false);

    private void OnOpenCustom(object? sender, RoutedEventArgs e) =>
        ShowSample(L.Instance.GetResource("Demo.Text.CodeWFWindowPageaxaml.OnOpenCustom010"), extends: false, fullScreenButton: false, customTitleContent: true);

    private void OnOpenExtends(object? sender, RoutedEventArgs e) =>
        ShowSample(L.Instance.GetResource("Demo.Text.CodeWFWindowPageaxaml.OnOpenExtends011"), extends: true, fullScreenButton: true, customTitleContent: false);

    private void ShowSample(string name, bool extends, bool fullScreenButton, bool customTitleContent)
    {
        var owner = TopLevel.GetTopLevel(this) as Window;
        var window = new CodeWFWindow
        {
            Title = string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.CodeWFWindowPageaxaml.ShowSample012"), name),
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
            var search = new TextBox { Width = 160, Height = 26, Watermark = L.Instance.GetResource("Demo.Text.CodeWFWindowPageaxaml.ShowSample013") };
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
                    Text = L.Instance.GetResource("Demo.Text.CodeWFWindowPageaxaml.ShowSample014"),
                    Foreground = Brushes.White,
                    VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Center,
                    HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Center,
                },
            };
            contentPanel.Children.Add(hero);
        }

        contentPanel.Children.Add(new TextBlock
        {
            Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.CodeWFWindowPageaxaml.ShowSample015"), name),
            HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Center,
            TextAlignment = TextAlignment.Center,
        });
        window.Content = contentPanel;

        window.Show(owner);
    }
}
