using CodeWF.Avalonia.Content.Tests.Markdown.Rendering;
using CodeWF.Avalonia.Markdown.Editor.Controls;
using CodeWF.Avalonia.Markdown.Editor.Controls.Wysiwyg;
using CodeWF.Avalonia.Markdown.Editor.Services;
using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Primitives;
using global::Avalonia.Controls.Presenters;
using global::Avalonia.Data;
using global::Avalonia.Headless;
using global::Avalonia.Input;
using global::Avalonia.Input.Raw;
using global::Avalonia.LogicalTree;
using global::Avalonia.Markup.Xaml.Styling;
using global::Avalonia.Media;
using global::Avalonia.Styling;
using global::Avalonia.Threading;
using global::Avalonia.VisualTree;
using AvaloniaEdit;
using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Editor;

[Collection("AvaloniaPlatform")]
public sealed class MarkdownSelectionToolbarTests(AvaloniaPlatformFixture platform)
{
    [Theory]
    [InlineData(false, false, 640, false)]
    [InlineData(false, true, 1100, false)]
    [InlineData(false, false, 640, true)]
    [InlineData(false, true, 1100, true)]
    [InlineData(true, false, 640, true)]
    [InlineData(true, true, 1100, true)]
    public void RealSelectionOpensToolbarAndPickerCommitsOnlyOnApply(bool source, bool dark, int width, bool keyboard) => platform.Run(() =>
    {
        const string text = "Target sample text";
        var styles = global::Avalonia.Application.Current!.Styles;
        var semi = new global::Semi.Avalonia.SemiTheme();
        var full = new global::CodeWF.Avalonia.Markdown.Themes.MarkdownFullThemes();
        var edit = new StyleInclude(new Uri("avares://CodeWF.Avalonia.Content.Tests/"))
            { Source = new Uri("avares://AvaloniaEdit/Themes/Simple/AvaloniaEdit.xaml") };
        styles.Add(semi); styles.Add(edit); styles.Add(full);
        var live = source ? null : new MarkdownLiveEditorView { HeaderText = null, ChipText = null };
        var code = source ? new MarkdownEditorView() : null;
        live?.SetText(text); code?.SetText(text);
        var window = new Window { Width = width, Height = 500, Content = (Control?)live ?? code,
            RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light };
        window.Resources["FontFamilyNormal"] = FontFamily.Default;
        window.Resources["FontSizeNormal"] = 14d;
        window.Resources["FontWeightNormal"] = FontWeight.Normal;
        window.Show();
        window.Activate();
        void Layout() { Dispatcher.UIThread.RunJobs(); window.UpdateLayout(); AvaloniaHeadlessPlatform.ForceRenderTimerTick(); Dispatcher.UIThread.RunJobs(); }
        void Click(Control button)
        {
            var point = button.TranslatePoint(new Point(button.Bounds.Width / 2, button.Bounds.Height / 2), window)!.Value;
            window.MouseMove(point); window.MouseDown(point, MouseButton.Left); window.MouseUp(point, MouseButton.Left); Layout();
        }
        string CurrentText() => live?.Text ?? code!.Text;
        void Select()
        {
            if (live is not null)
            {
                live.FocusBlock(0); Layout();
                var block = live.GetVisualDescendants().OfType<MarkdownBlockView>().Single();
                var input = block.ActiveEditor!;
                Assert.True(input.Focus(), $"active={block.IsEditing},root={TopLevel.GetTopLevel(input)},visible={input.IsEffectivelyVisible},focusable={input.Focusable},enabled={input.IsEffectivelyEnabled},parent={input.GetVisualParent()?.GetType().Name}");
                if (keyboard)
                {
                    input.CaretIndex = input.SelectionStart = input.SelectionEnd = 0;
                    var mods = KeyModifiers.None;
                    input.AddHandler(InputElement.KeyDownEvent, (_, e) => mods = e.KeyModifiers, global::Avalonia.Interactivity.RoutingStrategies.Tunnel);
                    for (var i = 0; i < 6; i++) window.KeyPress(Key.Right, RawInputModifiers.Shift, PhysicalKey.ArrowRight, null);
                    var beforeRelease = $"{input.SelectionStart}:{input.SelectionEnd}";
                    window.KeyRelease(Key.Right, RawInputModifiers.Shift, PhysicalKey.ArrowRight, null); Layout();
                    Assert.True(live.SelectedText == "Target", $"text={live.SelectedText},selection={input.SelectionStart}:{input.SelectionEnd},before={beforeRelease},caret={input.CaretIndex},mods={mods},active={block.IsEditing},focus={window.FocusManager?.GetFocusedElement()?.GetType().Name}");
                    return;
                }
                var presenter = input.GetVisualDescendants().OfType<TextPresenter>().Single();
                var start = presenter.TranslatePoint(presenter.TextLayout.HitTestTextPosition(0).TopLeft + new Vector(1, 10), window)!.Value;
                var end = presenter.TranslatePoint(presenter.TextLayout.HitTestTextPosition(6).TopLeft + new Vector(1, 10), window)!.Value;
                string Trace() => $"sel={input.SelectionStart}:{input.SelectionEnd},editing={block.IsEditing},focus={window.FocusManager?.GetFocusedElement()?.GetType().Name}";
                window.MouseMove(start); window.MouseDown(start, MouseButton.Left); var down = Trace();
                window.MouseMove(end, RawInputModifiers.LeftMouseButton); var move = Trace(); window.MouseUp(end, MouseButton.Left); var up = Trace(); Layout();
                Assert.False(string.IsNullOrEmpty(live.SelectedText), $"live start={start},end={end},down={down},move={move},up={up},after={Trace()}");
            }
            else
            {
                var editor = code!.GetVisualDescendants().OfType<TextEditor>().Single();
                Assert.True(editor.TextArea.Focus()); editor.TextArea.Caret.Offset = 0;
                for (var i = 0; i < 6; i++) window.KeyPress(Key.Right, RawInputModifiers.Shift, PhysicalKey.ArrowRight, null);
                window.KeyRelease(Key.Right, RawInputModifiers.Shift, PhysicalKey.ArrowRight, null); Layout();
                Assert.False(string.IsNullOrEmpty(code!.SelectedText), $"source focus={editor.TextArea.IsKeyboardFocusWithin},caret={editor.CaretOffset},text={editor.Text}");
            }
        }
        try
        {
            Layout(); Select();
            Assert.True(window.GetVisualDescendants().OfType<MarkdownSelectionToolbar>().Any(),
                $"source={source} selected={live?.SelectedText ?? code!.SelectedText} popups={string.Join(';', ((Control?)live ?? code!).GetLogicalDescendants().OfType<Popup>().Select(p => p.IsOpen + " parent=" + p.Child?.GetVisualParent()?.GetType().Name))}");
            var bar = Assert.Single(window.GetVisualDescendants().OfType<MarkdownSelectionToolbar>());
            var bounds = new Rect(bar.TranslatePoint(default, window)!.Value, bar.Bounds.Size);
            Assert.True(bounds.Left >= 0 && bounds.Right <= window.Bounds.Width && bounds.Top >= 0 && bounds.Bottom <= window.Bounds.Height, bounds.ToString());
            Assert.Equal(12, bar.GetVisualDescendants().OfType<Button>().Count());
            Assert.All(bar.GetVisualDescendants().OfType<Button>(), button => Assert.Equal(30, button.Bounds.Height));
            Click(bar.FindControl<Button>("ForegroundButton")!);
            var picker = Assert.Single(window.GetVisualDescendants().OfType<MarkdownSpanStylePicker>());
            var model = Assert.IsType<MarkdownSpanStylePickerViewModel>(picker.DataContext);
            Assert.NotNull(picker.FindControl<ColorSpectrum>("Spectrum")!.Template);
            Assert.Equal(text, CurrentText());
            var hex = picker.FindControl<TextBox>("HexInput")!;
            Click(hex); hex.Text = "#30953B"; Layout();
            Assert.Equal(text, CurrentText());
            Click(picker.FindControl<Button>("ApplyColor")!);
            Assert.Contains("color:#30953B", CurrentText(), StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Target", CurrentText());
            Assert.DoesNotContain(window.GetVisualDescendants(), control => control is MarkdownSelectionToolbar or MarkdownSpanStylePicker);
            (live is not null ? live.ExecuteAsync(MarkdownEditorAction.Undo) : code!.ExecuteAsync(MarkdownEditorAction.Undo)).GetAwaiter().GetResult();
            Assert.Equal(text, CurrentText());
            Select();
            bar = Assert.Single(window.GetVisualDescendants().OfType<MarkdownSelectionToolbar>());
            Click(bar.FindControl<Button>("FontSizeButton")!);
            picker = Assert.Single(window.GetVisualDescendants().OfType<MarkdownSpanStylePicker>());
            var size = picker.GetVisualDescendants().OfType<Button>().Single(button => button.Content is double value && value == 22);
            Click(size);
            Assert.Contains("font-size:22px", CurrentText());
            (live is not null ? live.ExecuteAsync(MarkdownEditorAction.Undo) : code!.ExecuteAsync(MarkdownEditorAction.Undo)).GetAwaiter().GetResult();
            Select();
            bar = Assert.Single(window.GetVisualDescendants().OfType<MarkdownSelectionToolbar>());
            Click(bar.FindControl<Button>("BackgroundButton")!);
            picker = Assert.Single(window.GetVisualDescendants().OfType<MarkdownSpanStylePicker>());
            picker.FindControl<TextBox>("HexInput")!.Text = "#FF0000";
            window.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, null);
            window.KeyRelease(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, null); Layout();
            Assert.Equal(text, CurrentText());
        }
        finally { window.Close(); styles.Remove(full); styles.Remove(edit); styles.Remove(semi); }
    });

    [Fact]
    public void HostFlyoutBindsOnFirstOpenRefreshesAndClosesOnCommit() => platform.Run(() =>
    {
        var styles = global::Avalonia.Application.Current!.Styles;
        var semi = new global::Semi.Avalonia.SemiTheme();
        var full = new CodeWF.Avalonia.Markdown.Themes.MarkdownFullThemes();
        styles.Add(semi); styles.Add(full);
        var current = "#C92A2A";
        var changes = new List<string>();
        var model = new MarkdownSpanStylePickerViewModel("color", (_, value) => changes.Add(value), () => { }, _ => current);
        var picker = new MarkdownSpanStylePicker();
        picker.Bind(StyledElement.DataContextProperty, new Binding("Picker"));
        var flyout = new MarkdownSpanStyleFlyout { Content = picker };
        var button = new Button { Flyout = flyout };
        var window = new Window { Width = 640, Height = 500, Content = button, DataContext = new PickerHost(model) };
        window.Show(); Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
        try
        {
            Assert.Null(picker.DataContext);
            current = "#30953B";
            flyout.ShowAt(button); Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
            Assert.Same(model, picker.DataContext);
            Assert.Equal("#30953B", model.HexValue);
            model.HexValue = "#1971C2"; model.ApplyCommand.Execute(null); Dispatcher.UIThread.RunJobs();
            Assert.Equal(new[] { "#1971C2" }, changes);
            Assert.False(flyout.IsOpen);
            current = "#C92A2A";
            flyout.ShowAt(button); Dispatcher.UIThread.RunJobs();
            Assert.Equal("#C92A2A", model.HexValue);
            model.HexValue = "#FFFFFF"; flyout.Hide();
            Assert.Single(changes);
        }
        finally { window.Close(); styles.Remove(full); styles.Remove(semi); }
    });

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SelectionToolbarAllowsTheNextRightClickToOpenAStandardMenu(bool source) => platform.Run(() =>
    {
        var styles = global::Avalonia.Application.Current!.Styles;
        var semi = new global::Semi.Avalonia.SemiTheme();
        var full = new CodeWF.Avalonia.Markdown.Themes.MarkdownFullThemes();
        var edit = new StyleInclude(new Uri("avares://AvaloniaEdit/")) { Source = new Uri("avares://AvaloniaEdit/Themes/Simple/AvaloniaEdit.xaml") };
        styles.Add(semi); styles.Add(edit); styles.Add(full);
        var live = source ? null : new MarkdownLiveEditorView();
        var code = source ? new MarkdownEditorView() : null;
        live?.SetText("Target sample"); code?.SetText("Target sample");
        var window = new Window { Width = 640, Height = 500, Content = (Control?)live ?? code };
        void Layout() { Dispatcher.UIThread.RunJobs(); window.UpdateLayout(); AvaloniaHeadlessPlatform.ForceRenderTimerTick(); Dispatcher.UIThread.RunJobs(); }
        window.Show(); window.Activate(); Layout();
        try
        {
            Control input;
            if (source) { input = code!.Editor.TextArea; code.Editor.TextArea.Focus(); code.Editor.CaretOffset = 0; }
            else { live!.FocusBlock(0); Layout(); var box = live.GetVisualDescendants().OfType<TextBox>().Single(box => box.IsEffectivelyVisible); input = box; box.Focus(); box.CaretIndex = 0; }
            var menu = new ContextMenu { Items = { new MenuItem { Header = "Copy" } } };
            input.ContextFlyout = null; input.ContextMenu = menu;
            var point = input.TranslatePoint(new Point(36, 12), window)!.Value;
            for (var i = 0; i < 6; i++) window.KeyPress(Key.Right, RawInputModifiers.Shift, PhysicalKey.ArrowRight, null);
            window.KeyRelease(Key.Right, RawInputModifiers.Shift, PhysicalKey.ArrowRight, null); Layout();
            var bar = Assert.Single(window.GetVisualDescendants().OfType<MarkdownSelectionToolbar>());
            var detail = $"source={source},point={point},bar={bar.TranslatePoint(default, window)}/{bar.Bounds.Size},hit={window.InputHitTest(point)?.GetType().Name}";
            window.MouseMove(point); window.MouseDown(point, MouseButton.Right); window.MouseUp(point, MouseButton.Right); Layout();
            Assert.True(menu.IsOpen, detail);
            Assert.Empty(window.GetVisualDescendants().OfType<MarkdownSelectionToolbar>());
            menu.Close();
        }
        finally { window.Close(); styles.Remove(full); styles.Remove(edit); styles.Remove(semi); }
    });

    public sealed record PickerHost(MarkdownSpanStylePickerViewModel Picker);

    [Fact]
    public void PickerValidationCancelAndSameColorNeverMutateDocument() => platform.Run(() =>
    {
        var changes = new List<string>();
        var model = new MarkdownSpanStylePickerViewModel("color", (property, value) => changes.Add(value), () => changes.Add("clear"), _ => "#30953B");
        model.SelectedHsv = Colors.Red.ToHsv();
        Assert.Empty(changes);
        model.HexValue = "invalid";
        Assert.False(model.ApplyCommand.CanExecute(null));
        model.ApplyCommand.Execute(null);
        Assert.Empty(changes);
        model.Prepare(); model.ApplyCommand.Execute(null);
        Assert.Empty(changes);
        model.ClearCommand.Execute(null);
        Assert.Equal(new[] { "clear" }, changes);
    });
}
