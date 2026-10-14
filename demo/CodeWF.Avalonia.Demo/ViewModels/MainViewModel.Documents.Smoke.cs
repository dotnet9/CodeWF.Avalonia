using Avalonia.Controls;
using Avalonia.VisualTree;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using CodeWF.Toolkit.Logging;
using Dock.Model.Core;

namespace CodeWF.Avalonia.Demo.ViewModels;

public partial class MainViewModel
{
    /// <summary>将真实文档浮动、调整参数后重新停靠，验证内容与属性面板不会串到其他文档。</summary>
    private async Task SmokeDocumentIsolationAsync(CancellationToken cancellation)
    {
        var item = Groups.SelectMany(group => group.Items).Single(entry => entry.Api == "CodeWFTitleBar");
        OpenItem(item);
        var document = ActiveDocument!;
        var dockable = document.DockDocument!;
        var view = (Views.DemoDocumentView)dockable.Context!;
        var main = await WaitForWindowAsync(view);
        var propertyGrid = view.FindControl<PropertyGrid>("DocumentProperties") ?? throw new InvalidOperationException("Document properties missing.");
        if (!ReferenceEquals(propertyGrid.Items, document.PropertyItems)) throw new InvalidOperationException("Properties are not owned by the document.");
        _dockFactory!.FloatDockable(dockable);
        var floating = await WaitForWindowAsync(view, main);
        try
        {
            floating.Width = 900;
            floating.Height = 640;
            OpenItem(Groups[0].Items[0]);
            if (!ReferenceEquals(TopLevel.GetTopLevel(propertyGrid), floating)) throw new InvalidOperationException("Properties did not float with the document.");
            var parameter = document.PropertyItems.Single(value => value.Id == "Title");
            var changed = "Detached title";
            if (!propertyGrid.CommitValue(parameter, changed)) throw new InvalidOperationException("Floating property commit was rejected.");
            if (((Views.Pages.ControlApiPage)document.Page).ViewModel.Title != changed) throw new InvalidOperationException("Floating title parameter was not applied.");
            if (!ReferenceEquals(propertyGrid.Items, document.PropertyItems)) throw new InvalidOperationException("Changing the active main document rebound floating properties.");
            document.ContentColumns[2].Width = new global::Avalonia.Controls.GridLength(310);
            await Task.Delay(80, cancellation);
            floating.UpdateLayout();
            if (propertyGrid.Bounds.Width < 250 || view.FindControl<GridSplitter>("DocumentSplitter")?.Bounds.Width is not > 0)
                throw new InvalidOperationException("The document splitter or property layout did not resize.");
            document.PropertiesVisible = false;
            if (document.ContentColumns[2].Width.Value != 0) throw new InvalidOperationException("Document property collapse failed.");
            document.PropertiesVisible = true;
            if (document.ContentColumns[2].Width.Value != 310) throw new InvalidOperationException("Document property width was not restored.");
            Logger.Info("Document isolation smoke: floating content and properties, local parameter update, main-document isolation and splitter width passed");
        }
        finally
        {
            if (dockable.Owner is IDock owner && !ReferenceEquals(owner, _docDock))
                _dockFactory!.MoveDockable(owner, _docDock!, dockable, null);
        }
        await WaitForWindowAsync(view);
        CloseDocument(document);

        async Task<Window> WaitForWindowAsync(Control control, Window? excluded = null)
        {
            var deadline = DateTime.UtcNow.AddSeconds(8);
            while (TopLevel.GetTopLevel(control) is not Window window || ReferenceEquals(window, excluded) || control.Bounds.Width <= 0)
            {
                await Task.Delay(40, cancellation);
                if (DateTime.UtcNow >= deadline) throw new TimeoutException("Dock document did not attach to the expected window.");
            }
            return (Window)TopLevel.GetTopLevel(control)!;
        }
    }
}
