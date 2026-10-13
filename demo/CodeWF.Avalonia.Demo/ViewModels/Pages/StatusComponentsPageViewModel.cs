using L = global::CodeWF.Avalonia.Lang.I18nManager;
using CodeWF.Avalonia.Controls.Models;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using ReactiveUI;

namespace CodeWF.Avalonia.Demo.ViewModels.Pages;

public sealed class StatusComponentsPageViewModel : ReactiveObject
{
    public StatusComponentsPageViewModel(string control = "") { ShowBadges = control != "StatusCard"; ShowCards = control != "StatusBadge"; }
    public bool ShowBadges { get; }
    public bool ShowCards { get; }

    private const string IdKind = "st.kind";
    private const string IdIconWidth = "st.iconWidth";

    private StatusLabelKind _kind = StatusLabelKind.Info;
    private int _iconWidth = 32;
    private string _badgeText = L.Instance.GetResource("Demo.Text.StatusComponentsPage.View013");
    public string BadgeKind => Kind.ToString();
    public bool BadgeDebug => Kind == StatusLabelKind.Debug;
    public bool BadgeInfo => Kind == StatusLabelKind.Info;
    public bool BadgeWarn => Kind == StatusLabelKind.Warn;
    public bool BadgeError => Kind == StatusLabelKind.Error;
    public bool BadgeFatal => Kind == StatusLabelKind.Fatal;
    public string BadgeText { get => _badgeText; private set => this.RaiseAndSetIfChanged(ref _badgeText, value); }
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
                this.RaisePropertyChanged(nameof(BadgeKind));
                this.RaisePropertyChanged(nameof(BadgeDebug));
                this.RaisePropertyChanged(nameof(BadgeInfo));
                this.RaisePropertyChanged(nameof(BadgeWarn));
                this.RaisePropertyChanged(nameof(BadgeError));
                this.RaisePropertyChanged(nameof(BadgeFatal));
                break;
            case "st.badgeText" when change.NewValue is string text:
                BadgeText = text;
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

        if (ShowBadges && !ShowCards)
        {
            kind.Name = "Classes / LeftText";
            kind.Description = L.Instance.GetResource("Demo.Api.StatusBadge.Desc");
            return [kind, new PropertyItem
            {
                Id = "st.badgeText", Name = "RightText", Kind = PropertyItemKind.Text,
                Value = BadgeText, DefaultValue = L.Instance.GetResource("Demo.Text.StatusComponentsPage.View013"),
                Description = L.Instance.GetResource("Demo.Api.StatusBadge.Desc"),
            }];
        }
        return [kind, iconWidth];
    }
}
