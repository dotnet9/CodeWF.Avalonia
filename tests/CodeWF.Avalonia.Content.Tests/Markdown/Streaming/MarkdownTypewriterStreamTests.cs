using System.Text;

using CodeWF.Avalonia.Markdown.Streaming;

using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Streaming;

/// <summary>
/// LLM 流式打字机包装：任意线程喂 delta → 内建节奏器按帧释放 → 排空自动
/// 终局。手动泵模式（Tick）驱动，不依赖真实计时器。
/// </summary>
public sealed class MarkdownTypewriterStreamTests
{
	private static readonly TimeSpan Frame = TimeSpan.FromMilliseconds(16);



	/// <summary>等待目标文档终局（Abort/Finish 的 Complete 在后台工作者上生效）。</summary>
	private static void WaitForComplete(MarkdownStreamingDocument target)
	{
		var deadline = DateTime.UtcNow.AddSeconds(10);
		while (!target.Latest.IsComplete)
		{
			if (DateTime.UtcNow > deadline)
			{
				throw new TimeoutException("目标文档未在 10s 内终局");
			}

			Thread.Sleep(5);
		}
	}

	/// <summary>目标文档在后台工作者上生效，断言前轮询至源码到达期望长度。</summary>
	private static void WaitForSourceLength(MarkdownStreamingDocument target, int length)
	{
		var deadline = DateTime.UtcNow.AddSeconds(10);
		while (target.Latest.Source.Length < length)
		{
			if (DateTime.UtcNow > deadline)
			{
				throw new TimeoutException($"目标源码未到达期望长度（当前 {target.Latest.Source.Length}，期望 {length}）");
			}

			Thread.Sleep(5);
		}
	}

	private static string Pump(MarkdownTypewriterStream stream, int frames)
	{
		for (var i = 0; i < frames; i++)
		{
			stream.Tick();
			Thread.Sleep(1); // 手动泵下时间仍需前进，驱动释放速率
		}

		return string.Empty;
	}

	[Fact]
	public void Deltas_FlowPaced_AndCompleteAutomatically()
	{
		using var target = new MarkdownStreamingDocument();
		using var stream = MarkdownTypewriterStream.AttachManual(target, TimeSpan.FromMilliseconds(20));

		stream.AppendDelta("# 标题\n\n");
		stream.AppendDelta("第一段内容，含 **粗体** 与 `code`。\n\n");
		stream.AppendDelta("- 列表甲\n- 列表乙\n");
		stream.Finish();

		Pump(stream, 400);
		WaitForSourceLength(target, stream.Target.Latest.Source.Length);

		Assert.True(stream.IsCompleted);
		Assert.True(target.Latest.IsComplete);
		Assert.Equal("# 标题\n\n第一段内容，含 **粗体** 与 `code`。\n\n- 列表甲\n- 列表乙\n", target.Latest.Source);
	}

	[Fact]
	public void Burst_Delta_ReleasesProgressively_NotAllAtOnce()
	{
		using var target = new MarkdownStreamingDocument();
		using var stream = MarkdownTypewriterStream.AttachManual(target, TimeSpan.FromMilliseconds(120));

		stream.AppendDelta(new string('x', 300));
		stream.Tick(); // 预热帧
		for (var i = 0; i < 4; i++)
		{
			Thread.Sleep(5); // 帧间真实时间流逝，驱动释放速率
			stream.Tick();
		}

		WaitForSourceLength(target, 1);
		var afterFourFrames = target.Latest.Source.Length;

		// 平滑：多帧渐进释放，而不是第一帧整段放下（300 字符低于 4096 直通阈值）。
		Assert.InRange(afterFourFrames, 1, 299);
		Pump(stream, 200);
		WaitForSourceLength(target, 300);
		Assert.Equal(300, target.Latest.Source.Length);
	}

	[Fact]
	public void Finish_BeforeTicks_StillCompletesAndDrains()
	{
		using var target = new MarkdownStreamingDocument();
		using var stream = MarkdownTypewriterStream.AttachManual(target, TimeSpan.FromMilliseconds(5));

		stream.AppendDelta("内容甲。");
		stream.Finish();
		stream.AppendDelta("内容乙。"); // Finish 后到达：忽略

		Pump(stream, 200);
		WaitForSourceLength(target, "内容甲。".Length);

		Assert.True(stream.IsCompleted);
		WaitForSourceLength(target, 4);
		Assert.Equal("内容甲。", target.Latest.Source);
	}

	[Fact]
	public void Abort_DropsBacklog_AndCompletesWithoutIt()
	{
		// 10 秒目标滞留：泵帧时间尺度内不会有任何字符释放。
		using var target = new MarkdownStreamingDocument();
		using var stream = MarkdownTypewriterStream.AttachManual(target, TimeSpan.FromSeconds(10));

		stream.AppendDelta("已释放。");
		stream.Tick();
		Pump(stream, 3);
		Assert.Equal(string.Empty, target.Latest.Source);

		stream.AppendDelta("这部分应被丢弃。" + new string('z', 100));
		stream.Abort();
		WaitForComplete(target);

		Assert.True(stream.IsCompleted);
		Assert.True(target.Latest.IsComplete);
		Assert.Equal(string.Empty, target.Latest.Source);
	}

	[Fact]
	public void OversizedDelta_BypassesPacing()
	{
		using var target = new MarkdownStreamingDocument();
		using var stream = MarkdownTypewriterStream.AttachManual(target, TimeSpan.FromMilliseconds(120));

		var big = new StringBuilder();
		for (var i = 0; i < 250; i++)
		{
			big.AppendLine($"第 {i} 行内容，用于超过直通阈值。");
		}

		Assert.True(big.Length > 4096);
		stream.AppendDelta(big.ToString());
		stream.Tick(); // 直通：首帧整段放行
		WaitForSourceLength(target, big.Length);

		Assert.Equal(big.Length, target.Latest.Source.Length); // 粘贴/大分片整段直放

		stream.Finish();
		Pump(stream, 60);
		Assert.True(stream.IsCompleted);
	}

	[Fact]
	public void MultipleBatches_PreserveOrderAcrossFrames()
	{
		using var target = new MarkdownStreamingDocument();
		using var stream = MarkdownTypewriterStream.AttachManual(target, TimeSpan.FromMilliseconds(10));
		var random = new Random(2026);
		var pushed = new StringBuilder();

		for (var batch = 0; batch < 50; batch++)
		{
			var chunk = new string((char)('A' + batch % 26), random.Next(5, 60));
			pushed.Append(chunk);
			stream.AppendDelta(chunk);
			Thread.Sleep(2); // 帧间真实时间流逝，驱动释放速率
			stream.Tick();
		}

		stream.Finish();
		Pump(stream, 400);
		WaitForSourceLength(target, pushed.Length);

		Assert.True(stream.IsCompleted);
		Assert.Equal(pushed.ToString(), target.Latest.Source);
	}
}
