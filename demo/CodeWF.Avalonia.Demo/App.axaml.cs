using Avalonia.Controls;
using CodeWF.Avalonia.Demo.Views;
using Prism.DryIoc;
using Prism.Ioc;

namespace CodeWF.Avalonia.Demo;

public class App : PrismApplication
{
    protected override Window CreateShell() => Container.Resolve<MainWindow>();

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
    }
}
