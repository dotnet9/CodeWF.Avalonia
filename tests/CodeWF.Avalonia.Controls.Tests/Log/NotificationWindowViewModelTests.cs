using CodeWF.Avalonia.Log;
using CodeWF.Avalonia.Log.Notifications.ViewModels;
using CodeWF.Toolkit.Lang;
using CodeWF.Toolkit.Logging;
using Microsoft.Extensions.Logging;
using Xunit;
using System.Globalization;

namespace CodeWF.Avalonia.Controls.Tests.Log;

[Collection(nameof(NotificationCultureCollection))]
public sealed class NotificationWindowViewModelTests
{
    [Theory]
    [InlineData("en-US", "2 new logs")]
    [InlineData("zh-CN", "2条新日志")]
    public void NewLogIndicator_TracksLogsAddedWhileWindowIsOpen(string culture, string expectedNewLogs)
    {
        using var cultureScope = new NotificationCultureScope(culture);
        var viewModel = CreateViewModel();
        try
        {
            viewModel.AddLogs([(CreateEvent(1, LogLevel.Error), "first")]);

            Assert.False(viewModel.IsNewLogVisible);
            Assert.False(viewModel.IsNavigationVisible);
            Assert.Equal(284, viewModel.MinimumCardHeight);
            Assert.Equal("1 / 1", viewModel.CountText);

            viewModel.OnOpened();
            viewModel.AddLogs([
                (CreateEvent(2, LogLevel.Error), "second"),
                (CreateEvent(3, LogLevel.Critical), "third")
            ]);

            Assert.True(viewModel.IsNewLogVisible);
            Assert.Equal(expectedNewLogs, viewModel.NewLogText);
            Assert.True(viewModel.IsNavigationVisible);
            Assert.Equal(320, viewModel.MinimumCardHeight);
            Assert.Equal("3 / 3", viewModel.CountText);
            Assert.False(viewModel.IsPreviousDisabled);
            Assert.True(viewModel.IsNextDisabled);

            viewModel.SelectPrevious();
            Assert.Equal("2 / 3", viewModel.CountText);
            Assert.Equal(expectedNewLogs, viewModel.NewLogText);
            Assert.False(viewModel.IsPreviousDisabled);
            Assert.False(viewModel.IsNextDisabled);

            viewModel.OnClosed();
            Assert.False(viewModel.IsNewLogVisible);
        }
        finally
        {
            viewModel.OnClosed();
        }
    }

    [Theory]
    [InlineData("en-US", "Critical")]
    [InlineData("zh-CN", "严重错误")]
    public void SelectedCriticalLog_UsesCriticalTextAndTruncatesExcessiveContent(string culture, string expectedLevel)
    {
        using var cultureScope = new NotificationCultureScope(culture);
        var viewModel = CreateViewModel();
        var content = new string('x', 4100);

        var logEvent = CreateEvent(1, LogLevel.Critical) with
        {
            Message = content,
            UserMessage = content
        };
        viewModel.AddLogs([(logEvent, "formatted template content")]);

        Assert.Equal(LogLevel.Critical, viewModel.Level);
        Assert.Equal(expectedLevel, viewModel.LevelText);
        Assert.Equal(4003, viewModel.LogContent.Length);
        Assert.EndsWith("...", viewModel.LogContent);
        Assert.Equal("formatted template content", viewModel.SelectedLog?.Content);
        Assert.Equal(420, viewModel.MinimumCardHeight);
    }

    // 只在测试作用域临时设置文化，恢复线程默认值与已经初始化的全局语言。
    private sealed class NotificationCultureScope : IDisposable
    {
        private readonly CultureInfo? _managerCulture = I18nManager.Instance.Culture;
        private readonly CultureInfo _currentCulture = CultureInfo.CurrentCulture;
        private readonly CultureInfo _currentUiCulture = CultureInfo.CurrentUICulture;
        private readonly CultureInfo? _defaultCulture = CultureInfo.DefaultThreadCurrentCulture;
        private readonly CultureInfo? _defaultUiCulture = CultureInfo.DefaultThreadCurrentUICulture;

        public NotificationCultureScope(string culture)
        {
            var selectedCulture = CultureInfo.GetCultureInfo(culture);
            // 未初始化时保留空状态，资源查询使用当前线程文化；公开 API 不支持恢复为空。
            if (_managerCulture is not null) I18nManager.Instance.Culture = selectedCulture;
            CultureInfo.CurrentCulture = selectedCulture;
            CultureInfo.CurrentUICulture = selectedCulture;
        }

        public void Dispose()
        {
            try
            {
                if (_managerCulture is not null) I18nManager.Instance.Culture = _managerCulture;
            }
            finally
            {
                CultureInfo.DefaultThreadCurrentCulture = _defaultCulture;
                CultureInfo.DefaultThreadCurrentUICulture = _defaultUiCulture;
                CultureInfo.CurrentCulture = _currentCulture;
                CultureInfo.CurrentUICulture = _currentUiCulture;
            }
        }
    }

    private static NotificationWindowViewModel CreateViewModel()
    {
        var viewModel = new NotificationWindowViewModel();
        viewModel.Configure(
            "Test App",
            TimeSpan.Zero,
            host: null,
            contentTemplate: null,
            DesktopNotificationAttentionMode.None);
        return viewModel;
    }

    private static CodeWFLogEvent CreateEvent(long sequence, LogLevel level) => new()
    {
        Sequence = sequence,
        Timestamp = new DateTimeOffset(2026, 7, 27, 10, 30, 0, TimeSpan.Zero),
        Level = level,
        CategoryName = "Tests",
        Message = $"message-{sequence}",
        UserMessage = $"user-message-{sequence}",
        RequestNotification = true
    };
}

// 全局语言及默认线程文化是共享状态，不能与其他测试集合并行修改。
[CollectionDefinition(nameof(NotificationCultureCollection), DisableParallelization = true)]
public sealed class NotificationCultureCollection
{
}
