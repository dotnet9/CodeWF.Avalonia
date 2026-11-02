using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Primitives;
using global::Avalonia.Input;
using global::Avalonia.Interactivity;
using global::Avalonia.Threading;
using CodeWF.Avalonia.Markdown.Editor.Services;

namespace CodeWF.Avalonia.Markdown.Editor.Controls;

public partial class MarkdownEditorView
{
    private Popup? _selectionBarPopup;

    private void InitializeSelectionToolbar()
    {
        _editor.AddHandler(PointerPressedEvent, (_, _) => HideSelectionToolbar(), RoutingStrategies.Tunnel, true);
        _editor.AddHandler(PointerReleasedEvent, (_, e) =>
        {
            if (e.InitialPressMouseButton == MouseButton.Left) Dispatcher.UIThread.Post(ShowSelectionToolbar);
        }, RoutingStrategies.Bubble, true);
        _editor.AddHandler(KeyUpEvent, (_, e) =>
        {
            if (e.Key == Key.Escape) HideSelectionToolbar();
            else Dispatcher.UIThread.Post(ShowSelectionToolbar);
        }, RoutingStrategies.Bubble, true);
        _editor.TextChanged += (_, _) => HideSelectionToolbar();
        _editor.TextArea.TextView.ScrollOffsetChanged += (_, _) => HideSelectionToolbar();
        DetachedFromVisualTree += (_, _) => HideSelectionToolbar();
    }

    private void ShowSelectionToolbar()
    {
        if (_editor.SelectionLength == 0 || !_editor.TextArea.IsKeyboardFocusWithin) { HideSelectionToolbar(); return; }
        if (_selectionBarPopup is null)
        {
            _selectionBarPopup = new Popup
            {
                PlacementTarget = this, Placement = PlacementMode.Top,
                IsLightDismissEnabled = true, OverlayDismissEventPassThrough = true, OverlayInputPassThroughElement = this, ShouldUseOverlayLayer = true,
                Child = new MarkdownSelectionToolbar
                {
                    DataContext = new MarkdownSelectionToolbarViewModel(action => _ = ExecuteAsync(action),
                        ApplySpanStyle, GetSpanStyle, () => _ = ExecuteAsync(MarkdownEditorAction.ClearSpanStyles), HideSelectionToolbar)
                }
            };
            LogicalChildren.Add(_selectionBarPopup);
        }
        var position = _editor.TextArea.Caret.CalculateCaretRectangle();
        var point = _editor.TextArea.TextView.TranslatePoint(position.TopLeft, this) ?? default;
        _selectionBarPopup.PlacementRect = new Rect(point, new Size(1, Math.Max(1, position.Height))).Inflate(new Thickness(0, 8));
        _selectionBarPopup.IsOpen = true;
    }

    private void HideSelectionToolbar()
    {
        if (_selectionBarPopup is { IsOpen: true }) _selectionBarPopup.IsOpen = false;
    }
}
