using Avalonia.Controls;
using Avalonia.Interactivity;
using CodeWF.Avalonia.Markdown.Editor.Controls;
using CodeWF.Avalonia.Markdown.Themes;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class MarkdownLiveEditorPage : DemoPageBase
{
    private const string IdTheme = "mle.theme";
    private const string IdFontSize = "mle.fontsize";

    public const string XamlSnippet = """
        <!-- MarkdownLiveEditorView 为纯代码构建控件，在代码中创建并放入容器 -->
        <Border x:Name="EditorHost" />
        """;

    public const string CSharpSnippet = """
        var editor = new MarkdownLiveEditorView(new MarkdownEditorOptions
        {
            EnableAutoPair = true,
            FontSize = 15,
        });
        editor.TypographyTheme = MarkdownTypographyThemes.Verdrant;  // 所见即所得的排版主题
        editor.SetText("# 实时编辑\\n\\n直接在块上编辑");
        EditorHost.Content = editor;

        editor.MarkdownChanged += (_, text) => Save(text);
        editor.FocusBlock(0);                       // 聚焦指定块
        await editor.ExecuteAsync(MarkdownEditorAction.Bold);
        editor.InsertText("追加文本");

        // 块模型指标
        var blockCount = editor.BlockCount;
        var activeBlock = editor.ActiveBlockIndex;
        var markdown = editor.Text;                 // = MarkdownBlockParser.Write(models)
        """;

    private readonly MarkdownLiveEditorView _editor;

    public MarkdownLiveEditorPage()
    {
        InitializeComponent();

        _editor = new MarkdownLiveEditorView(new MarkdownEditorOptions
        {
            EnableAutoPair = true,
            FontSize = 15,
        });
        _editor.TypographyTheme = MarkdownTypographyThemes.Verdant;
        _editor.SetText(SampleMarkdown);
        _editor.MarkdownChanged += (_, _) => UpdateStatus();

        EditorHost.Child = _editor;
        UpdateStatus();
    }



    public override string TitleKey => "Demo.Page.MarkdownLiveEditor.Title";

    public override string DescriptionKey => "Demo.Page.MarkdownLiveEditor.Desc";

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    public override void OnDocumentOpened() => UpdateStatus();

    public override void OnPropertyCommitted(PropertyValueChange change)
    {
        switch (change.Item.Id)
        {
            case IdTheme when change.NewValue is string theme:
                _editor.TypographyTheme = theme;
                break;
            case IdFontSize when change.NewValue is decimal size:
                _editor.EditorFontSize = (double)size;
                break;
        }

        UpdateStatus();
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
                MarkdownTypographyThemes.Verdant,
                MarkdownTypographyThemes.TenderGreen,
                MarkdownTypographyThemes.GeekBlack,
                MarkdownTypographyThemes.WeChatFormat,
            ],
            Value = MarkdownTypographyThemes.Verdant,
            DefaultValue = MarkdownTypographyThemes.Basic,
            Description = "所见即所得视图应用的排版主题。",
        };
        var fontSize = new PropertyItem
        {
            Id = IdFontSize,
            Name = "编辑器字号",
            Kind = PropertyItemKind.Number,
            Value = 15m,
            DefaultValue = 15m,
            Minimum = 10,
            Maximum = 28,
            Unit = "px",
            Description = "块编辑器文本字号。",
        };

        return [theme, fontSize];
    }

    private void OnInsert(object? sender, RoutedEventArgs e) =>
        _editor.InsertText("\n\n新插入的内容（InsertText）。");

    private void OnFocusFirst(object? sender, RoutedEventArgs e) =>
        _editor.FocusBlock(0);

    private void OnBold(object? sender, RoutedEventArgs e) =>
        _ = _editor.ExecuteAsync(MarkdownEditorAction.Bold);

    private void OnTable(object? sender, RoutedEventArgs e) =>
        _editor.InsertTable("| 列一 | 列二 |\n| --- | --- |\n| A | B |");

    private void UpdateStatus()
    {
        StatusText.Text =
            $"块数 {_editor.BlockCount} · 活动块索引 {_editor.ActiveBlockIndex} · {(_editor.Text?.Length ?? 0)} 字符";
    }

    private const string SampleMarkdown = """"
        # 所见即所得

        直接在块上编辑：标题、段落、列表、表格都是可编辑块。

        ## 列表
        - 块级建模（MarkdownBlockModel）
        - 编辑即渲染
        - 无左右分栏

        > 点击任意块开始编辑；顶部按钮演示插入与聚焦。

        | 能力 | 说明 |
        | --- | --- |
        | FocusBlock | 聚焦指定块 |
        | ApplyExternalEdit | 外部回写 |
        """";
}
