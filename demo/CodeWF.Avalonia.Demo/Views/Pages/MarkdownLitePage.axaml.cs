using CodeWF.Avalonia.Markdown.Controls;
using CodeWF.Avalonia.Markdown.Themes;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class MarkdownLitePage : DemoPageBase
{
    private const string IdTheme = "lite.theme";
    private const string IdSize = "lite.size";

    public const string XamlSnippet = """
        <!-- Lite 与完整包的渲染控件同名同命名空间，换包不换代码 -->
        <PackageReference Include="CodeWF.Avalonia.Markdown.Lite" />

        <codewf:MarkdownViewer Markdown="{Binding Content}"
                               TypographyTheme="Basic"
                               TypographySize="Normal" />
        """;

    public const string CSharpSnippet = """
        // Lite 包：只带渲染管线与 18 套排版主题（无编辑器 / 导出 / 数学 / Mermaid / TextMate 高亮）
        // 完整包：在其上叠加图片（GIF/SVG）、公式、图表、代码高亮、PNG/PDF/Word/公众号导出
        //
        // 下方示例演示降级行为：图片退化为替代文本、公式保留原文、代码块单色。
        var viewer = new MarkdownViewer
        {
            Markdown = "# Lite 示例",
            TypographyTheme = MarkdownTypographyThemes.Simple,
            EnableVirtualization = true,
        };
        """;

    public static string SampleMarkdown { get; } = """
        # Markdown Lite 渲染范围

        本页内容刻意只使用 **Lite 支持的元素**：标题、**加粗**、*斜体*、`行内代码`、[链接](https://codewf.com)、列表、表格与代码块。

        ## 列表
        - 基础块级元素
        - 表格与引用
        - 任务列表勾选回写同样可用

        > 引用块：Lite 与完整包共用同一渲染管线。

        ## 降级演示（完整包能力在 Lite 中的表现）

        | 元素 | Lite 行为 |
        | --- | --- |
        | 图片 | 显示替代文本 |
        | 数学公式 | 显示 LaTeX 原文 |
        | 代码高亮 | 单色渲染 |

        ```csharp
        // 代码块单色渲染（完整包为 TextMate 高亮）
        Console.WriteLine("Hello, Markdown Lite!");
        ```

        - [x] 使用 Lite 渲染
        - [ ] 切换排版主题（右侧属性面板）
        """;

    private string _theme = MarkdownTypographyThemes.Basic;
    private string _size = MarkdownTypographySizes.Normal;

    public MarkdownLitePage()
    {
        InitializeComponent();
    }


    public override string TitleKey => "Demo.Page.MarkdownLite.Title";

    public override string DescriptionKey => "Demo.Page.MarkdownLite.Desc";

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    public override void OnPropertyCommitted(PropertyValueChange change)
    {
        switch (change.Item.Id)
        {
            case IdTheme when change.NewValue is string theme:
                _theme = theme;
                break;
            case IdSize when change.NewValue is string size:
                _size = size;
                break;
        }

        Viewer.TypographyTheme = _theme;
        Viewer.TypographySize = _size;
    }

    protected override IReadOnlyList<PropertyItem> BuildProperties()
    {
        var theme = new PropertyItem
        {
            Id = IdTheme,
            Name = "排版主题",
            Kind = PropertyItemKind.Choice,
            Options =
            [
                MarkdownTypographyThemes.Basic,
                MarkdownTypographyThemes.Simple,
                MarkdownTypographyThemes.GeekBlack,
                MarkdownTypographyThemes.WeChatFormat,
            ],
            Value = _theme,
            DefaultValue = MarkdownTypographyThemes.Basic,
            Description = "Lite 内置同款 18 套排版主题，此处列出常用四个。",
        };
        var size = new PropertyItem
        {
            Id = IdSize,
            Name = "排版尺寸",
            Kind = PropertyItemKind.Choice,
            Options = [MarkdownTypographySizes.Normal, MarkdownTypographySizes.Small],
            Value = _size,
            DefaultValue = MarkdownTypographySizes.Normal,
            Description = "Normal / Small。",
        };

        return [theme, size];
    }
}
