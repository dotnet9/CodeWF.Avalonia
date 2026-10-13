using L = global::CodeWF.Avalonia.Lang.I18nManager;
using CodeWF.Avalonia.Controls.Models;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using ReactiveUI;

namespace CodeWF.Avalonia.Demo.ViewModels.Pages;

public sealed class StatusComponentsPageViewModel : ReactiveObject
{
    private const string IdKind = "st.kind";
    private const string IdIconWidth = "st.iconWidth";

    private StatusLabelKind _kind = StatusLabelKind.Debug;
    private int _iconWidth = 32;
    public StatusLabelKind Kind { get => _kind; private set => this.RaiseAndSetIfChanged(ref _kind, value); }
    public int IconWidth { get => _iconWidth; private set => this.RaiseAndSetIfChanged(ref _iconWidth, value); }
    public void Commit(PropertyValueChange change)
    {
        switch (change.Item.Id)
        {
            case IdKind when change.NewValue is string kind:
                Kind = kind switch
                {
                    var localizedValue when localizedValue == L.Instance.GetResource("Demo.Text.StatusComponentsPageaxaml.OnPropertyCommitted003") => StatusLabelKind.Info,
                    var localizedValue when localizedValue == L.Instance.GetResource("Demo.Text.StatusComponentsPageaxaml.OnPropertyCommitted004") => StatusLabelKind.Warn,
                    var localizedValue when localizedValue == L.Instance.GetResource("Demo.Text.StatusComponentsPageaxaml.OnPropertyCommitted005") => StatusLabelKind.Error,
                    var localizedValue when localizedValue == L.Instance.GetResource("Demo.Text.StatusComponentsPageaxaml.OnPropertyCommitted006") => StatusLabelKind.Fatal,
                    _ => StatusLabelKind.Debug,
                };
                break;
            case IdIconWidth when change.NewValue is decimal width:
                IconWidth = (int)width;
                break;
        }
    }

    public IReadOnlyList<PropertyItem> BuildProperties()
    {
        var kind = new PropertyItem
        {
            Id = IdKind,
            Name = L.Instance.GetResource("Demo.Text.StatusComponentsPageaxaml.BuildProperties007"),
            Kind = PropertyItemKind.Radio,
            Options = [L.Instance.GetResource("Demo.Text.StatusComponentsPageaxaml.BuildProperties008"), L.Instance.GetResource("Demo.Text.StatusComponentsPageaxaml.BuildProperties009"), L.Instance.GetResource("Demo.Text.StatusComponentsPageaxaml.BuildProperties010"), L.Instance.GetResource("Demo.Text.StatusComponentsPageaxaml.BuildProperties011"), L.Instance.GetResource("Demo.Text.StatusComponentsPageaxaml.BuildProperties012")],
            Value = L.Instance.GetResource("Demo.Text.StatusComponentsPageaxaml.BuildProperties013"),
            DefaultValue = L.Instance.GetResource("Demo.Text.StatusComponentsPageaxaml.BuildProperties014"),
            Description = L.Instance.GetResource("Demo.Text.StatusComponentsPageaxaml.BuildProperties015"),
        };
        var iconWidth = new PropertyItem
        {
            Id = IdIconWidth,
            Name = L.Instance.GetResource("Demo.Text.StatusComponentsPageaxaml.BuildProperties016"),
            Kind = PropertyItemKind.Number,
            Value = 32m,
            DefaultValue = 32m,
            Minimum = 16,
            Maximum = 64,
            Unit = "px",
            Description = L.Instance.GetResource("Demo.Text.StatusComponentsPageaxaml.BuildProperties017"),
        };

        return [kind, iconWidth];
    }
}
