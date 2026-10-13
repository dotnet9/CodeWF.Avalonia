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

        ok &= Check("Notification capacity and deduplication", () =>
        {
            ClearNotifications();
            for (var i = 0; i < 6; i++)
            {
                Notify(DemoNotificationKind.Info, "Notification " + i);
            }

            Expect(Notifications.Count == 4, "At most four notifications retained", Notifications.Count);
            Expect(HasNotifications, "Badge visible with notifications");
            ClearNotifications();
            Expect(!HasNotifications, "Badge hidden after clear");
        });

        ok &= Check("Command filtering and execution", () =>
        {
            var all = FilteredCommands;
            Expect(all.Count >= 25, "All document and shell commands listed by default", all.Count);

            CommandQuery = global::CodeWF.Avalonia.Lang.I18nManager.Instance.GetResource("Demo.Item.MarkdownExport");
            var filtered = FilteredCommands;
            Expect(filtered.Count > 0 && filtered.Count < all.Count, "Search returns a narrower result set", filtered.Count);
            Expect(filtered.All(e => e.Title.Contains(global::CodeWF.Avalonia.Lang.I18nManager.Instance.GetResource("Demo.Item.MarkdownExport"), StringComparison.OrdinalIgnoreCase)
                                     || e.Group.Contains(global::CodeWF.Avalonia.Lang.I18nManager.Instance.GetResource("Demo.Item.MarkdownExport"), StringComparison.OrdinalIgnoreCase)),
                "All results match the query");

            MoveCommandSelection(1);
            Expect(CommandIndex >= 0 && CommandIndex < filtered.Count, "Arrow selection remains in range", CommandIndex);

            CommandQuery = "zzz-no-command";
            Expect(FilteredCommands.Count == 0, "No match produces empty list");
            CommandQuery = string.Empty;
        });

        ok &= Check("Density toggle", () =>
        {
            Density = DemoDensity.Comfort;
            ToggleDensityCommand.Execute().Subscribe();
            Expect(Density == DemoDensity.Compact, "Switch to compact", Density);
            Expect(IsCompactDensity, "Compact flag applied");
            Expect(DensityLabel.Length > 0, "Density label localized", DensityLabel);
            ToggleDensityCommand.Execute().Subscribe();
            Expect(Density == DemoDensity.Comfort, "Restore comfortable density", Density);
        });

        ok &= Check("Notification shortcuts", () =>
        {
            CommandPaletteOpen = true;
            EscapeShellOverlay();
            Expect(!CommandPaletteOpen, "Escape closes the command palette");

            Notify(DemoNotificationKind.Warning, "Pending cleanup");
            EscapeShellOverlay();
            Expect(!HasNotifications, "Escape clears notifications");
        });

        ok &= Check("Navigation filtering", () =>
        {
            var before = FilteredGroups.Sum(g => g.Items.Count(i => !i.IsFilteredOut));
            NavFilter = global::CodeWF.Avalonia.Lang.I18nManager.Instance.GetResource("Demo.Item.MarkdownExport");
            var hits = FilteredGroups.Sum(g => g.Items.Count(i => !i.IsFilteredOut));
            Expect(hits > 0 && hits < before, "Filtering reduces visible leaves", hits);
            NavFilter = "zzz-no-page";
            Expect(IsNavEmpty, "No match displays empty state");
            NavFilter = string.Empty;
            Expect(FilteredGroups.Sum(g => g.Items.Count(i => !i.IsFilteredOut)) == before, "Clearing search restores all items");
        });

        ok &= Check("Navigation rename", () =>
        {
            var first = Groups.SelectMany(g => g.Items).First();
            var original = first.DisplayTitle;

            BeginRename(first);
            Expect(IsRenaming && ReferenceEquals(RenameTarget, first), "Enter editing state", RenameTarget?.TitleKey);
            RenameText = "My reader";
            CommitRename();
            Expect(!IsRenaming, "Editing ends after commit");
            Expect(first.DisplayTitle == "My reader", "Display title updated", first.DisplayTitle);
            Expect(HasRenames, "Rename marker is visible");

            ResetRenames();
            Expect(first.DisplayTitle == original, "Original title restored", first.DisplayTitle);
            Expect(!HasRenames, "Marker cleared");
        });

        ok &= Check("Layout snapshot round-trip", () =>
        {
            var temp = Path.Combine(Path.GetTempPath(), "codewf-avalonia-smoke-layout.json");
            var item = Groups.SelectMany(g => g.Items).First();
            var originalTitle = item.DisplayTitle;

            Density = DemoDensity.Compact;
            IsDarkMode = true;
            BeginRename(item);
            RenameText = "Smoke title";
            CommitRename();
            OpenItem(Groups[2].Items[1]);

            Expect(SaveLayoutTo(temp), "Write snapshot file", temp);

            // 改掉现场状态，再从存档恢复
            Density = DemoDensity.Comfort;
            IsDarkMode = false;
            ResetRenames();
            var raw = File.ReadAllText(temp);
            Expect(raw.Contains("Smoke title", StringComparison.Ordinal), "Snapshot contains custom title");
            Expect(raw.Contains("Compact", StringComparison.Ordinal), "Snapshot contains density");

            Expect(LoadLayoutFrom(temp), "Restore layout snapshot");
            Expect(Density == DemoDensity.Compact, "Density restored", Density);
            Expect(IsDarkMode, "Appearance mode restored", IsDarkMode);
            Expect(item.DisplayTitle == "Smoke title", "Custom title restored", item.DisplayTitle);
            Expect(Documents.Count > 0, "Document list restored", Documents.Count);

            // 收尾：删档并回到默认
            File.Delete(temp);
            item.CustomTitle = null;
            Density = DemoDensity.Comfort;
            IsDarkMode = false;
            Logger.Info("Layout round-trip completed, temporary file deleted: " + temp);
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
            Logger.Warn("Smoke: no window; shell rendering skipped");
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
                NavFilter = paletteOpen ? "Documents" : string.Empty;
                Notify(DemoNotificationKind.Success, "Shell rendering", compact ? "Compact" : "Comfort");

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
                        $"Smoke: shell render failed (compact={compact}, palette={paletteOpen}): {ex.GetType().Name}: {ex.Message}");
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
            Logger.Warn($"Smoke: rename render failed: {ex.GetType().Name}: {ex.Message}");
        }

        CancelRename();
        CommandPaletteOpen = false;
        NavFilter = string.Empty;
        Density = DemoDensity.Comfort;
        host.Content = null;
        ClearNotifications();

        Logger.Info($"Smoke: {rendered} shell renders, {failed} failures");
        return failed == 0;
    }

    private bool Check(string name, Action action)
    {
        try
        {
            action();
            Logger.Info("Smoke: shell check passed: " + name);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Warn($"Smoke: shell check failed ({name}): {ex.Message}");
            return false;
        }
    }

    private static void Expect(bool condition, string what, object? actual = null)
    {
        if (!condition)
        {
            throw new InvalidOperationException(actual is null ? what : $"{what} (actual: {actual})");
        }
    }
}
