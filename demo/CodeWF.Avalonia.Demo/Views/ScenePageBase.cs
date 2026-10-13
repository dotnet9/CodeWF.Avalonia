using L = global::CodeWF.Avalonia.Lang.I18nManager;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using CodeWF.Avalonia.Lang;
using CodeWF.Toolkit.Logging;

namespace CodeWF.Avalonia.Demo.Views;

/// <summary>
/// 场景页基类：把「工作流步骤条 + 内嵌组件页」编排成一条连续流程。
/// 内嵌页实例由场景页自己持有并唯一挂载，Shell 打开场景页时只是把场景外壳放进文档区；
/// 步骤条在代码里构建，避免为纯 UI 状态引入绑定开销。
/// </summary>
public abstract class ScenePageBase : DemoPageBase
{
    private readonly List<string> _stepKeys = [];
    private readonly List<DemoPageBase> _pages = [];
    private readonly List<Button> _stepButtons = [];
    private readonly List<TextBlock> _stepMarkers = [];
    private readonly StackPanel _stepBar = new() { Orientation = Orientation.Horizontal, Spacing = 8 };
    private readonly ContentControl _pageHost = new();
    private readonly TextBlock _summaryText = new()
    {
        FontSize = 11.5,
        Opacity = 0.66,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(16, 0, 0, 0),
    };

    private int _stepIndex;

    protected ScenePageBase()
    {
        // 语言切换时刷新步骤标题与状态栏文案
        I18nManager.Instance.CultureChanged += (_, _) =>
        {
            RefreshStepTitles();
            RefreshSummary();
        };
    }

    /// <summary>步骤数量。</summary>
    public int StepCount => _pages.Count;

    /// <summary>当前步骤索引。</summary>
    public int StepIndex => _stepIndex;

    /// <summary>当前内嵌组件页。</summary>
    public DemoPageBase? ActiveScenePage => _stepIndex >= 0 && _stepIndex < _pages.Count ? _pages[_stepIndex] : null;

    /// <summary>步骤条右侧的状态说明文案键（各场景页自行组织）。</summary>
    protected abstract string SummaryKey { get; }

    /// <summary>注册一步：标题键 + 该步要展示的组件页。</summary>
    protected void AddStep(string titleKey, DemoPageBase page)
    {
        _stepKeys.Add(titleKey);
        _pages.Add(page);
    }

    /// <summary>构建并挂载场景外壳（各场景页构造函数末尾调用）。</summary>
    protected void AttachSceneShell(int initialStep = 0)
    {
        BuildStepBar();
        _stepIndex = Math.Clamp(initialStep, 0, Math.Max(0, _pages.Count - 1));
        RefreshStepStates();
        RefreshSummary();
        _pageHost.Content = ActiveScenePage;
        Content = BuildSceneShell();
        ActiveScenePage?.OnDocumentOpened();
    }

    /// <summary>切换到指定步骤（步骤条点击 / 场景页按钮共用）。</summary>
    public void GoToStep(int index)
    {
        if (index < 0 || index >= _pages.Count || index == _stepIndex)
        {
            return;
        }

        _stepIndex = index;
        RefreshStepStates();
        RefreshSummary();
        _pageHost.Content = ActiveScenePage;
        ActiveScenePage?.OnDocumentOpened();
        Logger.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.ScenePageBase.GoToStep001"), index + 1, _pages.Count, I18nManager.Instance.GetResource(_stepKeys[index])));
    }

    public override void OnDocumentOpened()
    {
        ActiveScenePage?.OnDocumentOpened();
    }

    private Control BuildSceneShell()
    {
        var stepBarBorder = new Border
        {
            Padding = new Thickness(16, 10),
            BorderThickness = new Thickness(0, 0, 0, 1),
        };
        stepBarBorder.Bind(Border.BorderBrushProperty, stepBarBorder.GetResourceObservable("TabControlSeparatorBorderBrush"));

        var scroll = new ScrollViewer
        {
            Content = _stepBar,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
        };

        var head = new DockPanel { LastChildFill = false };
        DockPanel.SetDock(scroll, global::Avalonia.Controls.Dock.Left);
        DockPanel.SetDock(_summaryText, global::Avalonia.Controls.Dock.Right);
        head.Children.Add(scroll);
        head.Children.Add(_summaryText);
        stepBarBorder.Child = head;

        var grid = new Grid { RowDefinitions = new RowDefinitions("Auto,*") };
        Grid.SetRow(stepBarBorder, 0);
        Grid.SetRow(_pageHost, 1);
        grid.Children.Add(stepBarBorder);
        grid.Children.Add(_pageHost);
        return grid;
    }

    private void BuildStepBar()
    {
        _stepBar.Children.Clear();
        _stepButtons.Clear();
        _stepMarkers.Clear();

        for (var i = 0; i < _pages.Count; i++)
        {
            var index = i;
            var marker = new TextBlock { FontSize = 10, FontWeight = FontWeight.Bold };
            var markerHost = new Border
            {
                Width = 16,
                Height = 16,
                CornerRadius = new CornerRadius(8),
                Background = new SolidColorBrush(Color.Parse("#22000000")),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Child = marker,
            };
            var title = new TextBlock
            {
                FontSize = 12,
                VerticalAlignment = VerticalAlignment.Center,
                Text = I18nManager.Instance.GetResource(_stepKeys[i]),
            };

            var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 7 };
            row.Children.Add(markerHost);
            row.Children.Add(title);

            var button = new Button
            {
                Classes = { "scene-step" },
                Padding = new Thickness(10, 4),
                CornerRadius = new CornerRadius(14),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Color.Parse("#22000000")),
                Content = row,
            };
            button.Click += (_, _) => GoToStep(index);

            _stepButtons.Add(button);
            _stepMarkers.Add(marker);
            _stepBar.Children.Add(button);
        }
    }

    private void RefreshStepTitles()
    {
        for (var i = 0; i < _stepButtons.Count && i < _stepKeys.Count; i++)
        {
            if (_stepButtons[i].Content is StackPanel { Children: [_, TextBlock title] })
            {
                title.Text = I18nManager.Instance.GetResource(_stepKeys[i]);
            }
        }
    }

    private void RefreshStepStates()
    {
        var accent = Color.Parse("#0064FA");
        if (Application.Current is { } app
            && app.TryFindResource("SystemAccentColor", app.ActualThemeVariant, out var value)
            && value is Color color)
        {
            accent = color;
        }

        for (var i = 0; i < _stepButtons.Count; i++)
        {
            var isActive = i == _stepIndex;
            var isDone = i < _stepIndex;

            _stepMarkers[i].Text = isDone ? "✓" : (i + 1).ToString();
            _stepMarkers[i].Opacity = isActive || isDone ? 1 : 0.75;

            _stepButtons[i].Background = isActive
                ? new SolidColorBrush(Color.FromArgb(26, accent.R, accent.G, accent.B))
                : Brushes.Transparent;
            _stepButtons[i].BorderBrush = isActive
                ? new SolidColorBrush(accent)
                : new SolidColorBrush(Color.Parse("#22000000"));
            _stepButtons[i].FontWeight = isActive ? FontWeight.SemiBold : FontWeight.Normal;
        }
    }

    private void RefreshSummary() => _summaryText.Text = I18nManager.Instance.GetResource(SummaryKey);
}
