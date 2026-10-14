using L = global::CodeWF.Avalonia.Lang.I18nManager;
using System.Windows.Input;
using CodeWF.Avalonia.Markdown;
using CodeWF.Avalonia.Markdown.Themes;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using ReactiveUI;

namespace CodeWF.Avalonia.Demo.ViewModels.Pages;

public sealed class MarkdownExportPageViewModel : ReactiveObject
{
    private const string IdTheme = "mex.theme";
    private const string IdSize = "mex.size";
    private const string IdCopyKind = "mex.copykind";
    private string _theme = MarkdownTypographyThemes.WeChatFormat;
    private string _size = MarkdownTypographySizes.Normal;
    private CopyKind _copyKind = CopyKind.Wechat;
    private string _markdown = L.Instance.GetResource("Demo.Content.MarkdownExport.Samples.Markdown");
    private string _status = string.Empty;

    public MarkdownExportPageViewModel()
    {
        ExportCommand = ReactiveCommand.CreateFromTask<ExportKind>(ExportAsync);
        CopyCommand = ReactiveCommand.CreateFromTask(CopyAsync);
    }

    public ICommand ExportCommand { get; }
    public ICommand CopyCommand { get; }
    public Func<string, string, Task<string?>>? SelectOutput { private get; set; }
    public string Markdown { get => _markdown; set => this.RaiseAndSetIfChanged(ref _markdown, value); }
    public string Status { get => _status; private set => this.RaiseAndSetIfChanged(ref _status, value); }
    public void Commit(PropertyValueChange change)
    {
        switch (change.Item.Id)
        {
            case IdTheme when change.NewValue is string theme:
                _theme = theme;
                break;
            case IdSize when change.NewValue is string size:
                _size = size;
                break;
            case IdCopyKind when change.NewValue is string kind:
                _copyKind = kind == L.Instance.GetResource("Demo.Content.MarkdownExport.Properties.Zhihu") ? CopyKind.Zhihu
                    : kind == L.Instance.GetResource("Demo.Content.MarkdownExport.Properties.Juejin") ? CopyKind.Juejin : CopyKind.Wechat;
                break;
        }
    }

    public IReadOnlyList<PropertyItem> BuildProperties()
    {
        var theme = new PropertyItem
        {
            Id = IdTheme,
            Name = L.Instance.GetResource("Demo.Content.MarkdownExport.Properties.ExportTheme"),
            Kind = PropertyItemKind.Choice,
            Options =
            [
                MarkdownTypographyThemes.Basic,
                MarkdownTypographyThemes.Simple,
                MarkdownTypographyThemes.GeekBlack,
                MarkdownTypographyThemes.WeChatFormat,
            ],
            Value = _theme,
            DefaultValue = MarkdownTypographyThemes.WeChatFormat,
            Description = L.Instance.GetResource("Demo.Content.MarkdownExport.Properties.TypographyThemeOfTheExportedDocument"),
        };
        var size = new PropertyItem
        {
            Id = IdSize,
            Name = L.Instance.GetResource("Demo.Content.MarkdownExport.Properties.TypographySize"),
            Kind = PropertyItemKind.Choice,
            Options = [MarkdownTypographySizes.Normal, MarkdownTypographySizes.Small],
            Value = _size,
            DefaultValue = MarkdownTypographySizes.Normal,
            Description = L.Instance.GetResource("Demo.Content.MarkdownExport.Properties.TypographySizeOfTheExportedDocument"),
        };
        var copyKind = new PropertyItem
        {
            Id = IdCopyKind,
            Name = L.Instance.GetResource("Demo.Content.MarkdownExport.Properties.CopyDestination"),
            Kind = PropertyItemKind.Radio,
            Options = [L.Instance.GetResource("Demo.Content.MarkdownExport.Properties.WeChat"), L.Instance.GetResource("Demo.Content.MarkdownExport.Properties.Zhihu"), L.Instance.GetResource("Demo.Content.MarkdownExport.Properties.Juejin")],
            Value = CopyTitle,
            DefaultValue = L.Instance.GetResource("Demo.Content.MarkdownExport.Properties.WeChat"),
            Description = L.Instance.GetResource("Demo.Content.MarkdownExport.Properties.DestinationPlatformForRichText"),
        };

        return [theme, size, copyKind];
    }


    private string CopyTitle => L.Instance.GetResource(_copyKind switch
    {
        CopyKind.Zhihu => "Demo.Content.MarkdownExport.Properties.Zhihu",
        CopyKind.Juejin => "Demo.Content.MarkdownExport.Properties.Juejin",
        _ => "Demo.Content.MarkdownExport.Properties.WeChat",
    });

    private async Task CopyAsync()
    {
        try
        {
            await MarkdownHtmlClipboard.SetHtmlAsync(Markdown, _copyKind, _theme, _size);
            Status = string.Format(L.Instance.GetResource("Demo.Content.MarkdownExport.OnCopyWechat.CopiedRichTextPasteItIntoTheEditorFormat"), CopyTitle);
        }
        catch (Exception ex) { Status = L.Instance.GetResource("Demo.Content.MarkdownExport.OnCopyWechat.CopyFailed") + ex.Message; }
    }

    private async Task ExportAsync(ExportKind kind)
    {
        try
        {
            var extension = kind switch { ExportKind.Png => ".png", ExportKind.Pdf => ".pdf", ExportKind.Word => ".docx", _ => ".html" };
            var titleKey = kind switch
            {
                ExportKind.Png => "Demo.Content.MarkdownExport.OnExportPng.ExportPNG",
                ExportKind.Pdf => "Demo.Content.MarkdownExport.OnExportPdf.ExportPDF",
                ExportKind.Word => "Demo.Content.MarkdownExport.OnExportWord.ExportWord",
                _ => "Demo.Content.MarkdownExport.OnExportHtml.ExportHTML",
            };
            var path = SelectOutput is null ? null : await SelectOutput(L.Instance.GetResource(titleKey), extension);
            if (path is null) return;
            var markdown = Markdown;
            var theme = _theme;
            var size = _size;
            Status = string.Format(L.Instance.GetResource("Demo.Content.MarkdownExport.ExportAsync.ExportingFormat"), kind, path);
            await Task.Run(() => MarkdownDocumentExporter.ExportMarkdown(markdown, kind, theme, path, size));
            Status = string.Format(L.Instance.GetResource("Demo.Content.MarkdownExport.ExportAsync.ExportCompleteFormat"), path);
        }
        catch (Exception ex) { Status = string.Format(L.Instance.GetResource("Demo.Content.MarkdownExport.ExportAsync.ExportFailedFormat"), ex.Message); }
    }
}
