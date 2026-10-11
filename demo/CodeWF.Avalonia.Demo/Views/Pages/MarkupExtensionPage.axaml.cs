using CodeWF.Avalonia.Demo.ViewModels.Pages;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class MarkupExtensionPage : DemoPageBase
{
    public const string XamlSnippet = """
        <!-- If：条件取值，条件/分支支持绑定与常量混合 -->
        <TextBlock.Text>
          <codewf:If Condition="{Binding IsBusy}"
                     True="任务执行中，请稍候…"
                     False="系统空闲" />
        </TextBlock.Text>

        <!-- Switch：多分支取值，未命中走 Default -->
        <TextBlock.Text>
          <codewf:Switch Value="{Binding State}" Default="未知状态">
            <codewf:SwitchCase When="运行中" Then="● 服务运行中" />
            <codewf:SwitchCase When="已暂停" Then="‖ 服务已暂停" />
          </codewf:Switch>
        </TextBlock.Text>
        """;

    public MarkupExtensionPage()
    {
        InitializeComponent();
        DataContext = new MarkupExtensionPageViewModel();
    }

    public override string TitleKey => "Demo.Page.MarkupExtension.Title";

    public override string DescriptionKey => "Demo.Page.MarkupExtension.Desc";

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => """
        // If / Switch 均有绑定形态（多绑定），供条件与分支都来自数据时使用：
        //   {codewf:IfBinding ...} / SwitchBinding
        //
        // 配套转换器可独立使用（CodeWF.Avalonia.Controls.Converters）：
        //   IfConverter / IfConditionConverter / SwitchConverter
        //   EnumToBooleanConverter / IsCollectionEmptyConverter
        """;

    protected override IReadOnlyList<PropertyItem> BuildProperties()
    {
        var note = new PropertyItem
        {
            Id = "mx.note",
            Name = "适用场景",
            Kind = PropertyItemKind.ReadOnly,
            Value = "纯展示型条件文案 / 图标切换",
            Description = "If / Switch 只解决“取值”；需要联动多属性或命令时仍建议在 VM 中处理。",
        };

        return [note];
    }
}
