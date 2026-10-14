using DemoResources = global::CodeWF.Avalonia.Demo.I18n.ResourceKeys;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using CodeWF.Avalonia.Demo.ViewModels;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class WelcomePage : DemoPageBase
{
    public Action<string>? Navigate { get; set; }
    public Action<string>? TogglePanel { get; set; }
    public WelcomeViewModel Overview { get; }

    public WelcomePage()
    {
        Overview = new WelcomeViewModel(key => Navigate?.Invoke(key), panel => TogglePanel?.Invoke(panel));
        InitializeComponent();
        DataContext = Overview;
        Overview.CopyToClipboard = text => TopLevel.GetTopLevel(this)?.Clipboard is { } clipboard
            ? clipboard.SetTextAsync(text)
            : Task.FromException(new InvalidOperationException("Clipboard unavailable"));
    }

    public override string TitleKey => DemoPages.Welcome;
    public override string DescriptionKey => DemoResources.Page.Welcome.Desc;
    public override string XamlCode => "<codewf:StudioTheme />";
    public override string CSharpCode => "Application.Current.RequestedThemeVariant = StudioTheme.DotNetDark;";


}
