namespace CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

/// <summary>
/// When property values are committed./// </summary>
public enum PropertyCommitMode
{
    /// <summary>Commit on focus loss / Enter, Esc reverts (default for text editors).</summary>
    FocusLoss,

    /// <summary>Commit on every input (no focus-loss wait).</summary>
    Immediate,
}
