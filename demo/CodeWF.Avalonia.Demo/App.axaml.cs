using Avalonia.Controls;
using Avalonia.Threading;
using CodeWF.Toolkit.Lang;
using CodeWF.Avalonia.Demo.Infrastructure;
using CodeWF.Avalonia.Demo.Views;
using Prism.DryIoc;
using Prism.Ioc;

namespace CodeWF.Avalonia.Demo;

public class App : PrismApplication
{
    public override void Initialize()
    {
        base.Initialize();
        I18nManager.Instance.CultureChanged += OnCultureChanged;
        ApplyControlCulture();
        // 复用 Dock 自带的内容回收模板，分屏和浮动时转移原视图，避免重复视觉父级。
        var documentContent = new global::Avalonia.Controls.Recycling.ControlRecyclingDataTemplate();
        var toolContent = new global::Avalonia.Controls.Recycling.ControlRecyclingDataTemplate();
        DataTemplates.Add(new global::Avalonia.Controls.Templates.FuncDataTemplate<global::Dock.Model.ReactiveUI.Controls.Document>(
            (document, _) => documentContent.Build(document?.Context), supportsRecycling: true));
        DataTemplates.Add(new global::Avalonia.Controls.Templates.FuncDataTemplate<global::Dock.Model.ReactiveUI.Controls.Tool>(
            (tool, _) => toolContent.Build(tool?.Context), supportsRecycling: true));
    }

    protected override Window CreateShell()
    {
        // Lang 资源必须先于 Shell XAML 解析注册（I18n 标记扩展在解析期求值）。
        DemoBootstrapper.Initialize();
        return Container.Resolve<MainWindow>();
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
    }

    private static void OnCultureChanged(object? sender, EventArgs args)
    {
        if (Dispatcher.UIThread.CheckAccess()) ApplyControlCulture();
        else Dispatcher.UIThread.Post(ApplyControlCulture);
    }

    /// <summary>让第三方内建菜单跟随全局语言，保留宿主同键资源的优先级。</summary>
    private static void ApplyControlCulture()
    {
        if (global::Avalonia.Application.Current is not { } application || I18nManager.Instance.Culture is not { } culture) return;
        var semiCulture = culture.Name.StartsWith("zh-Hant", StringComparison.OrdinalIgnoreCase)
            ? System.Globalization.CultureInfo.GetCultureInfo("zh-TW") : culture;
        // 当前 Ursa 包没有日语资源，采用英文回退，避免主题默认回退为中文。
        var ursaCulture = culture.TwoLetterISOLanguageName == "ja"
            ? System.Globalization.CultureInfo.GetCultureInfo("en-US") : culture;
        foreach (var theme in application.Styles.OfType<Semi.Avalonia.SemiTheme>()) theme.Locale = semiCulture;
        foreach (var theme in application.Styles.OfType<Ursa.Themes.Semi.UrsaSemiTheme>()) theme.Locale = ursaCulture;
    }
}
