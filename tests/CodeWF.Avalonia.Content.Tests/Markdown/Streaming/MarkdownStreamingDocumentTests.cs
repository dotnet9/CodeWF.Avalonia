using CodeWF.Avalonia.Markdown.Shared.Rendering;
using CodeWF.Avalonia.Markdown.Streaming;

using Markdig;

using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Streaming;

/// <summary>
/// 流式文档行为测试：追加/完成/重置协议、冻结块共享、冻结区等价性冒烟
/// （完整等价性验证矩阵见 StreamingEquivalenceTests）。
/// </summary>
public sealed class MarkdownStreamingDocumentTests
{
	private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
		.UseAdvancedExtensions()
		.Build();

	private const string Sample =
		"# 标题\n\n段落甲\n\n- 列表项\n\n```csharp\nvar x = 1;\n```\n\n**结尾** 段落\n";

	[Fact]
	public async Task Append_Chunks_ProducesSnapshotsWithGrowingSource()
	{
		using var document = new MarkdownStreamingDocument();
		var snapshotCount = 0;
		document.SnapshotReady += (_, _) => snapshotCount++;

		var appended = 0;
		for (var step = 0; step < Sample.Length; step += 5)
		{
			var end = Math.Min(step + 5, Sample.Length);
			document.Append(Sample[appended..end]);
			appended = end;
			await WaitForAsync(() => document.Latest.Source.Length >= end);
		}

		Assert.True(snapshotCount > 0);
		Assert.Equal(Sample, document.Latest.Source);
		Assert.False(document.Latest.IsComplete);
	}

	[Fact]
	public async Task Complete_PublishesFinalFullParseSnapshot()
	{
		using var document = new MarkdownStreamingDocument();
		var completed = new TaskCompletionSource();
		document.Completed += (_, _) => completed.TrySetResult();

		document.Append(Sample);
		await WaitForAsync(() => document.Latest.Source.Length == Sample.Length);
		document.Complete();

		await WaitForAsync(() => document.Latest.IsComplete);
		await completed.Task.WaitAsync(TimeSpan.FromSeconds(5));

		Assert.Equal(Sample, document.Latest.Source);
		Assert.Equal(Sample.Length, document.Latest.FrozenOffset);

		var expected = MarkdownParser.Parse(Sample, Pipeline);
		Assert.Equal(expected.Blocks.Count, document.Latest.Model.Blocks.Count);
	}

	[Fact]
	public void Append_AfterComplete_Throws()
	{
		using var document = new MarkdownStreamingDocument();
		document.Append("内容\n");
		document.Complete();

		Assert.Throws<InvalidOperationException>(() => document.Append("更多"));
	}

	[Fact]
	public async Task Reset_StartsNewGeneration()
	{
		using var document = new MarkdownStreamingDocument();
		document.Append(Sample);
		await WaitForAsync(() => document.Latest.Source.Length == Sample.Length);

		var firstGeneration = document.Latest.Generation;
		document.Reset("# 新文档\n");

		await WaitForAsync(() => document.Latest.Generation > firstGeneration
			&& document.Latest.Source == "# 新文档\n");

		Assert.Equal(firstGeneration + 1, document.Latest.Generation);
		Assert.Single(document.Latest.Blocks);
	}

	[Fact]
	public async Task FrozenBlocks_AreSharedAcrossSnapshots()
	{
		using var document = new MarkdownStreamingDocument();
		var observed = new List<MarkdownStreamingSnapshot>();
		document.SnapshotReady += (_, snapshot) => observed.Add(snapshot);

		document.Reset(Sample[..20]);
		await WaitForAsync(() => document.Latest.Source.Length == 20);

		var appended = 20;
		for (var end = 23; end <= Sample.Length; end += 3)
		{
			var actualEnd = Math.Min(end, Sample.Length);
			document.Append(Sample[appended..actualEnd]);
			appended = actualEnd;
			await WaitForAsync(() => document.Latest.Source.Length >= actualEnd);
		}

		Assert.NotEmpty(observed);
		var previousFrozenCount = 0;
		var previousFrozenOffset = 0;
		var previousFrozen = Array.Empty<MarkdownDocumentBlock>();
		foreach (var snapshot in observed)
		{
			for (var i = 0; i < previousFrozenCount; i++)
			{
				Assert.True(ReferenceEquals(previousFrozen[i], snapshot.FrozenBlocks[i]),
					"冻结块实例必须跨快照共享");
			}

			Assert.True(snapshot.FrozenOffset >= previousFrozenOffset, "冻结边界单调不减");
			previousFrozenCount = snapshot.FrozenBlocks.Count;
			previousFrozenOffset = snapshot.FrozenOffset;
			previousFrozen = snapshot.FrozenBlocks.ToArray();
		}
	}

	[Fact]
	public async Task FrozenRegion_MatchesFullParseOfPrefix()
	{
		// 冻结区等价性冒烟（不变量 I4）：每个快照中完全落在冻结区内的块，
		// 其 kind/span/哈希必须与整篇解析对应前缀一致。
		using var document = new MarkdownStreamingDocument();
		var violations = new List<string>();
		document.SnapshotReady += (_, snapshot) =>
		{
			var prefix = snapshot.Source[..snapshot.FrozenOffset];
			var fullParse = MarkdownParser.Parse(prefix, Pipeline);
			var frozenBlocks = snapshot.Blocks
				.TakeWhile(block => block.SourceSpan.End <= snapshot.FrozenOffset)
				.ToList();

			var mismatches = frozenBlocks
				.Zip(fullParse.Blocks, (streamed, full) => (streamed, full))
				.Where(pair => pair.streamed.Kind != pair.full.Kind
					|| pair.streamed.SourceSpan != pair.full.SourceSpan
					|| pair.streamed.ContentHash != pair.full.ContentHash)
				.ToList();
			if (frozenBlocks.Count != fullParse.Blocks.Count || mismatches.Count > 0)
			{
				violations.Add($"冻结区 {frozenBlocks.Count} 块 vs 整篇解析 {fullParse.Blocks.Count} 块（不匹配 {mismatches.Count}）");
			}
		};

		var appended = 0;
		for (var step = 0; step < Sample.Length; step += 4)
		{
			var end = Math.Min(step + 4, Sample.Length);
			document.Append(Sample[appended..end]);
			appended = end;
			await WaitForAsync(() => document.Latest.Source.Length >= end);
		}

		document.Complete();
		await WaitForAsync(() => document.Latest.IsComplete);

		Assert.Empty(violations);
	}

	[Fact]
	public async Task SnapshotsForExpiredVersions_AreSkipped()
	{
		// 连续快速追加时，中间版本的快照允许跳过（合并协议），但最终快照必须是最新版本。
		using var document = new MarkdownStreamingDocument();
		var appended = 0;
		for (var step = 0; step < Sample.Length; step += 7)
		{
			var end = Math.Min(step + 7, Sample.Length);
			document.Append(Sample[appended..end]);
			appended = end;
		}

		await WaitForAsync(() => document.Latest.Source.Length == Sample.Length);

		Assert.Equal(Sample, document.Latest.Model.Source);
	}

	private static async Task WaitForAsync(Func<bool> condition, int timeoutMilliseconds = 5000)
	{
		var start = DateTime.UtcNow;
		while (!condition())
		{
			if (DateTime.UtcNow - start > TimeSpan.FromMilliseconds(timeoutMilliseconds))
			{
				throw new TimeoutException("等待流式快照超时");
			}

			await Task.Delay(10);
		}
	}
}
