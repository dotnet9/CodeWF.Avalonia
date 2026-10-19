using CodeWF.Avalonia.Markdown.Shared.Rendering;
using CodeWF.Avalonia.Markdown.Streaming;

using Markdig;

using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Streaming;

/// <summary>
/// 流式等价性不变量矩阵（docs/流式规则.md §3.5）：
///
/// I4  任意分片序列下，每个快照中完全落在冻结区内的块，与该前缀整篇解析
///     的对应块（kind + 源码 span + 内容哈希）完全一致；
/// I1  冻结边界单调不减；
/// I2  冻结边界永不越过末块（尾部至少保留一个未冻结块——边界 ≤ 文档末尾）；
/// I3  Reset 开新代后全部重新开始；
/// 共享 冻结块实例跨快照引用相同。
/// </summary>
public sealed class StreamingEquivalenceTests
{
	private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
		.UseAdvancedExtensions()
		.Build();

	// 覆盖冻结规则全部分支的语料：标题/段落/松紧列表/围栏/引用/表格/setext/
	// 主题分隔线/数学块/HTML 原始块/链接引用定义/定义列表/任务列表/脚注。
	public static TheoryData<string> Corpus => new()
	{
		"# 标题\n\n段落甲\n\n段落乙\n",
		"- 甲\n- 乙\n\n1. 一\n2. 二\n\n之后\n",
		"- 甲\n\n  缩进续行\n\n之后\n",
		"代码：\n\n```csharp\nvar x = 1;\n\nvar y = 2;\n```\n\n之后\n",
		"~~~\n波浪围栏\n~~~\n\n之后\n",
		"> 引用甲\n> 引用乙\n\n正文\n",
		"> 引用甲\n\n> 引用乙\n\n正文\n",
		"| 甲 | 乙 |\n| --- | --- |\n| 1 | 2 |\n\n之后\n",
		"标题甲\n===\n标题乙\n---\n\n正文\n",
		"段落\n---\n\n分隔线后\n",
		"$$\nE = mc^2\n$$\n\n公式后\n",
		"$$E = mc^2$$\n\n公式后\n",
		"<div>\nhtml 内容\n</div>\n\n之后\n",
		"<pre>\n原始文本\n\n保持原样\n</pre>\n\n之后\n",
		"段落\n<div>\nhtml 打断失败仍属段落\n</div>\n",
		"<div>\n内容甲\n===\n内容乙\n\n之后\n",
		"<div>\n***\n内容\n\n之后\n",
		"[引用]: https://example.com\n\n正文使用 [引用][]。\n",
		"术语\n: 定义内容\n\n正文\n",
		"- [ ] 任务甲\n- [x] 任务乙\n\n脚注[^1]\n\n[^1]: 注释内容\n",
		"文字 `代码` 更多 **粗体** 与 [链接](https://example.com)。\n\n---\n\n结尾\n",
		"# 甲\n\n## 乙\n\n段落\n\n- 列表\n\n```text\n代码\n```\n\n> 引用\n\n| 表 | 格 |\n| --- | --- |\n| 1 | 2 |\n\n结尾\n",
	};

	[Theory]
	[MemberData(nameof(Corpus))]
	public void FixedChunkSizes_PreserveEquivalence(string document)
	{
		foreach (var chunkSize in new[] { 1, 3, 7, 32, 500 })
		{
			VerifyStreaming(document, ChunkEvenly(document, chunkSize), $"固定分片 {chunkSize}");
		}
	}

	[Theory]
	[MemberData(nameof(Corpus))]
	public void RandomChunkSizes_PreserveEquivalence(string document)
	{
		for (var seed = 0; seed < 8; seed++)
		{
			VerifyStreaming(document, ChunkRandomly(document, new Random(seed)), $"随机分片 seed={seed}");
		}
	}

	[Fact]
	public void Complete_EqualsFullParse()
	{
		foreach (var document in Corpus)
		{
			using var streaming = new MarkdownStreamingDocument();
			FeedAll(streaming, document, 5);
			WaitForVersion(streaming, streaming.Latest.Version);
			streaming.Complete();
			WaitForComplete(streaming);

			var expected = MarkdownParser.Parse(document, Pipeline);
			Assert.True(streaming.Latest.IsComplete);
			Assert.Equal(expected.Blocks.Count, streaming.Latest.Model.Blocks.Count);
			for (var i = 0; i < expected.Blocks.Count; i++)
			{
				var streamed = streaming.Latest.Model.Blocks[i];
				var full = expected.Blocks[i];
				Assert.True(streamed.Kind == full.Kind
					&& streamed.SourceSpan == full.SourceSpan
					&& streamed.ContentHash == full.ContentHash,
					$"终局块 {i} 与整篇解析不一致：{document}");
			}
		}
	}

	private static void VerifyStreaming(string document, IReadOnlyList<string> chunks, string label)
	{
		using var streaming = new MarkdownStreamingDocument();
		var previousFrozenOffset = 0;
		var previousFrozen = Array.Empty<MarkdownDocumentBlock>();
		var offset = 0;

		for (var index = 0; index < chunks.Count; index++)
		{
			streaming.Append(chunks[index]);
			offset += chunks[index].Length;
			WaitForVersion(streaming, index + 1);

			var snapshot = streaming.Latest;

			// I1 边界单调。
			Assert.True(snapshot.FrozenOffset >= previousFrozenOffset, $"{label}：边界回退 @{offset}");

			// 共享：已见过的冻结块引用不变。
			for (var i = 0; i < previousFrozen.Length && i < snapshot.FrozenBlocks.Count; i++)
			{
				Assert.True(ReferenceEquals(previousFrozen[i], snapshot.FrozenBlocks[i]),
					$"{label}：冻结块实例被重建 @{offset}");
			}

			// I4 冻结区与"冻结前缀整篇解析"等价（尾部未冻结块不参与对比）。
			var prefix = document[..snapshot.FrozenOffset];
			var prefixParse = MarkdownParser.Parse(prefix, Pipeline);
			var frozenInView = snapshot.Model.Blocks
				.TakeWhile(block => block.SourceSpan.End <= snapshot.FrozenOffset)
				.ToList();
			Assert.True(frozenInView.Count == prefixParse.Blocks.Count,
				$"{label}：冻结区 {frozenInView.Count} 块 vs 前缀整篇解析 {prefixParse.Blocks.Count} 块 @{offset}");
			for (var i = 0; i < prefixParse.Blocks.Count; i++)
			{
				var streamed = frozenInView[i];
				var full = prefixParse.Blocks[i];
				Assert.True(streamed.Kind == full.Kind
					&& streamed.SourceSpan == full.SourceSpan
					&& streamed.ContentHash == full.ContentHash,
					$"{label}：块 {i} 不一致 @{offset}");
			}

			previousFrozenOffset = snapshot.FrozenOffset;
			previousFrozen = snapshot.FrozenBlocks.ToArray();
		}

		// I2 尾部保留：完整文档流完后至少最后一个块仍在未冻结区，或已终局。
		Assert.True(streaming.Latest.Source.Length == document.Length, $"{label}：源码不完整");
	}

	private static IReadOnlyList<string> ChunkEvenly(string document, int chunkSize)
	{
		var chunks = new List<string>();
		for (var offset = 0; offset < document.Length; offset += chunkSize)
		{
			chunks.Add(document[offset..Math.Min(offset + chunkSize, document.Length)]);
		}

		return chunks;
	}

	private static IReadOnlyList<string> ChunkRandomly(string document, Random random)
	{
		var chunks = new List<string>();
		var offset = 0;
		while (offset < document.Length)
		{
			var take = Math.Min(random.Next(1, document.Length), document.Length - offset);
			chunks.Add(document[offset..(offset + take)]);
			offset += take;
		}

		return chunks;
	}

	private static void FeedAll(MarkdownStreamingDocument streaming, string document, int chunkSize)
	{
		foreach (var chunk in ChunkEvenly(document, chunkSize))
		{
			streaming.Append(chunk);
		}
	}

	private static void WaitForComplete(MarkdownStreamingDocument streaming)
	{
		var deadline = DateTime.UtcNow.AddSeconds(10);
		while (!streaming.Latest.IsComplete)
		{
			if (DateTime.UtcNow > deadline)
			{
				throw new TimeoutException("等待终局快照超时");
			}

			Thread.Sleep(5);
		}
	}

	private static void WaitForVersion(MarkdownStreamingDocument streaming, long version)
	{
		var deadline = DateTime.UtcNow.AddSeconds(10);
		while (streaming.Latest.Version < version)
		{
			if (DateTime.UtcNow > deadline)
			{
				throw new TimeoutException($"等待版本 {version} 超时（当前 {streaming.Latest.Version}）");
			}

			Thread.Sleep(5);
		}
	}
}
