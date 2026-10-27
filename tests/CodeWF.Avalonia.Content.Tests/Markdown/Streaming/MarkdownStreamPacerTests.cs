using System.Text;

using CodeWF.Avalonia.Markdown.Streaming;

using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Streaming;

/// <summary>
/// 输入节奏器行为测试：平滑释放、滞留上限兜底、字素簇不拆、
/// 直通阈值、结束排空与内容保序完整。
/// </summary>
public sealed class MarkdownStreamPacerTests
{
	private static readonly TimeSpan Frame = TimeSpan.FromMilliseconds(16);

	[Fact]
	public void Burst_IsReleasedProgressively_AndCompletes()
	{
		var pacer = new MarkdownStreamPacer(MarkdownStreamPacer.PacingPreset.Balanced);
		var clock = TimeSpan.Zero;
		pacer.Push(new string('x', 400), clock);
		Assert.Empty(pacer.Tick(clock)); // 首帧无间隔，尚未积累释放量

		var total = 0;
		var firstTickReleased = -1;
		for (var i = 0; i < 40 && !pacer.IsDrained; i++)
		{
			clock += Frame;
			var released = pacer.Tick(clock).Length;
			if (firstTickReleased < 0 && released > 0)
			{
				firstTickReleased = released;
			}

			total += released;
		}

		Assert.InRange(firstTickReleased, 1, 399); // 平滑：首帧不放完全部
		Assert.True(pacer.IsDrained, "400 字符应在平滑窗口内排空");
		Assert.Equal(400, total);
	}

	[Fact]
	public void StaleChunks_AreForceReleased_ByMaxLag()
	{
		// 目标滞留 10s 使速率几乎为零，释放完全依赖滞留上限兜底。
		var pacer = new MarkdownStreamPacer(TimeSpan.FromSeconds(10));
		var clock = TimeSpan.Zero;
		pacer.Push("0123456789", clock);
		clock += TimeSpan.FromMilliseconds(1);
		Assert.Empty(pacer.Tick(clock));

		clock += TimeSpan.FromMilliseconds(999);
		var released = pacer.Tick(clock);
		Assert.Equal("0123456789", released); // 滞留已超 400ms，强制整段放行
	}

	[Fact]
	public void GraphemeClusters_NeverSplit()
	{
		// 高频小配额持续释放"汉👍"重复序列：32 字符的配额边界频繁落在
		// 代理对中间，释放必须整簇返回。断言每帧结果不以高位代理结尾
		// （配对被拆）且完整拼接与输入逐字符一致。
		var pacer = new MarkdownStreamPacer(TimeSpan.FromMilliseconds(30));
		var clock = TimeSpan.Zero;
		pacer.Tick(clock); // 预热帧

		var pushed = new StringBuilder();
		var released = new StringBuilder();
		for (var i = 0; i < 120; i++)
		{
			var chunk = string.Concat(Enumerable.Repeat("汉\U0001F44D", 32));
			pushed.Append(chunk);
			pacer.Push(chunk, clock);

			var text = pacer.Tick(Next(ref clock, Frame));
			if (text.Length > 0)
			{
				Assert.False(char.IsHighSurrogate(text[^1]), "释放以半截代理对结尾");
				Assert.False(char.IsLowSurrogate(text[0]), "释放以半截代理对开头");
				released.Append(text);
			}

			if (i % 3 == 0)
			{
				clock += Frame; // 到达与释放交错推进
			}
		}

		pacer.Finish();
		for (var i = 0; i < 400 && !pacer.IsDrained; i++)
		{
			var text = pacer.Tick(Next(ref clock, Frame));
			if (text.Length > 0)
			{
				Assert.False(char.IsHighSurrogate(text[^1]), "释放以半截代理对结尾");
				released.Append(text);
			}
		}

		Assert.True(pacer.IsDrained);
		Assert.Equal(pushed.ToString(), released.ToString());
	}

	[Fact]
	public void OversizedBypass_ReleasesEverythingAtOnce()
	{
		var pacer = new MarkdownStreamPacer(MarkdownStreamPacer.PacingPreset.Smooth);
		pacer.Push(new string('y', 5000), TimeSpan.Zero);
		var released = pacer.Tick(Frame);
		Assert.Equal(5000, released.Length); // 粘贴场景直通，不等平滑
	}

	[Fact]
	public void Finish_DrainsRemainingBacklog()
	{
		var pacer = new MarkdownStreamPacer(MarkdownStreamPacer.PacingPreset.Balanced);
		var clock = TimeSpan.Zero;
		pacer.Push(new string('z', 200), clock);
		pacer.Tick(clock);
		pacer.Finish();
		Assert.True(pacer.IsFinished);

		var total = 0;
		for (var i = 0; i < 60 && !pacer.IsDrained; i++)
		{
			clock += Frame;
			total += pacer.Tick(clock).Length;
		}

		Assert.True(pacer.IsDrained, "结束后的积压应在排空窗口内放完");
		Assert.Equal(200, total);
	}

	[Fact]
	public void RandomChunks_ContentPreserved_InOrder()
	{
		var pacer = new MarkdownStreamPacer(MarkdownStreamPacer.PacingPreset.Responsive);
		var random = new Random(20261026);
		var pushed = new StringBuilder();
		var released = new StringBuilder();
		var clock = TimeSpan.Zero;
		var nextArrival = TimeSpan.Zero;
		var arrivalsDone = false;

		for (var i = 0; i < 20000 && !(arrivalsDone && pacer.IsDrained); i++)
		{
			clock += Frame;
			if (!arrivalsDone)
			{
				while (nextArrival <= clock)
				{
					var chunk = new string((char)random.Next('A', 'Z'), random.Next(1, 300));
					pushed.Append(chunk);
					pacer.Push(chunk, nextArrival);
					nextArrival += TimeSpan.FromMilliseconds(random.Next(20, 200));
					if (pushed.Length >= 20000)
					{
						pacer.Finish();
						arrivalsDone = true;
						break;
					}
				}
			}

			released.Append(pacer.Tick(clock));
		}

		Assert.True(arrivalsDone && pacer.IsDrained, "随机流应在有限帧内排空");
		Assert.Equal(pushed.ToString(), released.ToString());
	}

	[Fact]
	public void EdgeCases_AreHandled()
	{
		// 目标滞留 1ms：小积压一帧内即可放完。
		var pacer = new MarkdownStreamPacer(TimeSpan.FromMilliseconds(1));
		var clock = TimeSpan.Zero;
		Assert.True(pacer.IsDrained);
		Assert.Empty(pacer.Tick(Next(ref clock, Frame)));

		pacer.Push(string.Empty, clock); // 空分片忽略
		Assert.True(pacer.IsDrained);

		pacer.Push("ab", clock);
		pacer.Finish();
		pacer.Push("cd", clock); // 结束后到达的文本忽略
		Assert.True(pacer.IsFinished);
		Assert.Equal("ab", pacer.Tick(Next(ref clock, Frame)));
		Assert.True(pacer.IsDrained);

		pacer.Reset();
		Assert.True(pacer.IsDrained);
		pacer.Push("ef", clock);
		Assert.Equal("ef", pacer.Tick(Next(ref clock, Frame)));
	}

	[Fact]
	public void InvalidTargetLag_Throws()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => new MarkdownStreamPacer(TimeSpan.Zero));
	}

	[Fact]
	public void RevealHistory_RecordsDisplayTimePerReleasedBatch()
	{
		var pacer = new MarkdownStreamPacer(TimeSpan.FromMilliseconds(1));
		var clock = TimeSpan.Zero;
		pacer.Tick(clock); // 预热帧：首帧无间隔不释放，先建立节拍
		Assert.Equal(0, pacer.RevealedLength);
		Assert.False(pacer.TryGetRevealTime(0, out _)); // 还没显示任何字符

		pacer.Push("abc", clock);
		var firstTick = Next(ref clock, Frame);
		Assert.Equal("abc", pacer.Tick(firstTick));
		Assert.Equal(3, pacer.RevealedLength);
		Assert.True(pacer.TryGetRevealTime(0, out var firstReveal));
		Assert.True(pacer.TryGetRevealTime(2, out var lastReveal));
		Assert.Equal(firstTick, firstReveal);
		Assert.Equal(firstTick, lastReveal); // 同一批释放共用一个显示时刻
		Assert.False(pacer.TryGetRevealTime(3, out _)); // 越界（尚未显示）

		pacer.Push("de", clock);
		var secondTick = Next(ref clock, Frame);
		Assert.Equal("de", pacer.Tick(secondTick));
		Assert.Equal(5, pacer.RevealedLength);
		Assert.True(pacer.TryGetRevealTime(3, out var laterReveal));
		Assert.Equal(secondTick, laterReveal);
		Assert.True(pacer.TryGetRevealTime(1, out var stillFirst));
		Assert.Equal(firstTick, stillFirst); // 旧批次仍可按偏移回查
	}

	[Fact]
	public void RevealHistory_KeepsNewestBatches_AndClearsOnReset()
	{
		// 每 5 帧到达 1 字符、约每 5 帧放行 1 字符：批次数超过环形容量，最旧批次被覆盖。
		var pacer = new MarkdownStreamPacer(MarkdownStreamPacer.PacingPreset.Balanced);
		var clock = TimeSpan.Zero;
		var pushCount = MarkdownStreamPacer.RevealHistoryCapacity + 60;
		pacer.Tick(clock); // 预热帧
		for (var i = 0; i < pushCount; i++)
		{
			pacer.Push("a", clock);
			for (var frame = 0; frame < 5; frame++)
			{
				clock += Frame;
				pacer.Tick(clock);
			}
		}

		Assert.True(pacer.RevealedLength > MarkdownStreamPacer.RevealHistoryCapacity);
		Assert.True(pacer.TryGetRevealTime(pacer.RevealedLength - 1, out _));
		// 环形缓冲只保留最近 256 批：最早可回查的偏移已经往后移，更早的偏移查不到时刻。
		Assert.True(pacer.EarliestRevealedOffset > 0);
		Assert.False(pacer.TryGetRevealTime(0, out _));

		pacer.Reset();
		Assert.Equal(0, pacer.RevealedLength);
		Assert.Equal(0, pacer.EarliestRevealedOffset);
		Assert.False(pacer.TryGetRevealTime(0, out _));
	}

	private static TimeSpan Next(ref TimeSpan clock, TimeSpan delta)
	{
		clock += delta;
		return clock;
	}
}
