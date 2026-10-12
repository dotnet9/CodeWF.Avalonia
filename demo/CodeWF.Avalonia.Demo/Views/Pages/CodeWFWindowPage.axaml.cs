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

    public const string XamlSnippet = """
        <!-- 替换 Window：无边框 + DWM 圆角 + 自定义标题栏，需引入 ACSemiTheme -->
        <codewf:CodeWFWindow xmlns:codewf="https://codewf.com"
                             Title="CodeWFWindow 示例"
                             TitleBarHeight="36"
                             WindowCornerRadius="8"
                             ContentExtendsIntoTitleBar="True"
                             IsFullScreenButtonVisible="True">
          <codewf:CodeWFWindow.LeftContent>
            <TextBlock Text="◆" Foreground="DodgerBlue" VerticalAlignment="Center" />
          </codewf:CodeWFWindow.LeftContent>
          <Panel>
            <!-- 窗口内容；钻入模式下可延伸到标题栏下方 -->
          </Panel>
        </codewf:CodeWFWindow>
        """;

    public const string CSharpSnippet = """
        var window = new CodeWFWindow
        {
            Title = "CodeWFWindow 示例",
            Width = 640,
            Height = 420,
            ContentExtendsIntoTitleBar = true,   // 内容钻入标题栏
            IsFullScreenButtonVisible = true,
            IsMinimizeButtonVisible = true,
            WindowCornerRadius = new CornerRadius(8),
        };
        window.Content = new TextBlock
        {
            Text = "无边框窗口内容",
            HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Center,
        };
        window.Show(owner);   // DWM 圆角仅 Windows 生效，其它平台自动降级
        """;

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
            Name = "标题栏高度",
            Kind = PropertyItemKind.Number,
            Value = (decimal)_titleBarHeight,
            DefaultValue = 32m,
            Minimum = 24,
            Maximum = 64,
            Unit = "px",
            Description = "新打开示例窗口的标题栏高度。",
        };
        var cornerRadius = new PropertyItem
        {
            Id = IdCornerRadius,
            Name = "窗口圆角",
            Kind = PropertyItemKind.Number,
            Value = (decimal)_cornerRadius,
            DefaultValue = 4m,
            Minimum = 0,
            Maximum = 24,
            Unit = "px",
            Description = "窗口四角圆角半径（Windows DWM）。",
        };
        var extends = new PropertyItem
        {
            Id = IdExtends,
            Name = "内容钻入标题栏",
            Kind = PropertyItemKind.Boolean,
            Value = _extends,
            DefaultValue = false,
            Description = "窗口内容延伸至标题栏下方，形成整体化视觉。",
        };

        return [titleHeight, cornerRadius, extends];
    }

    private void OnOpenDefault(object? sender, RoutedEventArgs e) =>
        ShowSample("默认配置", extends: false, fullScreenButton: false, customTitleContent: false);

    private void OnOpenCustom(object? sender, RoutedEventArgs e) =>
        ShowSample("精简按钮 + 自定义标题栏", extends: false, fullScreenButton: false, customTitleContent: true);

    private void OnOpenExtends(object? sender, RoutedEventArgs e) =>
        ShowSample("内容钻入标题栏", extends: true, fullScreenButton: true, customTitleContent: false);

    private void ShowSample(string name, bool extends, bool fullScreenButton, bool customTitleContent)
    {
        var owner = TopLevel.GetTopLevel(this) as Window;
        var window = new CodeWFWindow
        {
            Title = $"CodeWFWindow 示例 · {name}",
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
            var search = new TextBox { Width = 160, Height = 26, Watermark = "搜索…" };
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
                    Text = "这段内容钻入了标题栏（ContentExtendsIntoTitleBar=True）",
                    Foreground = Brushes.White,
                    VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Center,
                    HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Center,
                },
            };
            contentPanel.Children.Add(hero);
        }

        contentPanel.Children.Add(new TextBlock
        {
            Text = $"配置：{name}\n关闭此窗口后可再试其它配置。",
            HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Center,
            TextAlignment = TextAlignment.Center,
        });
        window.Content = contentPanel;

        window.Show(owner);
    }
}
