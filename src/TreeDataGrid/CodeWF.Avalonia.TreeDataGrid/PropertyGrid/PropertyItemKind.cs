namespace CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

/// <summary>属性值编辑器类型，叶子项在值单元格中显示对应编辑器。</summary>
public enum PropertyItemKind
{
    /// <summary>占满整行的分组标题，可折叠子项。</summary>
    Category,

    /// <summary>单行文本，默认类型。</summary>
    Text,

    /// <summary>多行文本，失去焦点时提交，按 Esc 回退。</summary>
    MultilineText,

    /// <summary>数值编辑器，值类型为 decimal。</summary>
    Number,

    /// <summary>复选框，切换状态时提交。</summary>
    Boolean,

    /// <summary>下拉框，候选项来自 <see cref="PropertyItem.Options"/>。</summary>
    Choice,

    /// <summary>横向排列的单选按钮组。</summary>
    Radio,

    /// <summary>百分比滑块，拖动时预览、松开后提交，值类型为 double，范围为 0 至 100。</summary>
    Percent,

    /// <summary>颜色，值为十六进制颜色字符串。</summary>
    Color,

    /// <summary>路径输入框和浏览按钮，通过 BrowseRequested 处理浏览请求。</summary>
    Path,

    /// <summary>占满整行的按钮，单击触发 BrowseRequested。</summary>
    Button,

    /// <summary>日期时间，文本格式为 yyyy/MM/dd HH:mm，值类型为 DateTime。</summary>
    DateTime,

    /// <summary>只读显示。</summary>
    ReadOnly,
}
