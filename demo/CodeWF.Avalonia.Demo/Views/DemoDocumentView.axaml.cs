using CodeWF.Avalonia.Demo.ViewModels;
using Avalonia.Controls;

namespace CodeWF.Avalonia.Demo.Views;

/// <summary>
/// 演示文档视图：演示始终显示，源码面板按需展开。
/// </summary>
public partial class DemoDocumentView : UserControl
{
    public DemoDocumentView()
    {
        InitializeComponent();
        // Grid 的列集合不支持绑定，仅在此连接视图模型持有的布局定义。
        DataContextChanged += (_, _) =>
        {
            if (DataContext is DemoDocumentViewModel document) DocumentWorkspace.ColumnDefinitions = document.ContentColumns;
        };
    }
}
