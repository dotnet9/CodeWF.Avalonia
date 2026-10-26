using CodeWF.Avalonia.Markdown.Rendering;

using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Rendering;

/// <summary>
/// 高亮行级缓存会话测试：前缀复用（只重算分叉后的行）、跨行状态链延续、
/// 缩短/清空与 CRLF 归一。
/// </summary>
public sealed class MarkdownCodeHighlightSessionTests
{
	private sealed class CountingTokenizer
	{
		public int InvocationCount;
		public List<(string Line, object? StartState)> Calls = [];

		public object? Tokenize(string line, object? startState, out object? endState)
		{
			InvocationCount++;
			Calls.Add((line, startState));
			endState = $"state:{line}"; // 伪状态：可断言状态链传递
			return $"tokens:{line}";
		}
	}

	[Fact]
	public void Append_GrowsTail_ReusesPrefixLines()
	{
		var session = new MarkdownCodeHighlightSession();
		var tokenizer = new CountingTokenizer();

		session.Update("int a;\nint b;\nint c;", tokenizer.Tokenize);
		Assert.Equal(3, tokenizer.InvocationCount);

		var second = session.Update("int a;\nint b;\nint c;\nint d;", tokenizer.Tokenize);
		Assert.Equal(4, tokenizer.InvocationCount); // 只重算新增行
		Assert.Equal(4, second.Count);
		Assert.Equal("tokens:int d;", second[3]);
		Assert.Equal("tokens:int a;", second[0]); // 前缀行复用缓存
	}

	[Fact]
	public void MidTextChange_RetokenizesFromDivergence_WithContinuedState()
	{
		var session = new MarkdownCodeHighlightSession();
		var tokenizer = new CountingTokenizer();

		session.Update("a\nb\nc\nd", tokenizer.Tokenize);
		Assert.Equal(4, tokenizer.InvocationCount);

		var second = session.Update("a\nB\nc\nd", tokenizer.Tokenize);
		Assert.Equal(7, tokenizer.InvocationCount); // 只重算 B/c/d 三行（4 + 3）
		Assert.Equal("tokens:B", second[1]);

		// 重算首行以保留前缀（a）的结束状态为起点，跨行状态链不断。
		Assert.Equal(("B", "state:a"), (tokenizer.Calls[4].Line, tokenizer.Calls[4].StartState));
		Assert.Equal(("c", "state:B"), (tokenizer.Calls[5].Line, tokenizer.Calls[5].StartState));
	}

	[Fact]
	public void SameCode_SecondUpdate_ReusesEverything()
	{
		var session = new MarkdownCodeHighlightSession();
		var tokenizer = new CountingTokenizer();

		session.Update("x\ny", tokenizer.Tokenize);
		var second = session.Update("x\ny", tokenizer.Tokenize);

		Assert.Equal(2, tokenizer.InvocationCount);
		Assert.Equal(2, second.Count);
	}

	[Fact]
	public void ShrunkCode_TruncatesCache_AndRegrowsRetokenizes()
	{
		var session = new MarkdownCodeHighlightSession();
		var tokenizer = new CountingTokenizer();

		session.Update("a\nb\nc", tokenizer.Tokenize);
		var shrunk = session.Update("a", tokenizer.Tokenize);
		Assert.Equal(3, tokenizer.InvocationCount);
		Assert.Single(shrunk);
		Assert.Equal(1, session.CachedLineCount);

		var regrown = session.Update("a\nb", tokenizer.Tokenize);
		Assert.Equal(4, tokenizer.InvocationCount); // a 复用，b 重算（3 + 1）
		Assert.Equal("tokens:b", regrown[1]);
	}

	[Fact]
	public void Reset_ForcesFullRetokenize()
	{
		var session = new MarkdownCodeHighlightSession();
		var tokenizer = new CountingTokenizer();

		session.Update("a\nb", tokenizer.Tokenize);
		session.Reset();
		session.Update("a\nb", tokenizer.Tokenize);

		Assert.Equal(4, tokenizer.InvocationCount);
	}

	[Fact]
	public void Crlf_IsNormalizedBeforeSplit()
	{
		var session = new MarkdownCodeHighlightSession();
		var tokenizer = new CountingTokenizer();

		var lines = session.Update("a\r\nb", tokenizer.Tokenize);

		Assert.Equal(2, lines.Count);
		Assert.Equal("tokens:a", lines[0]);
		Assert.Equal("tokens:b", lines[1]);
	}

	[Fact]
	public void EmptyCode_YieldsSingleEmptyLine()
	{
		var session = new MarkdownCodeHighlightSession();
		var tokenizer = new CountingTokenizer();

		var lines = session.Update(string.Empty, tokenizer.Tokenize);

		Assert.Single(lines);
		Assert.Equal("tokens:", lines[0]);
	}

	[Fact]
	public void NullTokens_Throws()
	{
		var session = new MarkdownCodeHighlightSession();
		Assert.Throws<InvalidOperationException>(() =>
			session.Update("a", (string line, object? start, out object? end) =>
			{
				end = null;
				return null;
			}));
	}
}
