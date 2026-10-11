using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class TrapezoidTabPage : DemoPageBase
{
    public const string XamlSnippet = """
        <!-- 梯形页签是 ControlTheme，套在普通 TabControl / TabStrip 上即可 -->
        <TabControl Theme="{StaticResource TrapezoidShapedTabControl}">
          <TabItem Header="首页" />
          <TabItem Header="设计" />
          <TabItem Header="发布" />
        </TabControl>

        <TabStrip Theme="{StaticResource TrapezoidShapedTabStrip}">
          <TabStripItem Content="概览" />
          <TabStripItem Content="明细" />
        </TabStrip>
        """;

    public const string CSharpSnippet = """
        // 路径绘制：TrapezoidShapedTabItemBorder（const DiagonalFilletRatio = 0.8）
        // Dock 包另有 TrapezoidShapedDocumentTabStripItemBorder，与文档页签视觉一致。
        //
        // 首 / 中 / 末自适应：主题内通过 TrapezoidShapedTabItemPaddingConverter
        // 与相邻页签关系自动绘制单侧 / 双侧斜边，无需逐个配置。
        tabControl.Theme = (ControlTheme)Application.Current.Resources["TrapezoidShapedTabControl"]!;
        """;

    public TrapezoidTabPage()
    {
        InitializeComponent();
    }

    public override string TitleKey => "Demo.Page.TrapezoidTab.Title";

    public override string DescriptionKey => "Demo.Page.TrapezoidTab.Desc";

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties()
    {
        var note = new PropertyItem
        {
            Id = "tt.note",
            Name = "主题键",
            Kind = PropertyItemKind.ReadOnly,
            Value = "TrapezoidShapedTabControl / TrapezoidShapedTabStrip / TrapezoidShapedTabItem",
            Description = "资源键均随 ACSemiTheme 注入，直接 StaticResource 引用。",
        };

        return [note];
    }
}
