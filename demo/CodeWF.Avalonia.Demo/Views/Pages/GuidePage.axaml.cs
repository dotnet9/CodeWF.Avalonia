using Avalonia.Interactivity;
using CodeWF.Avalonia.Controls.Controls;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using CodeWF.Toolkit.Logging;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class GuidePage : DemoPageBase
{
    public const string XamlSnippet = """
        <!-- 引导控件：Steps 为内容属性，目标用元素名绑定 -->
        <codewf:Guide IsArrowVisible="True" Completed="OnGuideCompleted">
          <codewf:GuideStep Title="步骤 1"
                            Description="气泡出现在目标的右上方"
                            Target="{Binding #TargetControl}"
                            Placement="RightTop" />
          <codewf:GuideStep Title="步骤 2"
                            Description="Primary 样式 + 自定义遮罩色"
                            Target="{Binding #TargetControl2}"
                            Placement="Bottom"
                            StyleType="Primary"
                            MaskColor="#66B3261E" />
        </codewf:Guide>
        """;

    public const string CSharpSnippet = """
        var guide = new Guide
        {
            IsArrowVisible = true,
            MissingTargetBehavior = GuideMissingTargetBehavior.Center, // 缺省目标：居中/跳过/关闭
        };
        guide.Steps.Add(new GuideStep
        {
            Title = "第一步",
            Description = "聚光目标控件",
            Target = someControl,
            Placement = GuidePlacementMode.RightTop,
            OpeningCommand = prepareCommand,     // 步骤打开前命令
        });
        guide.StepOpened += (_, e) => Logger.Info($"进入第 {e.Index} 步");
        guide.Completed += (_, _) => Logger.Info("引导完成");
        guide.Show();      // 键盘导航：Enter 下一 / Esc 关闭
        guide.GoNext();
        guide.Close();
        """;

    public GuidePage()
    {
        InitializeComponent();
        Guide.StepOpened += OnStepOpened;
    }


    public override string TitleKey => "Demo.Page.Guide.Title";

    public override string DescriptionKey => "Demo.Page.Guide.Desc";

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    private void OnStartGuide(object? sender, RoutedEventArgs e)
    {
        Logger.Info("Guide：开始引导演示");
        Guide.ResetState();
        Guide.Show();
    }

    private void OnStepOpened(object? sender, GuideStepEventArgs e) =>
        Logger.Info($"Guide：第 {e.Index + 1}/{Guide.StepCount} 步 · {Guide.CurrentTitle}");

    private void OnGuideCompleted(object? sender, System.EventArgs e) =>
        Logger.Info("Guide：引导完成");

    protected override IReadOnlyList<PropertyItem> BuildProperties()
    {
        var placement = new PropertyItem
        {
            Id = "guide.info",
            Name = "方位说明",
            Kind = PropertyItemKind.ReadOnly,
            Value = "Center / Top / TopLeft / TopRight / Left / LeftTop / LeftBottom / Right / RightTop / RightBottom / Bottom / BottomLeft / BottomRight",
            Description = "13 方位由每一步的 Placement 指定，覆盖全部气泡落位组合。",
        };

        return [placement];
    }
}
