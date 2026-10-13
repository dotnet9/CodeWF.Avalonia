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
    private string _markdown = L.Instance.GetResource("Demo.Text.MarkdownExportPageaxaml.SampleMarkdown004");
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
                _copyKind = kind == L.Instance.GetResource("Demo.Text.MarkdownExportPageaxaml.BuildProperties011") ? CopyKind.Zhihu
                    : kind == L.Instance.GetResource("Demo.Text.MarkdownExportPageaxaml.BuildProperties012") ? CopyKind.Juejin : CopyKind.Wechat;
                break;
        }
    }

    public IReadOnlyList<PropertyItem> BuildProperties()
    {
        var theme = new PropertyItem
        {
            Id = IdTheme,
            Name = L.Instance.GetResource("Demo.Text.MarkdownExportPageaxaml.BuildProperties005"),
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
            Description = L.Instance.GetResource("Demo.Text.MarkdownExportPageaxaml.BuildProperties006"),
        };
        var size = new PropertyItem
        {
            Id = IdSize,
            Name = L.Instance.GetResource("Demo.Text.MarkdownExportPageaxaml.BuildProperties007"),
            Kind = PropertyItemKind.Choice,
            Options = [MarkdownTypographySizes.Normal, MarkdownTypographySizes.Small],
            Value = _size,
            DefaultValue = MarkdownTypographySizes.Normal,
            Description = L.Instance.GetResource("Demo.Text.MarkdownExportPageaxaml.BuildProperties008"),
        };
        var copyKind = new PropertyItem
        {
            Id = IdCopyKind,
            Name = L.Instance.GetResource("Demo.Text.MarkdownExportPageaxaml.BuildProperties009"),
            Kind = PropertyItemKind.Radio,
            Options = [L.Instance.GetResource("Demo.Text.MarkdownExportPageaxaml.BuildProperties010"), L.Instance.GetResource("Demo.Text.MarkdownExportPageaxaml.BuildProperties011"), L.Instance.GetResource("Demo.Text.MarkdownExportPageaxaml.BuildProperties012")],
            Value = CopyTitle,
            DefaultValue = L.Instance.GetResource("Demo.Text.MarkdownExportPageaxaml.BuildProperties013"),
            Description = L.Instance.GetResource("Demo.Text.MarkdownExportPageaxaml.BuildProperties014"),
        };

        return [theme, size, copyKind];
    }


    private string CopyTitle => L.Instance.GetResource(_copyKind switch
    {
        CopyKind.Zhihu => "Demo.Text.MarkdownExportPageaxaml.BuildProperties011",
        CopyKind.Juejin => "Demo.Text.MarkdownExportPageaxaml.BuildProperties012",
        _ => "Demo.Text.MarkdownExportPageaxaml.BuildProperties010",
    });

    private async Task CopyAsync()
    {
        try
        {
            await MarkdownHtmlClipboard.SetHtmlAsync(Markdown, _copyKind, _theme, _size);
            Status = string.Format(L.Instance.GetResource("Demo.Text.MarkdownExportPageaxaml.OnCopyWechat021"), CopyTitle);
        }
        catch (Exception ex) { Status = L.Instance.GetResource("Demo.Text.MarkdownExportPageaxaml.OnCopyWechat022") + ex.Message; }
    }

    private async Task ExportAsync(ExportKind kind)
    {
        try
        {
            var extension = kind switch { ExportKind.Png => ".png", ExportKind.Pdf => ".pdf", ExportKind.Word => ".docx", _ => ".html" };
            var titleKey = kind switch
            {
                ExportKind.Png => "Demo.Text.MarkdownExportPageaxaml.OnExportPng015",
                ExportKind.Pdf => "Demo.Text.MarkdownExportPageaxaml.OnExportPdf016",
                ExportKind.Word => "Demo.Text.MarkdownExportPageaxaml.OnExportWord017",
                _ => "Demo.Text.MarkdownExportPageaxaml.OnExportHtml018",
            };
            var path = SelectOutput is null ? null : await SelectOutput(L.Instance.GetResource(titleKey), extension);
            if (path is null) return;
            var markdown = Markdown;
            var theme = _theme;
            var size = _size;
            Status = string.Format(L.Instance.GetResource("Demo.Text.MarkdownExportPageaxaml.ExportAsync023"), kind, path);
            await Task.Run(() => MarkdownDocumentExporter.ExportMarkdown(markdown, kind, theme, path, size));
            Status = string.Format(L.Instance.GetResource("Demo.Text.MarkdownExportPageaxaml.ExportAsync024"), path);
        }
        catch (Exception ex) { Status = string.Format(L.Instance.GetResource("Demo.Text.MarkdownExportPageaxaml.ExportAsync025"), ex.Message); }
    }
}
