using Avalonia.Controls;
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
}
