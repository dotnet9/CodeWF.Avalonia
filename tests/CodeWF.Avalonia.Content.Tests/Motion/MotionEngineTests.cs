using Avalonia;
using Avalonia.Controls;
using CodeWF.Avalonia.Content.Tests.Markdown.Rendering;
using CodeWF.Avalonia.Motion;
using global::Avalonia.Media;

using Xunit;
namespace CodeWF.Avalonia.Content.Tests.Motion;

/// <summary>
/// Motion 引擎移植测试：弹簧数学（闭式解/回弹/静止估计）、预乘 alpha 混色、
/// ManualFrameClock 下的可中断动画（保速重定向）、动效分级与常驻循环门控。
/// </summary>
[Collection("AvaloniaPlatform")]
public sealed class MotionEngineTests(AvaloniaPlatformFixture platform)
{
    [Fact]
    public void SpringSpec_FromVisual_MapsToCorrectDamping()
    {
        // Smooth：bounce 0 → 临界阻尼（ζ=1）；Snappy：bounce 0.12 → 欠阻尼（ζ=0.88，会回弹）。
        Assert.Equal(1.0, ArcMotionPresets.Smooth.DampingRatio, 3);
        Assert.Equal(0.88, ArcMotionPresets.Snappy.DampingRatio, 3);
        Assert.True(ArcMotionPresets.Smooth.NaturalFrequency > 0);
    }

    [Fact]
    public void SpringSolver_ClosedForm_SettlesToTarget()
    {
        var settle = ArcMotionPresets.Smooth.EstimateSettle();

        var (displacement, velocity) = SpringSolver.Evaluate(ArcMotionPresets.Smooth, -1, 0, settle.TotalSeconds * 1.5);

        Assert.True(Math.Abs(displacement) < 0.01, $"静止后残余位移 {displacement}");
        Assert.True(Math.Abs(velocity) < 0.05, $"静止后残余速度 {velocity}");
        Assert.InRange(settle.TotalMilliseconds, 100, 2000);
    }

    [Fact]
    public void ArcSpringEasing_EndpointsHoldAndBouncyPresetOvershoots()
    {
        var snappy = ArcSpringEasing.For(ArcSpring.Snappy);
        Assert.Equal(0, snappy.Ease(0));
        Assert.Equal(1, snappy.Ease(1));

        var max = 0.0;
        for (var i = 1; i < 100; i++)
        {
            max = Math.Max(max, snappy.Ease(i / 100.0));
        }

        Assert.True(max > 1.0, $"Snappy（bounce 0.12）应有过冲，实测峰值 {max}");
        Assert.Equal(1.0, ArcSpringEasing.For(ArcSpring.Smooth).Ease(1));
    }

    [Fact]
    public void MixPremultiplied_TransparentEndKeepsHue()
    {
        // 预乘 alpha：不透明红 → 透明的中间态仍是纯红（半透明），不会经过浅灰——暗色面不闪灰的修复点。
        var half = ArcBrushTransition.MixPremultiplied(Colors.Red, Colors.Transparent, 0.5);

        Assert.Equal(128, half.A); // 约 127.5 四舍五入
        Assert.Equal(255, half.R);
        Assert.Equal(0, half.G);
        Assert.Equal(0, half.B);

        var gray = ArcBrushTransition.MixPremultiplied(Colors.Black, Colors.White, 0.5);
        Assert.Equal(gray.R, gray.G);
        Assert.Equal(gray.B, gray.G);
    }

    [Fact]
    public void MotionValue_ReachesTargetAndReportsChange()
    {
        RunWithWindow((window, owner) =>
        {
            var clock = new ManualFrameClock();
            using var clockRestore = clock.Install();
            using var value = new MotionValue<double>(owner, 0, MotionVectors.Double);
            var changes = 0;
            value.Changed += _ => changes++;

            var task = value.AnimateTo(100, MotionSpec.Spring(ArcSpring.Smooth));
            clock.Advance(TimeSpan.FromSeconds(2));

            Assert.True(task.IsCompletedSuccessfully);
            Assert.True(task.Result);
            Assert.Equal(100, value.Value, 0);
            Assert.InRange(changes, 3, 1000);
            Assert.False(value.IsAnimating);
        });
    }

    [Fact]
    public void MotionValue_RetargetKeepsPositionAndVelocity()
    {
        RunWithWindow((window, owner) =>
        {
            var clock = new ManualFrameClock();
            using var clockRestore = clock.Install();
            using var value = new MotionValue<double>(owner, 0, MotionVectors.Double);

            var first = value.AnimateTo(100, MotionSpec.Spring(ArcSpring.Smooth));
            clock.Advance(TimeSpan.FromMilliseconds(100));
            Assert.True(value.IsAnimating);
            var midFlight = value.Value;
            Assert.InRange(midFlight, 0.1, 99.9);

            // 重定向：位置当场不跳变（保持连续），之后带速度收敛到新目标。
            var second = value.AnimateTo(50, MotionSpec.Spring(ArcSpring.Smooth));
            Assert.Equal(midFlight, value.Value, 0);
            Assert.NotEqual(first, second);

            clock.Advance(TimeSpan.FromSeconds(2));
            Assert.Equal(50, value.Value, 0);
        });
    }

    [Fact]
    public void MotionValue_OffLevel_JumpsInstantly()
    {
        RunWithWindow((window, owner) =>
        {
            ArcMotion.SetLevel(window, ArcMotionLevel.Off);
            var clock = new ManualFrameClock();
            using var clockRestore = clock.Install();
            using var value = new MotionValue<double>(owner, 0, MotionVectors.Double);
            var changes = 0;
            value.Changed += _ => changes++;

            var task = value.AnimateTo(100, MotionSpec.Spring(ArcSpring.Smooth));

            Assert.True(task.IsCompletedSuccessfully);
            Assert.Equal(100, value.Value);
            Assert.Equal(1, changes);
            clock.Advance(TimeSpan.FromMilliseconds(50));
            Assert.Equal(100, value.Value);
            ArcMotion.SetLevel(window, ArcMotionLevel.Full);
        });
    }

    [Fact]
    public void LiveLoop_RunsWhileStartedAndStopsOnWithdraw()
    {
        RunWithWindow((window, owner) =>
        {
            var clock = new ManualFrameClock();
            using var clockRestore = clock.Install();
            var frames = 0;
            using var loop = new LiveLoop(owner, _ => { frames++; return true; }) { MinimumLevel = ArcMotionLevel.Off };
            var runningEvents = new List<bool>();
            loop.RunningChanged += runningEvents.Add;

            loop.Start();
            clock.Advance(TimeSpan.FromMilliseconds(300));
            Assert.True(loop.IsRunning);
            Assert.InRange(frames, 3, 1000);

            loop.Stop();
            Assert.False(loop.IsRunning);
            var framesAfterStop = frames;
            clock.Advance(TimeSpan.FromMilliseconds(200));
            Assert.Equal(framesAfterStop, frames);
            Assert.Equal([true, false], runningEvents);
        });
    }

    private static void RunWithWindow(Action<Window, Control> test)
    {
        // 无头平台的共享会话与既有测试一致（Markdown/Rendering/AvaloniaPlatformCollection）。
        var session = typeof(AvaloniaPlatformFixture).Assembly.GetType(
            "CodeWF.Avalonia.Content.Tests.Markdown.Rendering.AvaloniaPlatformFixture", throwOnError: true)!;
        var fixture = (AvaloniaPlatformFixture)Activator.CreateInstance(session, nonPublic: true)!;
        fixture.Run(() =>
        {
            var owner = new Border { Width = 20, Height = 20, Background = Brushes.Red };
            var window = new Window { Width = 300, Height = 200, Content = owner };
            try
            {
                window.Show();
                test(window, owner);
            }
            finally
            {
                window.Close();
            }
        });
    }
}
