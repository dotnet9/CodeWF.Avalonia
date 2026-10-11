using Avalonia.Markup.Xaml;
using CodeWF.Avalonia.Demo.ViewModels;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class WelcomePage : DemoPageBase
{
    public const string XamlSnippet = """
        <!-- 演示台外壳：Ribbon 页签 + 导航树 + 文档区 + PropertyGrid + LogView -->
        <codewf:ACSemiTheme />
        <codewf:DataGridSemiTheme />
        <codewf:TreeDataGridSemiTheme />
        <codewf:DockSemiTheme />
        <codewf:CodeWFLogTheme />
        <codewf:MarkdownFullThemes />

        <Application.RequestedThemeVariant>Default</Application.RequestedThemeVariant>
        """;

    public WelcomePage()
    {
        InitializeComponent();
    }

    public override string TitleKey => "Demo.Page.Welcome.Title";

    public override string DescriptionKey => "Demo.Page.Welcome.Desc";

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => """
        // 启动引导：注册 Lang 资源（Json 后端）并初始化日志核心，必须在 Shell XAML 解析前调用
        I18nManager.Instance.Register(
            new JsonLangPlugin { ResourceFolder = Path.Combine(AppContext.BaseDirectory, "I18n") },
            new CultureInfo("zh-CN"));

        Logger.TryInitialize(new LoggerOptions
        {
            File = new FileLogOptions { DirectoryPath = logDirectory },
        });

        // 6 色板 × 浅/深：切换应用级主题变体，ACSemiTheme 已映射全部变体资源
        Application.Current.RequestedThemeVariant = SemiTheme.Aquatic;
        Application.Current.RequestedThemeVariant = ThemeVariant.Dark;

        // 运行时切换语言，全部 {codewf:I18n} 文案即时刷新
        I18nManager.Instance.Culture = new CultureInfo("en-US");
        """;
}
