using Avalonia.Controls;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.Views;

/// <summary>
/// 演示页基类：每个控件一个独立演示页，自持 ShowMeCode 源码片段与右侧 PropertyGrid 的属性定义。
/// </summary>
public abstract class DemoPageBase : UserControl
{
    protected DemoPageBase() => Classes.Add("demo-page");

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

    public event EventHandler? PropertiesChanged;

    public virtual void RefreshProperties()
    {
        _properties = null;
        PropertiesChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>构建右侧属性面板定义；仅调用一次。</summary>
    protected virtual IReadOnlyList<PropertyItem> BuildProperties() => [];

    /// <summary>本场景页的工作流步骤标题键（原型里的轨道式步骤条）；为空表示这是普通组件页。</summary>
    public IReadOnlyList<string> SceneStepKeys { get; protected init; } = [];

    /// <summary>每个步骤对应要显示的组件页实例（与步骤一一对应，实例由场景页自身持有）。</summary>
    public IReadOnlyList<DemoPageBase> SceneStepPages { get; protected init; } = [];

    /// <summary>当前显示的场景步骤页；由场景页在步骤切换时写回。</summary>
    public DemoPageBase? ActiveScenePage { get; set; }

    /// <summary>属性提交回调：把右侧面板的修改反映到演示内容。</summary>
    public virtual void OnPropertyCommitted(PropertyValueChange change)
    {
    }

    /// <summary>文档首次打开时回调。</summary>
    public virtual void OnDocumentOpened()
    {
    }
}
