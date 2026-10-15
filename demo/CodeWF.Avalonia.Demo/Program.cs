using Avalonia;
using Avalonia.Media;
using ReactiveUI.Avalonia;

namespace CodeWF.Avalonia.Demo;

internal static class Program
{
    // 附加 --smoke 时：启动后自动执行冒烟演示并退出（CI / AOT 冒烟）。
    public static bool IsSmokeMode { get; private set; }

    public static string? CaptureDirectory { get; private set; }

    [STAThread]
    public static void Main(string[] args)
    {
        if (args.Length == 2 && args[0] == "--verify-runtime")
        {
            RuntimeVerification.Run(args[1]);
            return;
        }
        IsSmokeMode = args.Any(a => string.Equals(a, "--smoke", StringComparison.OrdinalIgnoreCase));
        CaptureDirectory = args.SkipWhile(a => a != "--capture").Skip(1).FirstOrDefault();
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            // 为西文字体缺失的中文字形指定无衬线回退，避免系统选到宋体。
            .With(new FontManagerOptions
            {
                FontFallbacks =
                [
                    new FontFallback { FontFamily = new FontFamily("Microsoft YaHei UI") },
                    new FontFallback { FontFamily = new FontFamily("PingFang SC") },
                    new FontFallback { FontFamily = new FontFamily("Noto Sans CJK SC") }
                ]
            })
            .UseReactiveUI(_ => { })
            .LogToTrace();
}
