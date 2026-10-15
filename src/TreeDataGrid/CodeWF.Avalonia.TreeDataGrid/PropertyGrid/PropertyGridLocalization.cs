using CodeWF.Toolkit.Lang.Json;

namespace CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

/// <summary>属性面板从库内嵌资源读取文案，跟随全局语言并允许宿主同键覆盖。</summary>
public static class PropertyGridLocalization
{
    private static readonly JsonLocalizer Resources = new(typeof(PropertyGridLocalization).Assembly);

    /// <summary>查询当前语言文本，缺失语言或资源键时由共享国际化引擎回退。</summary>
    public static string Get(string key) => Resources.GetResource(key);
}
