using DemoResources = global::CodeWF.Avalonia.Demo.I18n.ResourceKeys;
using L = global::CodeWF.Toolkit.Lang.I18nManager;
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
    private string _badgeText = L.Instance.GetResource(DemoResources.Content.StatusComponents.View.ChangeTheStatusAndIconWidthInThe);
    private bool _badgeTextEdited;
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
                    var localizedValue when localizedValue == L.Instance.GetResource(DemoResources.Content.StatusComponents.OnPropertyCommitted.Information) => StatusLabelKind.Info,
                    var localizedValue when localizedValue == L.Instance.GetResource(DemoResources.Content.StatusComponents.OnPropertyCommitted.Warning) => StatusLabelKind.Warn,
                    var localizedValue when localizedValue == L.Instance.GetResource(DemoResources.Content.StatusComponents.OnPropertyCommitted.Error) => StatusLabelKind.Error,
                    var localizedValue when localizedValue == L.Instance.GetResource(DemoResources.Content.StatusComponents.OnPropertyCommitted.Critical) => StatusLabelKind.Fatal,
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
                _badgeTextEdited = true;
                BadgeText = text;
                break;
            case IdIconWidth when change.NewValue is decimal width:
                IconWidth = (int)width;
                break;
        }
    }

    public IReadOnlyList<PropertyItem> BuildProperties()
    {
        // 未编辑的示例文案跟随界面语言，用户输入与已选状态在重建属性表时保留。
        if (!_badgeTextEdited)
            BadgeText = L.Instance.GetResource(DemoResources.Content.StatusComponents.View.ChangeTheStatusAndIconWidthInThe);
        var kind = new PropertyItem
        {
            Id = IdKind,
            Name = L.Instance.GetResource(DemoResources.Content.StatusComponents.Properties.CardStatus),
            Kind = PropertyItemKind.Choice,
            Options = [L.Instance.GetResource(DemoResources.Content.StatusComponents.Properties.Debug), L.Instance.GetResource(DemoResources.Content.StatusComponents.Properties.Information), L.Instance.GetResource(DemoResources.Content.StatusComponents.Properties.Warning), L.Instance.GetResource(DemoResources.Content.StatusComponents.Properties.Error), L.Instance.GetResource(DemoResources.Content.StatusComponents.Properties.Critical)],
            Value = L.Instance.GetResource(Kind switch
            {
                StatusLabelKind.Debug => DemoResources.Content.StatusComponents.Properties.Debug,
                StatusLabelKind.Warn => DemoResources.Content.StatusComponents.Properties.Warning,
                StatusLabelKind.Error => DemoResources.Content.StatusComponents.Properties.Error,
                StatusLabelKind.Fatal => DemoResources.Content.StatusComponents.Properties.Critical,
                _ => DemoResources.Content.StatusComponents.Properties.Information,
            }),
            DefaultValue = L.Instance.GetResource(DemoResources.Content.StatusComponents.Properties.Information),
            Description = L.Instance.GetResource(DemoResources.Content.StatusComponents.Properties.FiveStatusLabelKindValuesChangeTheCardStateAnd),
        };
        var iconWidth = new PropertyItem
        {
            Id = IdIconWidth,
            Name = L.Instance.GetResource(DemoResources.Content.StatusComponents.Properties.IconWidth),
            Kind = PropertyItemKind.Number,
            Value = (decimal)IconWidth,
            DefaultValue = 32m,
            Minimum = 16,
            Maximum = 64,
            Unit = "px",
            Description = L.Instance.GetResource(DemoResources.Content.StatusComponents.Properties.IconWidthOfTheConfigurableCard),
        };

        if (ShowBadges && !ShowCards)
        {
            kind.Name = "Classes / LeftText";
            kind.Description = L.Instance.GetResource(DemoResources.Api.StatusBadge.Desc);
            return [kind, new PropertyItem
            {
                Id = "st.badgeText", Name = "RightText", Kind = PropertyItemKind.Text,
                Value = BadgeText, DefaultValue = L.Instance.GetResource(DemoResources.Content.StatusComponents.View.ChangeTheStatusAndIconWidthInThe),
                Description = L.Instance.GetResource(DemoResources.Api.StatusBadge.Desc),
            }];
        }
        return [kind, iconWidth];
    }
}
