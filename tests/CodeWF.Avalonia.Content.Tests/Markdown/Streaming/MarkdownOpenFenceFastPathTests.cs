using CodeWF.Avalonia.Markdown.Shared.Rendering;
using CodeWF.Avalonia.Markdown.Streaming;

using Markdig;
using Markdig.Syntax;

using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Streaming;

/// <summary>
/// 开放围栏快速路径测试（规格：docs/流式规则.md §4.4）：
/// 尾块与「补全闭合围栏后 Markdig 解析」逐字段一致（Q3），流式全程块序列
/// 与终局整篇解析一致。
/// </summary>
public sealed class MarkdownOpenFenceFastPathTests
{
	private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
		.UseAdvancedExtensions()
		.Build();

	[Theory]
	[InlineData("```python\nvar x = 1;")]
	[InlineData("```python\nvar x = 1;\n")]
	[InlineData("```python\nvar x = 1;\nvar y = 2;")]
	[InlineData("```python")]
	[InlineData("```python\n")]
	[InlineData("```py x\na")]
	[InlineData("```py\na\n\nb")]
	[InlineData("```py\na\n\n")]
	[InlineData("  ```py\n  indent();\n  more()")]
	[InlineData("~~~\n波浪围栏内容")]
	public void OpenFenceTail_MatchesRemendedParse_FieldByField(string tail)
	{
		using var streaming = new MarkdownStreamingDocument();
		streaming.Append(tail);
		WaitForVersion(streaming, 1);

		var snapshot = streaming.Latest;
		var fast = Assert.IsType<MarkdownDocumentBlock>(snapshot.Model.Blocks.Single());
		var fastFenced = Assert.IsType<FencedCodeBlock>(fast.SyntaxBlock);

		// 常规路径参照：补全闭合围栏后整篇解析（围栏行首冻结 → 唯一块即尾块）。
		var remend = MarkdownTailRemender.Apply(tail);
		var expected = MarkdownParser.Parse(remend.RemendedText, Pipeline).Blocks.Single();
		var expectedFenced = Assert.IsType<FencedCodeBlock>(expected.SyntaxBlock);

		Assert.Equal(MarkdownBlockKind.Code, fast.Kind);
		Assert.Equal(remend.RemendedText[expectedFenced.Span.Start..], fast.SourceText);
		Assert.Equal(expected.ContentHash, fast.ContentHash);
		Assert.Equal(expected.PlainText, fast.PlainText);
		Assert.Equal(expected.DependencyFlags, fast.DependencyFlags);
		Assert.Equal(expected.IsHeavy, fast.IsHeavy);
		Assert.Equal(
			new MarkdownTextSpan(expectedFenced.Span.Start, tail.Length),
			fast.SourceSpan);

		// 逐字段对齐的三个关键面：行内容、info string、补全文本内坐标系。
		Assert.Equal(expectedFenced.Lines.ToString(), fastFenced.Lines.ToString());
		Assert.Equal(expectedFenced.Lines.Count, fastFenced.Lines.Count);
		Assert.Equal(expectedFenced.Info, fastFenced.Info);
		Assert.Equal(expectedFenced.Span, fastFenced.Span);
	}

	[Fact]
	public void FenceCloseAsLastLine_FallsBackToRemendPath()
	{
		// 末行已是完整闭合围栏（未换行）：不命中快速路径，快照即完整代码块。
		var tail = "```python\nvar x = 1;\n```";
		using var streaming = new MarkdownStreamingDocument();
		streaming.Append(tail);
		WaitForVersion(streaming, 1);

		var snapshot = streaming.Latest;
		var block = snapshot.Model.Blocks.Single();
		Assert.Equal(MarkdownBlockKind.Code, block.Kind);

		var expected = MarkdownParser.Parse(tail, Pipeline).Blocks.Single();
		Assert.Equal(expected.ContentHash, block.ContentHash);
		Assert.Equal(tail, block.SourceText);
	}

	[Fact]
	public void StreamingThroughFence_BlocksStayEquivalent_AndCompleteMatchesFullParse()
	{
		var document = "前言\n\n```csharp\nint a;\n\nint b;\n```\n\n后记\n";
		using var streaming = new MarkdownStreamingDocument();
		var sawCodeTail = false;
		var appends = 0;

		for (var offset = 0; offset < document.Length; offset += 2)
		{
			var end = Math.Min(offset + 2, document.Length);
			streaming.Append(document[offset..end]);
			appends++;
			WaitForVersion(streaming, appends);

			var snapshot = streaming.Latest;
			if (snapshot.Source.EndsWith("int a;\n\nint b;", StringComparison.Ordinal))
			{
				// 开放围栏阶段：尾块是代码块，明文随内容增长（PlainText 经提取器归一换行）。
				var last = snapshot.Model.Blocks[^1];
				Assert.Equal(MarkdownBlockKind.Code, last.Kind);
				Assert.Equal("int a;\n\nint b;", last.PlainText.Replace("\r\n", "\n"));
				sawCodeTail = true;
			}
		}

		Assert.True(sawCodeTail, "开放围栏阶段应出现代码尾块");

		streaming.Complete();
		WaitForComplete(streaming);

		var final = streaming.Latest.Model;
		var expectedModel = MarkdownParser.Parse(document, Pipeline);
		Assert.Equal(expectedModel.Blocks.Count, final.Blocks.Count);
		for (var i = 0; i < expectedModel.Blocks.Count; i++)
		{
			Assert.Equal(expectedModel.Blocks[i].Kind, final.Blocks[i].Kind);
			Assert.Equal(expectedModel.Blocks[i].SourceSpan, final.Blocks[i].SourceSpan);
			Assert.Equal(expectedModel.Blocks[i].ContentHash, final.Blocks[i].ContentHash);
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
