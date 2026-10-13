using System.Globalization;
using System.Windows.Input;
using CodeWF.Avalonia.Demo.Infrastructure;
using CodeWF.Avalonia.Lang;
using CodeWF.Toolkit.Logging;
using ReactiveUI;

namespace CodeWF.Avalonia.Demo.ViewModels.Pages;

public sealed class LangPageViewModel : ReactiveObject
{
    private bool _active;
    public LangPageViewModel() => SwitchCultureCommand = ReactiveCommand.Create<string>(ApplyCulture);
    public ICommand SwitchCultureCommand { get; }
    public Action<string>? CultureRequested { private get; set; }
    public string State
    {
        get
        {
            var manager = I18nManager.Instance;
            var languages = manager.GetLanguages();
            var names = languages is null ? manager.GetResource("Demo.Text.LangPageaxaml.UpdateState004")
                : string.Join(", ", languages.Select(language => language.CultureName));
            return string.Format(CultureInfo.CurrentCulture, manager.GetResource("Demo.Text.LangPageaxaml.UpdateState005"), manager.Culture?.Name ?? DemoBootstrapper.DefaultCultureName)
                + string.Format(CultureInfo.CurrentCulture, manager.GetResource("Demo.Text.LangPageaxaml.UpdateState006"), names)
                + string.Format(CultureInfo.CurrentCulture, manager.GetResource("Demo.Text.LangPageaxaml.UpdateState007"), DemoBootstrapper.I18nFolder);
        }
    }

    public void Activate()
    {
        if (_active) return;
        _active = true;
        I18nManager.Instance.CultureChanged += OnCultureChanged;
        this.RaisePropertyChanged(nameof(State));
    }

    public void Deactivate()
    {
        if (!_active) return;
        _active = false;
        I18nManager.Instance.CultureChanged -= OnCultureChanged;
    }

    private void OnCultureChanged(object? sender, EventArgs e) => this.RaisePropertyChanged(nameof(State));
    private void ApplyCulture(string name)
    {
        if (name == "system") name = CultureInfo.CurrentUICulture.Name.StartsWith("zh", StringComparison.OrdinalIgnoreCase) ? "zh-CN" : "en-US";
        I18nManager.Instance.Culture = new CultureInfo(name);
        CultureRequested?.Invoke(name);
        Logger.Info(I18nManager.Instance.GetResource("Demo.Text.LangPageaxaml.ApplyCulture003") + name);
        this.RaisePropertyChanged(nameof(State));
    }
}
