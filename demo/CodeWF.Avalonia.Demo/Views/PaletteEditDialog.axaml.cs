using Avalonia.Controls;
using Avalonia.Media;
using CodeWF.Avalonia.Demo.I18n;
using CodeWF.Avalonia.Demo.ViewModels;
using CodeWF.Toolkit.Lang;

namespace CodeWF.Avalonia.Demo.Views;

/// <summary>对话框内一行的可编辑颜色字段。</summary>
public sealed class ColorField
{
    public ColorField(string label, string color) { Label = label; Color = color; }
    public string Label { get; }
    public string Color { get; set; } = string.Empty;
}

/// <summary>新增/编辑用户配色：收集名称与编写层 8 基色（浅/深 × 纸面/面板/墨色/强调）。</summary>
public partial class PaletteEditDialog : Window
{
    private readonly UserPaletteDraft _draft;
    private readonly System.Collections.Generic.List<ColorField> _fields;
    private bool _submitted;

    public PaletteEditDialog(UserPaletteDraft draft)
    {
        _draft = draft;
        InitializeComponent();
        TitleText.Text = string.IsNullOrEmpty(draft.Key)
            ? L.Instance.GetResource(ResourceKeys.Shell.AddPalette)
            : L.Instance.GetResource(ResourceKeys.Shell.EditPalette);
        _fields = draft.Labels.Zip(draft.Colors, (label, color) => new ColorField(label, color)).ToList();
        ColorInputs.ItemsSource = _fields;
        OkButton.Click += (_, _) =>
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(_draft.Name.Trim(), "\\S")
                || _fields.Any(field => !System.Text.RegularExpressions.Regex.IsMatch(field.Color, "^#[0-9a-fA-F]{6}$")))
            {
                TitleText.Opacity = 0.45;
                var restore = new global::Avalonia.Threading.DispatcherTimer { Interval = System.TimeSpan.FromMilliseconds(600) };
        restore.Tick += (_, _) => { restore.Stop(); TitleText.Opacity = 1; };
        restore.Start();
                return;
            }
            for (var i = 0; i < _fields.Count; i++) _draft.Colors[i] = _fields[i].Color;
            _submitted = true;
            Close(true);
        };
        CancelButton.Click += (_, _) => Close(false);
    }

}
