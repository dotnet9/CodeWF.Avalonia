using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using CodeWF.Avalonia.Controls.Controls;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using CodeWF.Toolkit.Logging;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class AppGuidePage : DemoPageBase
{
    public const string XamlSnippet = """
        <!-- 引导控件置于页面即可（遮罩为窗口级 Popup）；目标在主窗口上，步骤由代码构建 -->
        <codewf:Guide x:Name="AppGuide" IsArrowVisible="True" />
        """;

    public const string CSharpSnippet = """
        // 目标在另一个控件树（主窗口）时：解析到根后按名称查找，再构建步骤
        var root = TopLevel.GetTopLevel(this) as Visual;
        var ribbon = FindDescendant(root, "RibbonRoot");
        var navTree = FindDescendant(root, "NavTree");

        AppGuide.Steps.Add(new GuideStep
        {
            Title = "① Ribbon",
            Description = "7 个页签分组演示命令",
            Target = ribbon,
            Placement = GuidePlacementMode.Bottom,
        });
        AppGuide.Show();
        """;

    public AppGuidePage()
    {
        InitializeComponent();
    }


    public override string TitleKey => "Demo.Page.AppGuide.Title";

    public override string DescriptionKey => "Demo.Page.AppGuide.Desc";

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    public override void OnDocumentOpened()
    {
        AppGuide.StepOpened += OnStepOpened;
    }

    private void OnStartGuide(object? sender, RoutedEventArgs e)
    {
        if (AppGuide.StepCount > 0)
        {
            AppGuide.ResetState();
            AppGuide.Show();
            return;
        }

        var root = TopLevel.GetTopLevel(this);
        if (root is null)
        {
            return;
        }

        void AddStep(string title, string description, string targetName, GuidePlacementMode placement)
        {
            var target = FindDescendant(root, targetName);
            AppGuide.Steps.Add(new GuideStep
            {
                Title = title,
                Description = target is null ? description + "（目标解析失败，将按缺省行为处理）" : description,
                Target = target,
                Placement = placement,
            });
        }

        AddStep("① Ribbon 命令区", "7 个页签分组全部演示命令；双击页签可折叠 / 展开命令区。", "RibbonRoot", GuidePlacementMode.Bottom);
        AddStep("② 导航树", "与 Ribbon 分组镜像；单击叶子即打开对应演示文档。", "NavTree", GuidePlacementMode.Right);
        AddStep("③ 文档区", "每个控件一个独立演示文档；文档头部可切换 演示 / XAML / C# 三段。", "DocumentTabs", GuidePlacementMode.Top);
        AddStep("④ PropertyGrid", "右侧属性面板驱动当前演示；无文档时展示全局外观 / 语言设置。", "PropertyPanelHost", GuidePlacementMode.Left);
        AddStep("⑤ 日志面板", "Logger 实时事件流；支持级别过滤、复制、清空与打开日志目录。", "LogPanelHost", GuidePlacementMode.Top);

        Logger.Info("AppGuide：开始演示台引导");
        AppGuide.Show();
    }

    private void OnStepOpened(object? sender, GuideStepEventArgs e) =>
        Logger.Info($"AppGuide：第 {e.Index + 1}/{AppGuide.StepCount} 步 · {AppGuide.CurrentTitle}");

    private static Control? FindDescendant(object root, string name)
    {
        Control? result = null;
        if (root is Visual visual)
        {
            foreach (var descendant in visual.GetVisualDescendants())
            {
                if (descendant is Control control
                    && control.Name == name)
                {
                    result = control;
                    break;
                }
            }
        }

        return result;
    }

    protected override IReadOnlyList<PropertyItem> BuildProperties()
    {
        var note = new PropertyItem
        {
            Id = "appguide.note",
            Name = "目标解析",
            Kind = PropertyItemKind.ReadOnly,
            Value = "RibbonRoot / NavTree / DocumentTabs / PropertyPanelHost / LogPanelHost",
            Description = "演示台五个区域控件名；跨控件树目标在代码中解析后传入 GuideStep.Target。",
        };

        return [note];
    }
}
