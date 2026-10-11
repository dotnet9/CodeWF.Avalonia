using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using CodeWF.Avalonia.Markdown;
using CodeWF.Avalonia.Markdown.Themes;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class MarkdownExportPage : DemoPageBase
{
    private const string IdTheme = "mex.theme";
    private const string IdSize = "mex.size";
    private const string IdCopyKind = "mex.copykind";

    public const string XamlSnippet = """
        <!-- 导出按钮 + 存储选择器，导出实现见 C# 段 -->
        <Button Content="导出 PNG" Click="OnExportPng" />
        <Button Content="复制公众号 HTML" Click="OnCopyWechat" />
        """;

    public const string CSharpSnippet = """
        // 四种格式一条龙：MarkdownDocumentExporter（themeName / typographySize 可选）
        MarkdownDocumentExporter.ExportMarkdown(markdown, ExportKind.Png, @"D:\\out\\doc.png");
        MarkdownDocumentExporter.ExportMarkdown(markdown, ExportKind.Pdf, @"D:\\out\\doc.pdf");
        MarkdownDocumentExporter.ExportMarkdown(markdown, ExportKind.Word, @"D:\\out\\doc.docx");
        MarkdownDocumentExporter.ExportMarkdown(markdown, ExportKind.Html, @"D:\\out\\doc.html",
            themeName: MarkdownTypographyThemes.WeChatFormat);

        // 公众号富文本复制（微信 / 知乎 / 掘金），写入系统剪贴板后可直接粘贴
        var ok = await MarkdownHtmlClipboard.TrySetHtmlAsync(
            markdown, "公众号", MarkdownTypographyThemes.WeChatFormat);

        // 存储选择器拿到本地路径后再导出
        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "导出 PNG",
            DefaultExtension = ".png",
        });
        if (file is not null)
        {
            MarkdownDocumentExporter.ExportMarkdown(markdown, ExportKind.Png, file.Path.LocalPath);
        }
        """;

    private string _theme = MarkdownTypographyThemes.WeChatFormat;
    private string _size = MarkdownTypographySizes.Normal;
    private string _copyKind = "微信";

    public MarkdownExportPage()
    {
        InitializeComponent();
    }

    public static string SampleMarkdown { get; } = """
        # 导出示例文档

        这份文档用于演示 **PNG / PDF / Word / HTML** 四种导出与公众号富文本复制。

        ## 特性清单
        - 按排版主题渲染导出样式
        - PDF 支持页面尺寸与边距选项
        - 公众号 HTML 兼容微信编辑器粘贴

        ## 表格

        | 格式 | 说明 |
        | --- | --- |
        | PNG | 位图，适合预览分享 |
        | PDF | 打印与归档 |
        | Word | 二次编辑 |
        | HTML | 发布到公众号 / 站点 |

        ```csharp
        MarkdownDocumentExporter.ExportMarkdown(md, ExportKind.Pdf, path);
        ```

        > 提示：导出主题可在右侧属性面板切换后重试。
        """;

    public override string TitleKey => "Demo.Page.MarkdownExport.Title";

    public override string DescriptionKey => "Demo.Page.MarkdownExport.Desc";

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
            case IdCopyKind when change.NewValue is string kind:
                _copyKind = kind;
                break;
        }
    }

    protected override IReadOnlyList<PropertyItem> BuildProperties()
    {
        var theme = new PropertyItem
        {
            Id = IdTheme,
            Name = "导出主题",
            Kind = PropertyItemKind.Choice,
            Options =
            [
                MarkdownTypographyThemes.Basic,
                MarkdownTypographyThemes.Simple,
                MarkdownTypographyThemes.GeekBlack,
                MarkdownTypographyThemes.WeChatFormat,
            ],
            Value = _theme,
            DefaultValue = MarkdownTypographyThemes.WeChatFormat,
            Description = "导出文档使用的排版主题。",
        };
        var size = new PropertyItem
        {
            Id = IdSize,
            Name = "排版尺寸",
            Kind = PropertyItemKind.Choice,
            Options = [MarkdownTypographySizes.Normal, MarkdownTypographySizes.Small],
            Value = _size,
            DefaultValue = MarkdownTypographySizes.Normal,
            Description = "导出排版尺寸。",
        };
        var copyKind = new PropertyItem
        {
            Id = IdCopyKind,
            Name = "复制目标",
            Kind = PropertyItemKind.Radio,
            Options = ["微信", "知乎", "掘金"],
            Value = _copyKind,
            DefaultValue = "微信",
            Description = "公众号富文本复制的目标平台。",
        };

        return [theme, size, copyKind];
    }

    private async void OnExportPng(object? sender, RoutedEventArgs e) =>
        await ExportAsync(ExportKind.Png, "导出 PNG", "markdown-export", ".png");

    private async void OnExportPdf(object? sender, RoutedEventArgs e) =>
        await ExportAsync(ExportKind.Pdf, "导出 PDF", "markdown-export", ".pdf");

    private async void OnExportWord(object? sender, RoutedEventArgs e) =>
        await ExportAsync(ExportKind.Word, "导出 Word", "markdown-export", ".docx");

    private async void OnExportHtml(object? sender, RoutedEventArgs e) =>
        await ExportAsync(ExportKind.Html, "导出 HTML", "markdown-export", ".html");

    private async void OnCopyWechat(object? sender, RoutedEventArgs e)
    {
        try
        {
            var kind = _copyKind switch
            {
                "知乎" => CopyKind.Zhihu,
                "掘金" => CopyKind.Juejin,
                _ => CopyKind.Wechat,
            };
            await MarkdownHtmlClipboard.SetHtmlAsync(SourceBox.Text ?? string.Empty, kind, _theme, _size);
            SetStatus($"已复制 {_copyKind} 富文本，可直接粘贴到编辑器。");
        }
        catch (Exception ex)
        {
            SetStatus("复制失败：" + ex.Message);
        }
    }

    private async Task ExportAsync(ExportKind kind, string title, string suggestedName, string extension)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
        {
            return;
        }

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = title,
            SuggestedFileName = suggestedName,
            DefaultExtension = extension,
        });
        if (file is null)
        {
            return;
        }

        var markdown = SourceBox.Text ?? string.Empty;
        var path = file.Path.LocalPath;
        SetStatus($"正在导出 {kind} → {path}");
        try
        {
            await Task.Run(() => MarkdownDocumentExporter.ExportMarkdown(
                markdown, kind, _theme, path, _size));
            SetStatus($"导出完成：{path}");
        }
        catch (Exception ex)
        {
            SetStatus($"导出失败：{ex.Message}");
        }
    }

    private void SetStatus(string message) => StatusText.Text = $"[{DateTime.Now:HH:mm:ss}] {message}";
}
