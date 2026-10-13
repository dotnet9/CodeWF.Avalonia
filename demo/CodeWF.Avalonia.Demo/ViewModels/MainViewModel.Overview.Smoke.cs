using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using CodeWF.Toolkit.Logging;

namespace CodeWF.Avalonia.Demo.ViewModels;

public partial class MainViewModel
{
    // Deterministic visual QA of the native application, independent of persisted user layout.
    public async Task CaptureStudioAsync(Window window, string directory)
    {
        Directory.CreateDirectory(directory);
        var overview = (Views.Pages.WelcomePage)Documents.First(d => d.Page is Views.Pages.WelcomePage).Page;
        // Dock creates document content after the window's Loaded event.
        var readyBy = DateTime.UtcNow.AddSeconds(10);
        while (!overview.IsLoaded || overview.Bounds.Width <= 0 || overview.Bounds.Height <= 0)
        {
            if (DateTime.UtcNow >= readyBy)
                throw new TimeoutException("Overview did not load before visual capture.");
            await Task.Delay(50);
            window.UpdateLayout();
        }
        foreach (var palette in Palettes.Take(3))
        {
            SelectedPalette = palette;
            foreach (var dark in new[] { true, false })
            {
                IsDarkMode = dark;
                overview.Overview.LabIndex = 0;
                await Task.Delay(350);
                window.UpdateLayout();
                Save("" + palette.TitleKey.Split('.').Last() + (dark ? "-dark" : "-light"));
            }
        }
        SelectedPalette = Palettes[0];
        IsDarkMode = true;
        for (var tab = 1; tab <= 2; tab++)
        {
            overview.Overview.LabIndex = tab;
            await Task.Delay(350);
            window.UpdateLayout();
            Save("lab-" + tab);
        }
        overview.Overview.LabIndex = 0;
        var scroll = overview.GetVisualDescendants().OfType<ScrollViewer>().First();
        scroll.Offset = new Vector(0, scroll.Extent.Height);
        await Task.Delay(250);
        window.UpdateLayout();
        Save("capabilities");
        scroll.Offset = default;
        SelectedCulture = Cultures[1];
        await Task.Delay(250);
        window.UpdateLayout();
        Save("english");
        SelectedCulture = Cultures[0];
        window.Width = 1120;
        window.Height = 640;
        await Task.Delay(250);
        window.UpdateLayout();
        Save("compact-window");
        window.Width = 1440;
        window.Height = 860;
        foreach (var key in new[] { DemoKeys.ItemMarkdownViewer, DemoKeys.ItemMarkdownEditor, DemoKeys.ItemMarkdownLiveEditor, DemoKeys.ItemMarkdownExport, DemoKeys.ItemPropertyGrid, DemoKeys.ItemLang })
        {
            overview.Overview.NavigateCommand.Execute(key);
            await Task.Delay(500);
            window.UpdateLayout();
            Save(key.Split('.').Last());
        }

        void Save(string name)
        {
            using var bitmap = new RenderTargetBitmap(new PixelSize((int)window.Bounds.Width, (int)window.Bounds.Height));
            bitmap.Render(window);
            bitmap.Save(Path.Combine(directory, name + ".png"), new PngBitmapEncoderOptions());
        }
    }

    private async Task<bool> SmokeOverviewAsync(CancellationToken cancellation)
    {
        try
        {
            foreach (var palette in Palettes.Take(3))
            {
                foreach (var dark in new[] { false, true })
                {
                    var variant = palette.ResolveVariant(dark);
                    Application.Current!.RequestedThemeVariant = variant;
                    if (!Application.Current.TryFindResource("StudioAccent", variant, out var brush) || brush is not IBrush)
                        throw new InvalidOperationException("Missing StudioAccent: " + variant.Key);
                }
            }
            var overview = (Views.Pages.WelcomePage)Documents.First(d => d.Page is Views.Pages.WelcomePage).Page;
            overview.Overview.Markdown = "# Smoke\n**live render**";
            await Task.Delay(50, cancellation);
            var sourceEditor = overview.GetVisualDescendants().OfType<global::CodeWF.Avalonia.Markdown.Editor.Controls.MarkdownEditorView>().Single();
            if (sourceEditor.Text != overview.Overview.Markdown || sourceEditor.Editor.Template is null)
                throw new InvalidOperationException("Markdown source binding or editor template was not applied.");
            if (overview.Overview.Rows.Count == 0) throw new InvalidOperationException("Empty natural-sort source");
            overview.Overview.Markdown = "# Hello, CodeWF\n\n- **Live preview** Markdown\n- Explore `DataGrid` and Dock";
            await SmokeMarkdownExportsAsync(cancellation);
            foreach (var key in new[] { DemoKeys.ItemMarkdownFlow, DemoKeys.ItemDataFlow, DemoKeys.ItemDocsFlow, DemoKeys.ItemTransfer, DemoKeys.ItemGuide, DemoKeys.ItemLog })
            {
                overview.Overview.NavigateCommand.Execute(key);
                if (ActiveDocument?.TitleKey != key) throw new InvalidOperationException("Overview navigation failed: " + key);
            }
            overview.Overview.NavigateCommand.Execute(DemoKeys.ItemWelcome);
            foreach (var culture in Cultures)
            {
                SelectedCulture = culture;
                await Task.Delay(30, cancellation);
                if (ActiveDocument?.DockDocument?.Title != global::CodeWF.Avalonia.Lang.I18nManager.Instance.GetResource(DemoKeys.ItemWelcome))
                    throw new InvalidOperationException("Dock title was not localized: " + culture.Name);
            }
            SelectedCulture = Cultures[0];
            Logger.Info("Overview smoke: theme resources, six capability routes, and interactions passed");
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            Logger.Error("Overview smoke failed", ex);
            return false;
        }
        finally { ApplyAppearance(); }
    }

    private static async Task SmokeMarkdownExportsAsync(CancellationToken cancellation)
    {
        var directory = Path.Combine(Path.GetTempPath(), "CodeWF.Markdown.Smoke", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            await Task.Run(() =>
            {
                const string markdown = "# Studio\n\n**Rendered** Markdown with `code`.\n\n| Package | Ready |\n| --- | --- |\n| Markdown | Yes |";
                foreach (var kind in new[] { global::CodeWF.Avalonia.Markdown.ExportKind.Png, global::CodeWF.Avalonia.Markdown.ExportKind.Pdf,
                             global::CodeWF.Avalonia.Markdown.ExportKind.Word, global::CodeWF.Avalonia.Markdown.ExportKind.Html })
                {
                    cancellation.ThrowIfCancellationRequested();
                    var path = Path.Combine(directory, kind.ToString());
                    global::CodeWF.Avalonia.Markdown.MarkdownDocumentExporter.ExportMarkdown(markdown, kind, path);
                    if (new FileInfo(path).Length == 0) throw new InvalidOperationException("Empty Markdown export: " + kind);
                }
                using var image = new Bitmap(Path.Combine(directory, "Png"));
                if (image.PixelSize.Width <= 0 || image.PixelSize.Height <= 0) throw new InvalidOperationException("Invalid PNG export.");
                var pdfHeader = File.ReadAllBytes(Path.Combine(directory, "Pdf")).Take(5).ToArray();
                if (System.Text.Encoding.ASCII.GetString(pdfHeader) != "%PDF-") throw new InvalidOperationException("Invalid PDF export.");
                using var word = System.IO.Compression.ZipFile.OpenRead(Path.Combine(directory, "Word"));
                if (word.GetEntry("word/document.xml") is null) throw new InvalidOperationException("Invalid Word export.");
                if (!File.ReadAllText(Path.Combine(directory, "Html")).Contains("Studio")) throw new InvalidOperationException("Invalid HTML export.");
            }, cancellation);
            Logger.Info("Markdown smoke: PNG decode, PDF signature, Word document and HTML export passed");
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
