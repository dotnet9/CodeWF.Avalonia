using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using CodeWF.Toolkit.Logging;

namespace CodeWF.Avalonia.Demo.ViewModels;

/// <summary>
/// Shell 冒烟：按当前信息架构逐页构建（含场景页的全部步骤），并挂进可视化树逐色板渲染一遍。
/// 构造成功 ≠ 渲染成功 —— 控件模板与主题资源只有在真实布局/测量时才会解析，
/// 主题变体切换正是 StaticResource 类问题的暴露点，所以这里逐色板走一轮。
/// </summary>
public partial class MainViewModel
{
    private async Task<bool> SmokeWalkCatalogAsync(CancellationToken cancellation)
    {
        var built = 0;
        var failed = 0;
        var sceneSteps = 0;
        var rendered = 0;

        var host = await AttachSmokeHostAsync(cancellation);
        if (host is null)
        {
            Logger.Warn("冒烟：没有可挂载的窗口，本次只做页面构建，未逐色板渲染");
        }

        var variants = Palettes.SelectMany(p => new[] { p.ResolveVariant(false), p.ResolveVariant(true) })
            .Distinct()
            .ToArray();
        var variantFailures = variants.ToDictionary(v => v, _ => 0);
        var variantRenders = variants.ToDictionary(v => v, _ => 0);
        var reported = new HashSet<string>();

        foreach (var group in Groups)
        {
            foreach (var item in group.Items)
            {
                try
                {
                    var page = item.PageFactory();
                    page.OnDocumentOpened();
                    built++;

                    if (page is Views.ScenePageBase scene)
                    {
                        for (var step = 0; step < scene.StepCount; step++)
                        {
                            scene.GoToStep(step);
                            sceneSteps++;
                        }

                        if (scene.StepCount == 0)
                        {
                            Logger.Warn($"冒烟：场景页没有步骤（{item.Title}）");
                        }
                    }

                    if (host is not null)
                    {
                        host.Content = page;
                        foreach (var variant in variants)
                        {
                            if (Application.Current is { } app)
                            {
                                app.RequestedThemeVariant = variant;
                            }

                            variantRenders[variant]++;
                            try
                            {
                                LayoutOnce(host);
                                rendered++;
                            }
                            catch (Exception ex)
                            {
                                variantFailures[variant]++;
                                failed++;
                                // 同一变体下的同类失败只报一次，避免刷屏。
                                if (reported.Add($"{variant.Key}:{ex.Message}"))
                                {
                                    Logger.Warn(
                                        $"冒烟：{variant.Key} 变体渲染失败（{item.Title}）：{ex.GetType().Name}: {ex.Message}");
                                }
                            }
                        }

                        host.Content = null;
                    }

                    await Task.Delay(10, cancellation);
                }
                catch (Exception ex)
                {
                    failed++;
                    Logger.Warn($"冒烟：页面失败（{item.Title}）：{ex.GetType().Name}: {ex.Message}");
                }
            }
        }

        // 还原成用户选择的外观
        if (Application.Current is { } restore)
        {
            restore.RequestedThemeVariant = SelectedPalette.ResolveVariant(IsDarkMode);
        }

        Logger.Info($"冒烟：构建 {built} 个页面（含 {sceneSteps} 次场景步骤切换，渲染 {rendered} 次），失败 {failed} 个");
        foreach (var variant in variants)
        {
            Logger.Info(
                $"冒烟：变体 {variant.Key} 渲染 {variantRenders[variant]} 次，失败 {variantFailures[variant]} 次");
        }

        // 外壳实体化件（通知中心 / 命令面板 / 密度 / 布局记忆 / 就地重命名）
        if (!await SmokeShellFeaturesAsync(cancellation))
        {
            failed++;
        }

        return failed == 0;
    }

    /// <summary>强制走一轮测量 + 排布（触发控件模板与主题资源解析）。</summary>
    private static void LayoutOnce(Control host)
    {
        host.Measure(new Size(1000, 700));
        host.Arrange(new Rect(0, 0, 1000, 700));
    }

    /// <summary>
    /// 把渲染宿主挂到主窗口的可视化树上。
    /// 无窗口时（无头冒烟）返回 null，调用方退化为「仅构建」。
    /// </summary>
    private async Task<ContentControl?> AttachSmokeHostAsync(CancellationToken cancellation)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return null;
        }

        var window = desktop.MainWindow ?? desktop.Windows.FirstOrDefault();
        if (window is null)
        {
            await Task.Delay(300, cancellation);
            window = desktop.MainWindow ?? desktop.Windows.FirstOrDefault();
        }

        if (window?.Content is not Control root)
        {
            return null;
        }

        var host = new ContentControl
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };

        switch (root)
        {
            case Panel panel:
                panel.Children.Add(host);
                break;
            case Decorator decorator:
                decorator.Child = host;
                break;
            default:
                return null;
        }

        return host;
    }

    /// <summary>标签栏引导入口：跳到「应用引导」演示页。</summary>
    private void OpenGuidanceDemo()
    {
        var item = Groups.SelectMany(g => g.Items)
            .FirstOrDefault(i => i.TitleKey == DemoKeys.ItemAppGuide);
        if (item is not null)
        {
            OpenItem(item);
        }
    }
}
