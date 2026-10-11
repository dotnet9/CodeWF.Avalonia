using Avalonia.Controls;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.Views;

/// <summary>
/// 演示页基类：每个控件一个独立演示页，自持 ShowMeCode 源码片段与右侧 PropertyGrid 的属性定义。
/// </summary>
public abstract class DemoPageBase : UserControl
{
    private IReadOnlyList<PropertyItem>? _properties;

    /// <summary>标题的 Lang 键。</summary>
    public abstract string TitleKey { get; }

    /// <summary>说明文字的 Lang 键。</summary>
    public abstract string DescriptionKey { get; }

    /// <summary>ShowMeCode 的 XAML 片段。</summary>
    public abstract string XamlCode { get; }

    /// <summary>ShowMeCode 的 C# 片段。</summary>
    public virtual string CSharpCode => string.Empty;

    /// <summary>右侧 PropertyGrid 的属性定义（首次访问构建并缓存）。</summary>
    public IReadOnlyList<PropertyItem> Properties => _properties ??= BuildProperties();

    /// <summary>构建右侧属性面板定义；仅调用一次。</summary>
    protected virtual IReadOnlyList<PropertyItem> BuildProperties() => [];

    /// <summary>属性提交回调：把右侧面板的修改反映到演示内容。</summary>
    public virtual void OnPropertyCommitted(PropertyValueChange change)
    {
    }

    /// <summary>文档首次打开时回调。</summary>
    public virtual void OnDocumentOpened()
    {
    }
}
