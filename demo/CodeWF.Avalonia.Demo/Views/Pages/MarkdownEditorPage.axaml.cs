using Avalonia.Controls;
using Avalonia.Interactivity;
using CodeWF.Avalonia.Markdown.Editor.Controls;
using CodeWF.Avalonia.Markdown.Editor.Services;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class MarkdownEditorPage : DemoPageBase
{
    private const string IdFontSize = "mde.fontsize";
    private const string IdLineNumbers = "mde.linenums";
    private const string IdAutoPair = "mde.autopair";

    public const string XamlSnippet = """
        <!-- MarkdownEditorView 为纯代码构建控件（无 XAML 模板依赖），在代码中创建并放入容器 -->
        <Border x:Name="EditorHost" />
        """;

    public const string CSharpSnippet = """
        var editor = new MarkdownEditorView(new MarkdownEditorOptions
        {
            LinkUrlPlaceholder = "https://example.com",
            CodeFenceLanguage = "csharp",
            EnableAutoPair = true,
            ShowLineNumbers = true,
            FontSize = 15,
        });
        EditorHost.Content = editor;
        editor.SetText("# 标题\n\n正文");
        editor.MarkdownChanged += (_, text) => Save(text);

        // 40 项动作统一入口：MarkdownEditorAction 枚举
        await editor.ExecuteAsync(MarkdownEditorAction.Bold);
        await editor.ExecuteAsync(MarkdownEditorAction.Table);
        editor.InsertTable("| 列一 | 列二 |\\n| --- | --- |\\n| A | B |");
        editor.WrapSelection("**", "**");

        // 光标 / 选区 / 行数
        var (line, column, lineCount) = editor.CaretPosition;
        var text = editor.Text;
        """;

    private readonly MarkdownEditorView _editor;

    public MarkdownEditorPage()
    {
        InitializeComponent();

        _editor = new MarkdownEditorView(new MarkdownEditorOptions
        {
            LinkUrlPlaceholder = "https://example.com",
            CodeFenceLanguage = "csharp",
            EnableAutoPair = true,
            ShowLineNumbers = true,
            FontSize = 15,
        });
        _editor.SetText(SampleMarkdown);
        _editor.MarkdownChanged += (_, _) => UpdateStatus();

        EditorHost.Child = _editor;
        UpdateStatus();
    }



    public override string TitleKey => "Demo.Page.MarkdownEditor.Title";

    public override string DescriptionKey => "Demo.Page.MarkdownEditor.Desc";

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    public override void OnDocumentOpened() => UpdateStatus();

    public override void OnPropertyCommitted(PropertyValueChange change)
    {
        switch (change.Item.Id)
        {
            case IdFontSize when change.NewValue is decimal size:
                _editor.EditorFontSize = (double)size;
                break;
            case IdLineNumbers when change.NewValue is bool lineNumbers:
                _editor.ShowLineNumbers = lineNumbers;
                break;
            case IdAutoPair when change.NewValue is bool autoPair:
                _editor.EnableAutoPair = autoPair;
                break;
        }

        UpdateStatus();
    }

    protected override IReadOnlyList<PropertyItem> BuildProperties()
    {
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
            Description = "编辑器文本字号。",
        };
        var lineNumbers = new PropertyItem
        {
            Id = IdLineNumbers,
            Name = "显示行号",
            Kind = PropertyItemKind.Boolean,
            Value = true,
            DefaultValue = true,
            Description = "是否在编辑器左侧显示行号。",
        };
        var autoPair = new PropertyItem
        {
            Id = IdAutoPair,
            Name = "自动配对",
            Kind = PropertyItemKind.Boolean,
            Value = true,
            DefaultValue = true,
            Description = "输入 **、` 等成对标记时自动补全后半个标记。",
        };

        return [fontSize, lineNumbers, autoPair];
    }

    private void OnBold(object? sender, RoutedEventArgs e) =>
        _ = _editor.ExecuteAsync(MarkdownEditorAction.Bold);

    private void OnItalic(object? sender, RoutedEventArgs e) =>
        _ = _editor.ExecuteAsync(MarkdownEditorAction.Italic);

    private void OnInlineCode(object? sender, RoutedEventArgs e) =>
        _ = _editor.ExecuteAsync(MarkdownEditorAction.InlineCode);

    private void OnHeading(object? sender, RoutedEventArgs e) =>
        _ = _editor.ExecuteAsync(MarkdownEditorAction.Heading2);

    private void OnTable(object? sender, RoutedEventArgs e) =>
        _editor.InsertTable("| 列一 | 列二 |\n| --- | --- |\n| A | B |");

    private void OnTaskList(object? sender, RoutedEventArgs e) =>
        _ = _editor.ExecuteAsync(MarkdownEditorAction.TaskList);

    private void OnCodeFence(object? sender, RoutedEventArgs e) =>
        _ = _editor.ExecuteAsync(MarkdownEditorAction.CodeFence);

    private void OnQuote(object? sender, RoutedEventArgs e) =>
        _ = _editor.ExecuteAsync(MarkdownEditorAction.Quote);

    private void UpdateStatus()
    {
        var caret = _editor.CaretPosition;
        StatusText.Text = $"行 {caret.Line} / 列 {caret.Column} · 共 {caret.LineCount} 行 · {(_editor.Text?.Length ?? 0)} 字符";
    }

    private const string SampleMarkdown = """"
        # MarkdownEditorView

        AvaloniaEdit 驱动的 Markdown 源码编辑器。

        ## 内置能力
        - TextMate 语法着色
        - 标题 / 列表 / 表格 / 任务列表 / 代码块 / 数学块等约 40 项动作
        - 查找替换、自动配对、行号、字号

        ```csharp
        var editor = new MarkdownEditorView();
        ```

        > 用上方按钮或快捷键体验编辑动作；修改会实时反映在状态栏。
        """";
}
