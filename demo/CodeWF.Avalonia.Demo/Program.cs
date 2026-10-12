using ReactiveUI.Avalonia;
using Avalonia;

namespace CodeWF.Avalonia.Demo;

internal static class Program
{
    // 附加 --smoke 时：启动后自动执行冒烟演示并退出（CI / AOT 冒烟）。
    public static bool IsSmokeMode { get; private set; }

    [STAThread]
    public static void Main(string[] args)
    {
        IsSmokeMode = args.Any(a => string.Equals(a, "--smoke", StringComparison.OrdinalIgnoreCase));
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .UseReactiveUI(_ => { })
            .LogToTrace();
}
