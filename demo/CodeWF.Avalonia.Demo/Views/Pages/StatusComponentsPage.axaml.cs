using CodeWF.Avalonia.Controls.Controls;
using CodeWF.Avalonia.Controls.Models;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class StatusComponentsPage : DemoPageBase
{
    private const string IdKind = "st.kind";
    private const string IdIconWidth = "st.iconWidth";

    public const string XamlSnippet = """
        <!-- 徽标：左签 + 右值；卡片：语义状态驱动伪类换肤（:debug/:info/:warn/:error/:fatal） -->
        <codewf:StatusBadge LeftText="Info" RightText="信息" />

        <codewf:StatusCard Kind="Warn"
                           TopText="WARN"
                           BottomText="磁盘空间不足 10%"
                           IconWidth="32" />
        """;

    public const string CSharpSnippet = """
        // StatusLabelKind：Debug / Info / Warn / Error / Fatal（内置浅深主题配色）
        var card = new StatusCard
        {
            Kind = StatusLabelKind.Warn,
            TopText = "WARN",
            BottomText = "磁盘空间不足 10%",
            IconWidth = 32,
        };

        // 运行时切状态：伪类与配色随 Kind 自动切换
        card.Kind = StatusLabelKind.Error;

        var badge = new StatusBadge { LeftText = "Build", RightText = "通过" };
        """;

    public StatusComponentsPage()
    {
        InitializeComponent();
    }


    public override string TitleKey => "Demo.Page.Status.Title";

    public override string DescriptionKey => "Demo.Page.Status.Desc";

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    public override void OnPropertyCommitted(PropertyValueChange change)
    {
        switch (change.Item.Id)
        {
            case IdKind when change.NewValue is string kind:
                DrivenCard.Kind = kind switch
                {
                    "信息" => StatusLabelKind.Info,
                    "警告" => StatusLabelKind.Warn,
                    "错误" => StatusLabelKind.Error,
                    "严重" => StatusLabelKind.Fatal,
                    _ => StatusLabelKind.Debug,
                };
                break;
            case IdIconWidth when change.NewValue is decimal width:
                DrivenCard.IconWidth = (double)width;
                break;
        }
    }

    protected override IReadOnlyList<PropertyItem> BuildProperties()
    {
        var kind = new PropertyItem
        {
            Id = IdKind,
            Name = "卡片状态",
            Kind = PropertyItemKind.Radio,
            Options = ["调试", "信息", "警告", "错误", "严重"],
            Value = "信息",
            DefaultValue = "信息",
            Description = "StatusLabelKind 五档语义状态，切换时卡片伪类与配色同步变化。",
        };
        var iconWidth = new PropertyItem
        {
            Id = IdIconWidth,
            Name = "图标宽度",
            Kind = PropertyItemKind.Number,
            Value = 32m,
            DefaultValue = 32m,
            Minimum = 16,
            Maximum = 64,
            Unit = "px",
            Description = "驱动卡片的图标宽度。",
        };

        return [kind, iconWidth];
    }
}
