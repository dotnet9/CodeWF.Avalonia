using Avalonia.Controls;
using CodeWF.Toolkit.Logging;
using System.Text.Json;

namespace CodeWF.Avalonia.Demo.ViewModels;

/// <summary>
/// 外壳实体化件的冒烟用例（随 <c>--smoke</c> 一起跑）：
/// 通知中心容量与自动过期、命令面板过滤与执行、密度切换、布局存档往返、就地重命名往返。
/// 全部走真实 ViewModel 接口，CM 里失败即让 <c>--smoke</c> 退出码非 0。
/// </summary>
public partial class MainViewModel
{
    private async Task<bool> SmokeShellFeaturesAsync(CancellationToken cancellation)
    {
        var ok = true;

        ok &= Check("通知中心容量与去重", () =>
        {
            ClearNotifications();
            for (var i = 0; i < 6; i++)
            {
                Notify(DemoNotificationKind.Info, "通知 " + i);
            }

            Expect(Notifications.Count == 4, "最多保留 4 张卡片", Notifications.Count);
            Expect(HasNotifications, "有卡片时徽标可见");
            ClearNotifications();
            Expect(!HasNotifications, "清空后徽标隐藏");
        });

        ok &= Check("命令面板过滤与执行", () =>
        {
            var all = FilteredCommands;
            Expect(all.Count >= 25, "默认列出全部文档与外壳动作", all.Count);

            CommandQuery = "导出";
            var filtered = FilteredCommands;
            Expect(filtered.Count > 0 && filtered.Count < all.Count, "关键字过滤命中且收窄", filtered.Count);
            Expect(filtered.All(e => e.Title.Contains("导出", StringComparison.OrdinalIgnoreCase)
                                     || e.Group.Contains("导出", StringComparison.OrdinalIgnoreCase)),
                "过滤结果都与关键字相关");

            MoveCommandSelection(1);
            Expect(CommandIndex >= 0 && CommandIndex < filtered.Count, "↑↓ 选择在范围内", CommandIndex);

            CommandQuery = "zzz-不存在的命令";
            Expect(FilteredCommands.Count == 0, "无匹配时列表为空");
            CommandQuery = string.Empty;
        });

        ok &= Check("密度切换", () =>
        {
            Density = DemoDensity.Comfort;
            ToggleDensityCommand.Execute().Subscribe();
            Expect(Density == DemoDensity.Compact, "切到紧凑", Density);
            Expect(IsCompactDensity, "紧凑标记生效");
            Expect(DensityLabel.Length > 0, "密度文案已本地化", DensityLabel);
            ToggleDensityCommand.Execute().Subscribe();
            Expect(Density == DemoDensity.Comfort, "再切回舒适", Density);
        });

        ok &= Check("通知中心快捷键", () =>
        {
            CommandPaletteOpen = true;
            EscapeShellOverlay();
            Expect(!CommandPaletteOpen, "Esc 关闭命令面板");

            Notify(DemoNotificationKind.Warning, "待清理");
            EscapeShellOverlay();
            Expect(!HasNotifications, "再按 Esc 清空通知");
        });

        ok &= Check("导航树过滤", () =>
        {
            var before = FilteredGroups.Sum(g => g.Items.Count(i => !i.IsFilteredOut));
            NavFilter = "导出";
            var hits = FilteredGroups.Sum(g => g.Items.Count(i => !i.IsFilteredOut));
            Expect(hits > 0 && hits < before, "过滤后可见叶子变少", hits);
            NavFilter = "zzz-不存在的页面";
            Expect(IsNavEmpty, "无匹配时进入空态");
            NavFilter = string.Empty;
            Expect(FilteredGroups.Sum(g => g.Items.Count(i => !i.IsFilteredOut)) == before, "清空关键字后恢复全部");
        });

        ok &= Check("导航树就地重命名", () =>
        {
            var first = Groups.SelectMany(g => g.Items).First();
            var original = first.DisplayTitle;

            BeginRename(first);
            Expect(IsRenaming && ReferenceEquals(RenameTarget, first), "进入编辑态", RenameTarget?.TitleKey);
            RenameText = "我的阅读器";
            CommitRename();
            Expect(!IsRenaming, "提交后退出编辑态");
            Expect(first.DisplayTitle == "我的阅读器", "显示名已更新", first.DisplayTitle);
            Expect(HasRenames, "出现「已重命名」标记");

            ResetRenames();
            Expect(first.DisplayTitle == original, "恢复原名", first.DisplayTitle);
            Expect(!HasRenames, "标记清除");
        });

        ok &= Check("布局存档往返", () =>
        {
            var temp = Path.Combine(Path.GetTempPath(), "codewf-avalonia-smoke-layout.json");
            var item = Groups.SelectMany(g => g.Items).First();
            var originalTitle = item.DisplayTitle;

            Density = DemoDensity.Compact;
            IsDarkMode = true;
            BeginRename(item);
            RenameText = "烟雾测试名";
            CommitRename();
            OpenItem(Groups[2].Items[1]);

            Expect(SaveLayoutTo(temp), "写出存档文件", temp);

            // 改掉现场状态，再从存档恢复
            Density = DemoDensity.Comfort;
            IsDarkMode = false;
            ResetRenames();
            var raw = File.ReadAllText(temp);
            Expect(raw.Contains("烟雾测试名", StringComparison.Ordinal), "存档里带上自定义名");
            Expect(raw.Contains("Compact", StringComparison.Ordinal), "存档里带上密度");

            Expect(LoadLayoutFrom(temp), "从存档恢复");
            Expect(Density == DemoDensity.Compact, "密度已恢复", Density);
            Expect(IsDarkMode, "深浅已恢复", IsDarkMode);
            Expect(item.DisplayTitle == "烟雾测试名", "自定义名已恢复", item.DisplayTitle);
            Expect(Documents.Count > 0, "文档列表已恢复", Documents.Count);

            // 收尾：删档并回到默认
            File.Delete(temp);
            item.CustomTitle = null;
            Density = DemoDensity.Comfort;
            IsDarkMode = false;
            Logger.Info("布局存档往返完成，临时文件已删除：" + temp);
        });

        // 外壳新控件的渲染（导航树面板 + 命令面板 / 通知中心浮层 + 重命名输入框）
        if (!await SmokeRenderShellChromeAsync(cancellation))
        {
            ok = false;
        }

        return ok;
    }

    /// <summary>
    /// 把外壳新控件（导航树面板 + 命令面板 / 通知中心浮层）也挂进可视化树渲染一遍：
    /// 文档页渲染覆盖不到外壳，而这两个浮层正是本次新增的 XAML 与绑定。
    /// </summary>
    private async Task<bool> SmokeRenderShellChromeAsync(CancellationToken cancellation)
    {
        var host = await AttachSmokeHostAsync(cancellation);
        if (host is null)
        {
            Logger.Warn("冒烟：没有可挂载的窗口，跳过外壳渲染");
            return true;
        }

        var rendered = 0;
        var failed = 0;
        var item = Groups.SelectMany(g => g.Items).First();

        foreach (var compact in new[] { false, true })
        {
            Density = compact ? DemoDensity.Compact : DemoDensity.Comfort;
            foreach (var paletteOpen in new[] { false, true })
            {
                CommandPaletteOpen = paletteOpen;
                NavFilter = paletteOpen ? "文档" : string.Empty;
                Notify(DemoNotificationKind.Success, "外壳渲染", compact ? "紧凑" : "舒适");

                var chrome = new Grid();
                chrome.Children.Add(new Views.Tools.NavToolView { DataContext = this });
                chrome.Children.Add(new Views.ShellOverlay { DataContext = this });
                host.Content = chrome;

                try
                {
                    LayoutOnce(host);
                    rendered++;
                }
                catch (Exception ex)
                {
                    failed++;
                    Logger.Warn(
                        $"冒烟：外壳渲染失败（compact={compact}, palette={paletteOpen}）：{ex.GetType().Name}: {ex.Message}");
                }
            }
        }

        // 就地重命名态：导航树里应出现输入框，也走一轮渲染
        BeginRename(item);
        var renameHost = new Grid();
        renameHost.Children.Add(new Views.Tools.NavToolView { DataContext = this });
        host.Content = renameHost;
        try
        {
            LayoutOnce(host);
            rendered++;
        }
        catch (Exception ex)
        {
            failed++;
            Logger.Warn($"冒烟：导航树重命名态渲染失败：{ex.GetType().Name}: {ex.Message}");
        }

        CancelRename();
        CommandPaletteOpen = false;
        NavFilter = string.Empty;
        Density = DemoDensity.Comfort;
        host.Content = null;
        ClearNotifications();

        Logger.Info($"冒烟：外壳渲染 {rendered} 次，失败 {failed} 次");
        return failed == 0;
    }

    private bool Check(string name, Action action)
    {
        try
        {
            action();
            Logger.Info("冒烟：外壳用例通过 —— " + name);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Warn($"冒烟：外壳用例失败（{name}）：{ex.Message}");
            return false;
        }
    }

    private static void Expect(bool condition, string what, object? actual = null)
    {
        if (!condition)
        {
            throw new InvalidOperationException(actual is null ? what : $"{what}（实际 {actual}）");
        }
    }
}
