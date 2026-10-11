using CodeWF.Avalonia.Demo.ViewModels.Pages;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class ColumnDisplayEditorPage : DemoPageBase
{
    public const string XamlSnippet = """
        <!-- 拖拽排序直接 Move 源集合；Visible 绑定到 ColumnDisplayItem -->
        <codewf:ColumnDisplayEditor ItemsSource="{Binding Columns}"
                                    VisibleHeaderText="显示"
                                    DisplayTextHeaderText="列名" />
        """;

    public const string CSharpSnippet = """
        var columns = new ObservableCollection<ColumnDisplayItem>
        {
            new("name", "名称"),
            new("status", "状态"),
            new("remark", "备注", visible: false),
        };

        var editor = new ColumnDisplayEditor { ItemsSource = columns };

        // 业务侧监听即可获得排序 / 可见性变更（编辑器直接操作源集合）
        columns.CollectionChanged += (_, e) =>
        {
            if (e.Action == NotifyCollectionChangedAction.Move)
            {
                Save(columns);   // 顺序已实时变更
            }
        };
        columns[0].PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ColumnDisplayItem.Visible))
            {
                Save(columns);
            }
        };

        // 持久化恢复：按保存的 Key 顺序与 Visible 重建集合
        """;

    public ColumnDisplayEditorPage()
    {
        InitializeComponent();
        DataContext = new ColumnDisplayEditorPageViewModel();
    }

    public override string TitleKey => "Demo.Page.ColumnDisplayEditor.Title";

    public override string DescriptionKey => "Demo.Page.ColumnDisplayEditor.Desc";

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties()
    {
        var note = new PropertyItem
        {
            Id = "cde.note",
            Name = "数据契约",
            Kind = PropertyItemKind.ReadOnly,
            Value = "IList<ColumnDisplayItem>（Key / DisplayText / Visible）",
            Description = "编辑器要求 ItemsSource 为 IList；拖拽排序通过 IList.Move 完成。",
        };

        return [note];
    }
}
