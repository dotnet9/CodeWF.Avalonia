namespace CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

/// <summary>属性值的提交时机。</summary>
public enum PropertyCommitMode
{
    /// <summary>失去焦点或按回车时提交，按 Esc 回退；文本编辑器默认使用此模式。</summary>
    FocusLoss,

    /// <summary>每次输入后立即提交，无需等待失去焦点。</summary>
    Immediate,
}
