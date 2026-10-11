using CodeWF.Avalonia.Markdown.Themes;
using ReactiveUI;

namespace CodeWF.Avalonia.Demo.ViewModels.Pages;

/// <summary>MarkdownViewer 演示页 VM：源文本编辑 / 增量渲染 / 排版主题 / 虚拟化。</summary>
public class MarkdownViewerPageViewModel : ReactiveObject
{
    public const string SampleMarkdown = """
        # MarkdownViewer 演示

        完整渲染管线：**Markdig 解析** → 块级模型 → `WindowedStackPanel` 虚拟化呈现。

        ## 任务回写
        勾选下面的任务，Markdown 源文本会同步更新（TaskWriteBackRequested）：

        - [x] 解析 Markdown 文本
        - [ ] 调整排版主题（右侧属性面板）
        - [ ] 编辑左侧源文本后点击「应用」，观察增量渲染耗时

        ## 表格与代码

        | 能力 | 说明 |
        | --- | --- |
        | 增量渲染 | 只重排变更块 |
        | 虚拟化 | 超过阈值后按窗口实现块 |

        ```csharp
        var viewer = new MarkdownViewer { Markdown = "# 你好" };
        ```

        > 提示：右侧 PropertyGrid 即本控件的兄弟实例——属性面板自身也是一次自我演示。
        """;

    private string _source = SampleMarkdown;
    private string _appliedMarkdown = SampleMarkdown;
    private string _theme = MarkdownTypographyThemes.Basic;
    private string _size = MarkdownTypographySizes.Normal;
    private bool _enableVirtualization = true;
    private int _threshold = 40;

    /// <summary>源文本（编辑区）。</summary>
    public string Source
    {
        get => _source;
        set => this.RaiseAndSetIfChanged(ref _source, value);
    }

    /// <summary>已应用的文本（触发渲染）。</summary>
    public string AppliedMarkdown
    {
        get => _appliedMarkdown;
        private set => this.RaiseAndSetIfChanged(ref _appliedMarkdown, value);
    }

    public string Theme
    {
        get => _theme;
        set => this.RaiseAndSetIfChanged(ref _theme, value);
    }

    public string Size
    {
        get => _size;
        set => this.RaiseAndSetIfChanged(ref _size, value);
    }

    public bool EnableVirtualization
    {
        get => _enableVirtualization;
        set => this.RaiseAndSetIfChanged(ref _enableVirtualization, value);
    }

    public int Threshold
    {
        get => _threshold;
        set => this.RaiseAndSetIfChanged(ref _threshold, value);
    }

    public string[] ThemeOptions { get; } =
    [
        MarkdownTypographyThemes.Basic,
        MarkdownTypographyThemes.OrangeHeart,
        MarkdownTypographyThemes.InkBlack,
        MarkdownTypographyThemes.ColorfulPurple,
        MarkdownTypographyThemes.TenderGreen,
        MarkdownTypographyThemes.Verdant,
        MarkdownTypographyThemes.RedScarlet,
        MarkdownTypographyThemes.BlueGlow,
        MarkdownTypographyThemes.TechnologyBlue,
        MarkdownTypographyThemes.LanQing,
        MarkdownTypographyThemes.Yamabuki,
        MarkdownTypographyThemes.FrontendPeak,
        MarkdownTypographyThemes.GeekBlack,
        MarkdownTypographyThemes.Simple,
        MarkdownTypographyThemes.RosePurple,
        MarkdownTypographyThemes.CuteGreen,
        MarkdownTypographyThemes.FullStackBlue,
        MarkdownTypographyThemes.WeChatFormat,
    ];

    public string[] SizeOptions { get; } = [MarkdownTypographySizes.Normal, MarkdownTypographySizes.Small];

    public void Apply() => AppliedMarkdown = Source;

    public void AppendParagraph()
    {
        Source += "\n\n新增段落：增量渲染只解析与重排变更块（"
                  + DateTime.Now.ToString("HH:mm:ss") + "）。";
        Apply();
    }

    public void ResetSample()
    {
        Source = SampleMarkdown;
        Apply();
    }

    /// <summary>任务勾选回写：把控件给出的最新 Markdown 应用回源文本。</summary>
    public void ApplyExternalEdit(string markdown)
    {
        Source = markdown;
        AppliedMarkdown = markdown;
    }
}
