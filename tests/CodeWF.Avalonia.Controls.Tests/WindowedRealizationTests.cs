using CodeWF.Avalonia.Controls.Controls;

namespace CodeWF.Avalonia.Controls.Tests;

/// <summary>
/// 窗口化面板的裁剪策略（纯逻辑）：窗口内物化、窗口外释放、固定项始终物化、overscan 扩展区间。
/// </summary>
public sealed class WindowedRealizationTests
{
    private const double ItemHeight = 20;
    private const int ItemCount = 220;

    [Fact]
    public void Resolve_WhenNoWindow_RealizesEveryItem()
    {
        var result = WindowedRealization.Resolve(
            Heights(), Pinned(), 0, double.NegativeInfinity, double.PositiveInfinity);

        Assert.Equal(ItemCount, result.Count(realized => realized));
    }

    [Fact]
    public void Resolve_WhenWindowCoversViewport_RealizesOnlyVisibleItems()
    {
        var result = WindowedRealization.Resolve(Heights(), Pinned(), 0, 0, 200);

        Assert.True(result[0]);
        Assert.True(result[10]);
        Assert.False(result[11]);
        Assert.Equal(11, result.Count(realized => realized));
    }

    [Fact]
    public void Resolve_WhenItemIsPinnedOutOfWindow_KeepsItRealized()
    {
        var pinned = new bool[ItemCount];
        pinned[ItemCount - 1] = true;

        var result = WindowedRealization.Resolve(Heights(), pinned, 0, 0, 200);

        Assert.True(result[ItemCount - 1]);
        Assert.Equal(12, result.Count(realized => realized));
    }

    [Fact]
    public void Resolve_WithSpacing_AccountsForGap()
    {
        var heights = Enumerable.Repeat(ItemHeight, 10).ToArray();

        // 间距 4：第 6 项顶边 = 6 * 24 = 144，窗口 0..150 覆盖到第 6 项。
        var result = WindowedRealization.Resolve(heights, new bool[10], 4, 0, 150);

        Assert.True(result[6]);
        Assert.False(result[7]);
    }

    [Fact]
    public void ResolveWindow_WithViewportAndOverscan_ExtendsBothSides()
    {
        var (top, bottom) = WindowedRealization.ResolveWindow(1000, 200, 2);

        Assert.Equal(600, top, 3);
        Assert.Equal(1600, bottom, 3);
    }

    [Fact]
    public void ResolveWindow_WithInvalidViewport_DisablesClipping()
    {
        var (top, bottom) = WindowedRealization.ResolveWindow(100, 0, 2);

        Assert.True(double.IsNegativeInfinity(top));
        Assert.True(double.IsPositiveInfinity(bottom));
    }

    [Fact]
    public void ResolveInPlace_WithLargeMeasuredBlock_KeepsItWithinBudget()
    {
        // 第 0 项是大块（400），其余是估算尺寸的轻量项；窗口在文档后半段。
        var heights = Enumerable.Repeat(20d, 20).ToArray();
        heights[0] = 400;
        var realized = Enumerable.Repeat(true, 20).ToArray();
        var inWindow = new bool[20];
        for (var i = 10; i < 20; i++)
        {
            inWindow[i] = true;
        }

        var result = WindowedRealization.ResolveInPlace(
            heights, new bool[20], realized, inWindow, true, 1000, 100, 0);

        Assert.True(result[0]);
        Assert.Single(result, keep => keep);
    }

    [Fact]
    public void ResolveInPlace_WhenBudgetTooSmall_ReleasesLargeBlock()
    {
        var heights = Enumerable.Repeat(20d, 20).ToArray();
        heights[0] = 400;
        var realized = Enumerable.Repeat(true, 20).ToArray();
        var inWindow = new bool[20];
        for (var i = 10; i < 20; i++)
        {
            inWindow[i] = true;
        }

        var result = WindowedRealization.ResolveInPlace(
            heights, new bool[20], realized, inWindow, true, 100, 100, 0);

        Assert.False(result[0]);
    }

    [Fact]
    public void ResolveInPlace_WhenBelowMinHeight_IsNotKept()
    {
        var heights = Enumerable.Repeat(20d, 20).ToArray();
        var realized = Enumerable.Repeat(true, 20).ToArray();
        var inWindow = new bool[20];
        for (var i = 10; i < 20; i++)
        {
            inWindow[i] = true;
        }

        var result = WindowedRealization.ResolveInPlace(
            heights, new bool[20], realized, inWindow, true, 5000, 100, 0);

        Assert.DoesNotContain(true, result);
    }

    [Fact]
    public void ResolveInPlace_WhenCandidateMaskExcludesItem_IsNotKept()
    {
        var heights = Enumerable.Repeat(20d, 20).ToArray();
        heights[0] = 400;
        heights[5] = 400;
        var realized = Enumerable.Repeat(true, 20).ToArray();
        var inWindow = new bool[20];
        for (var i = 10; i < 20; i++)
        {
            inWindow[i] = true;
        }

        // 只有第 5 项是候选：第 0 项即使更大也不保留。
        var candidates = new bool[20];
        candidates[5] = true;

        var result = WindowedRealization.ResolveInPlace(
            heights, new bool[20], realized, inWindow, true, 1000, 100, 0, candidates);

        Assert.False(result[0]);
        Assert.True(result[5]);
    }

    [Fact]
    public void ResolveInPlace_WhenInWindow_IsNeverAKeepAliveCandidate()
    {
        var heights = Enumerable.Repeat(20d, 20).ToArray();
        heights[12] = 400;
        var realized = Enumerable.Repeat(true, 20).ToArray();
        var inWindow = new bool[20];
        for (var i = 10; i < 20; i++)
        {
            inWindow[i] = true;
        }

        var result = WindowedRealization.ResolveInPlace(
            heights, new bool[20], realized, inWindow, true, 1000, 100, 0);

        Assert.False(result[12]);
        Assert.DoesNotContain(true, result);
    }

    [Fact]
    public void ResolveInPlace_UnknownSizedItems_FollowScrollDirection()
    {
        // 全部尺寸未知（0），窗口为第 10..18 项：窗口上下各有一个紧邻候选（9 与 19），距离相同。
        var heights = new double[20];
        Array.Fill(heights, 0d);
        var realized = Enumerable.Repeat(true, 20).ToArray();
        var inWindow = new bool[20];
        for (var i = 10; i <= 18; i++)
        {
            inWindow[i] = true;
        }

        // 向下滚动：同距离时先留窗口上沿外的那一项。
        var down = WindowedRealization.ResolveInPlace(
            heights, new bool[20], realized, inWindow, true, 0, 0, 1, null, scrollDown: true);
        Assert.True(down[9]);
        Assert.Equal(1, down.Count(keep => keep));

        // 向上滚动：先留窗口下沿外的那一项。
        var up = WindowedRealization.ResolveInPlace(
            heights, new bool[20], realized, inWindow, true, 0, 0, 1, null, scrollDown: false);
        Assert.True(up[19]);
        Assert.Equal(1, up.Count(keep => keep));
    }

    [Fact]
    public void ResolveInPlace_WithoutBudgetOrQuota_KeepsNothing()
    {
        var heights = Enumerable.Repeat(400d, 20).ToArray();
        var realized = Enumerable.Repeat(true, 20).ToArray();
        var inWindow = new bool[20];
        for (var i = 10; i < 20; i++)
        {
            inWindow[i] = true;
        }

        var result = WindowedRealization.ResolveInPlace(
            heights, new bool[20], realized, inWindow, true, 0, 0, 0);

        Assert.DoesNotContain(true, result);
    }

    [Fact]
    public void ResolveInPlace_WhenItemWasAlreadyReleased_IsNotKeptAgain()
    {
        var heights = Enumerable.Repeat(400d, 20).ToArray();
        var realized = new bool[20];
        var inWindow = new bool[20];
        for (var i = 10; i < 20; i++)
        {
            inWindow[i] = true;
        }

        var result = WindowedRealization.ResolveInPlace(
            heights, new bool[20], realized, inWindow, true, 5000, 100, 0);

        Assert.DoesNotContain(true, result);
    }

    private static double[] Heights() => Enumerable.Repeat(ItemHeight, ItemCount).ToArray();

    private static bool[] Pinned() => new bool[ItemCount];
}