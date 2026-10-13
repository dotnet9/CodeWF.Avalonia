using CodeWF.Toolkit.Logging;

namespace CodeWF.Avalonia.Demo.ViewModels;

/// <summary>
/// Shell 冒烟：按当前信息架构逐页构建（含 3 个场景页的全部步骤），
/// 验证页面工厂、Dock 文档承载与场景步骤切换的接线是否正确。
///
/// 注意：这里刻意**只触发页面工厂与场景步骤**，不把页面渲染进可视化树 ——
/// 主题资源在延迟内容中的解析时机仍在治理（见 ci.yml 中的说明），
/// 渲染会触发库侧 Semi 令牌在启动窗口期解析失败的问题，那是独立的库侧事项。
/// </summary>
public partial class MainViewModel
{
    private async Task SmokeWalkCatalogAsync(CancellationToken cancellation)
    {
        var built = 0;
        var failed = 0;
        var sceneSteps = 0;

        foreach (var group in Groups)
        {
            foreach (var item in group.Items)
            {
                try
                {
                    // 页面工厂：构建页面实例（场景页同时把内嵌组件页构造出来）
                    var page = item.PageFactory();
                    page.OnDocumentOpened();
                    built++;

                    if (page is Views.ScenePageBase scene)
                    {
                        // 场景页：把每一步都切一遍，验证步骤接线与内嵌页可构建
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

                    await Task.Delay(10, cancellation);
                }
                catch (Exception ex)
                {
                    failed++;
                    Logger.Warn($"冒烟：页面构建失败（{item.Title}）：{ex.Message}");
                }
            }
        }

        Logger.Info($"冒烟：构建 {built} 个页面（含 {sceneSteps} 次场景步骤切换），失败 {failed} 个");
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
