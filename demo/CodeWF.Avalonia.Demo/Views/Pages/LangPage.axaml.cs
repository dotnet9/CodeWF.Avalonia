using System.Globalization;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CodeWF.Avalonia.Demo.Infrastructure;
using CodeWF.Avalonia.Demo.ViewModels;
using CodeWF.Avalonia.Lang;
using CodeWF.Avalonia.Lang.Json;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using CodeWF.Toolkit.Logging;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class LangPage : DemoPageBase
{
    public const string XamlSnippet = """
        <!-- 全部界面文案经 I18n 标记扩展绑定，文化切换即时刷新 -->
        <TextBlock Text="{codewf:I18n Demo.Lang.SampleGreeting}" />
        <TextBlock Text="{codewf:I18n Demo.Lang.SampleFormat}" />

        <!-- 带参数模板：{codewf:I18nBinding Hello, Name={Binding UserName}} -->
        """;

    public const string CSharpSnippet = """
        // 三种内置后端按需注册（同一时刻一个插件）
        I18nManager.Instance.Register(
            new JsonLangPlugin { ResourceFolder = Path.Combine(AppContext.BaseDirectory, "I18n") },
            new CultureInfo("zh-CN"));            // 递归扫描目录内 *.json
        // I18nManager.Instance.Register(new XmlLangPlugin { ResourceFolder = folder }, culture);
        // I18nManager.Instance.Register(new ResxLangPlugin(typeof(Properties.Resources)), culture);

        // 追加程序集内嵌语言资源（如控件包分发的 JSON）
        I18nManager.Instance.AddResource(typeof(MainWindow).Assembly);

        // 运行时切换：全部 {codewf:I18n} 绑定自动刷新
        I18nManager.Instance.Culture = new CultureInfo("en-US");

        // 直接取值与诊断
        var text = I18nManager.Instance.GetResource("Demo.Lang.SampleGreeting");
        var languages = I18nManager.Instance.GetLanguages();
        var diagnostics = ((JsonLangPlugin)plugin).LoadDiagnostics;
        """;

    public LangPage()
    {
        InitializeComponent();
        I18nManager.Instance.CultureChanged += OnCultureChanged;
        UpdateState();
    }

    public override string TitleKey => "Demo.Page.Lang.Title";

    public override string DescriptionKey => "Demo.Page.Lang.Desc";

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    public override void OnDocumentOpened() => UpdateState();

    private void OnCultureChanged(object? sender, EventArgs e) => UpdateState();

    private void OnSwitchEnglish(object? sender, RoutedEventArgs e) =>
        ApplyCulture("en-US");

    private void OnSwitchChinese(object? sender, RoutedEventArgs e) =>
        ApplyCulture("zh-CN");

    private void OnSwitchSystem(object? sender, RoutedEventArgs e)
    {
        var system = CultureInfo.CurrentUICulture.Name.StartsWith("zh", StringComparison.OrdinalIgnoreCase)
            ? "zh-CN"
            : "en-US";
        ApplyCulture(system);
    }

    private void ApplyCulture(string cultureName)
    {
        I18nManager.Instance.Culture = new CultureInfo(cultureName);

        // 与 Shell 的语言下拉保持一致（可选，仅演示台做了同步）。
        if (TopLevel.GetTopLevel(this)?.DataContext is MainViewModel main)
        {
            var culture = main.Cultures.FirstOrDefault(c => c.Name == cultureName);
            if (culture is not null)
            {
                main.SelectedCulture = culture;
            }
        }

        Logger.Info("Lang：切换语言 → " + cultureName);
        UpdateState();
    }

    private void UpdateState()
    {
        var languages = I18nManager.Instance.GetLanguages();
        var languageNames = languages is null
            ? "无"
            : string.Join("、", languages.Select(l => l.CultureName));
        var culture = I18nManager.Instance.Culture;

        StateText.Text = $"当前文化：{culture?.Name ?? DemoBootstrapper.DefaultCultureName} · "
            + $"已加载语言：{languageNames} · "
            + $"资源目录：{DemoBootstrapper.I18nFolder}";
    }
}
