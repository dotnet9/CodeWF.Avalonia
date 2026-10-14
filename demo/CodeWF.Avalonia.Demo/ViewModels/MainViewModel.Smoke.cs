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
            Logger.Warn("Smoke: no window; page construction only, palette rendering skipped");
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
                            Logger.Warn($"Smoke: scene has no steps ({item.Title})");
                        }
                    }

                    if (host is not null)
                    {
                        Logger.Info("Smoke: rendering " + item.TitleKey);
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
                                        $"Smoke: {variant.Key} render failed ({item.Title}): {ex.GetType().Name}: {ex.Message}");
                                }
                            }
                        }

                        if (page is Views.Pages.ContentExpansionPage expansion)
                        {
                            var animators = global::Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(expansion)
                                .OfType<global::CodeWF.Avalonia.Controls.Controls.ContentExpansionAnimator>().ToArray();
                            if (animators.Length != 2 || animators.Any(animator => animator.Progress != 1 || animator.Bounds.Width <= 0 || animator.Bounds.Height <= 0 || animator.Opacity <= 0))
                                throw new InvalidOperationException("Expansion controls were not visible on first render.");
                            expansion.ViewModel.Milliseconds = 200;
                            expansion.ViewModel.IsExpanded = false;
                            await Task.Delay(350, cancellation);
                            if (animators.Length != 2 || animators.Any(animator => animator.IsExpanded || animator.Progress != 0))
                                throw new InvalidOperationException("Expansion state binding did not reach both controls.");
                            expansion.ViewModel.IsExpanded = true;
                            await Task.Delay(350, cancellation);
                            expansion.ViewModel.IsExpanded = false;
                            await Task.Delay(80, cancellation);
                            if (animators.Any(animator => animator.Progress <= 0 || animator.Progress >= 1))
                                throw new InvalidOperationException("Expansion animation did not produce intermediate progress.");
                            expansion.ViewModel.IsExpanded = true;
                            await Task.Delay(350, cancellation);
                            if (animators.Any(animator => !animator.IsExpanded || animator.Progress != 1))
                                throw new InvalidOperationException("Reversed expansion animation did not reach its target.");
                            expansion.ViewModel.Animate = false;
                            expansion.ViewModel.IsExpanded = false;
                            await Task.Delay(30, cancellation);
                            if (animators.Any(animator => animator.Progress != 0))
                                throw new InvalidOperationException("Disabled expansion animation did not collapse immediately.");
                            expansion.ViewModel.IsExpanded = true;
                            await Task.Delay(30, cancellation);
                            if (animators.Any(animator => !animator.IsExpanded || animator.Progress != 1))
                                throw new InvalidOperationException("Expansion controls did not return to the expanded state.");
                            Logger.Info("Expansion smoke: first render, animated collapse/expand, reversal and disabled motion passed");
                        }
                        host.Content = null;
                    }

                    await Task.Delay(10, cancellation);
                }
                catch (Exception ex)
                {
                    failed++;
                    Logger.Warn($"Smoke: page failed ({item.Title}): {ex.GetType().Name}: {ex.Message}");
                }
            }
        }

        // 还原成用户选择的外观
        if (Application.Current is { } restore)
        {
            restore.RequestedThemeVariant = SelectedPalette.ResolveVariant(IsDarkMode);
        }

        Logger.Info($"Smoke: {built} pages, {sceneSteps} scene steps, {rendered} renders, {failed} failures");
        foreach (var variant in variants)
        {
            Logger.Info(
                $"Smoke: {variant.Key}, {variantRenders[variant]} renders, {variantFailures[variant]} failures");
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
            .FirstOrDefault(i => i.TitleKey == DemoPages.AppGuide);
        if (item is not null)
        {
            OpenItem(item);
        }
    }
}
