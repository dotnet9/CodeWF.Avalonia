using System.Diagnostics;
using System.Text;

using CodeWF.Avalonia.Markdown.Shared.Rendering;

using Markdig;

namespace CodeWF.Avalonia.Markdown.Benchmarks;

/// <summary>
/// Markdown 解析基准（阶段 0 基线 + 流式内核前后对照）。
///
/// 用法：
///   dotnet run -c Release --            全部场景
///   dotnet run -c Release -- full       仅整篇解析
///   dotnet run -c Release -- stream     仅逐 token 追加（现状管线语义）
///
/// "现状管线"指编辑驱动路径：每次文本变化都对整篇文本重新 Markdig 解析，
/// 再做块级 diff。diff 只省渲染，解析始终 O(全文)——这正是流式内核要消除的部分。
/// </summary>
internal static class Program
{
	private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
		.UseAdvancedExtensions()
		.Build();

	public static int Main(string[] args)
	{
		var mode = args.Length > 0 ? args[0].ToLowerInvariant() : "all";
		var results = new List<string>();

		if (mode is "all" or "full")
		{
			foreach (var size in new[] { 5, 20, 100 })
			{
				results.Add(BenchFullParse(size));
			}
		}

		if (mode is "all" or "stream")
		{
			results.Add(BenchTokenStreaming(20, tokenLength: 4));
			results.Add(BenchTokenStreaming(20, tokenLength: 32));
		}

		Console.WriteLine();
		Console.WriteLine("==== 基准结果 ====");
		foreach (var line in results)
		{
			Console.WriteLine(line);
		}

		return 0;
	}

	/// <summary>整篇解析：固定文档解析 iterations 次，报平均/最小耗时与每次分配。</summary>
	private static string BenchFullParse(int sizeKb)
	{
		var document = DocumentBuilder.Build(sizeKb * 1024);
		const int iterations = 30;

		// 预热（JIT + 缓存），不计入。
		MarkdownParser.Parse(document, Pipeline);

		var min = TimeSpan.MaxValue;
		var total = TimeSpan.Zero;
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();
		var allocatedBefore = GC.GetTotalAllocatedBytes(precise: true);

		for (var i = 0; i < iterations; i++)
		{
			var start = Stopwatch.GetTimestamp();
			_ = MarkdownParser.Parse(document, Pipeline);
			var elapsed = Stopwatch.GetElapsedTime(start);
			total += elapsed;
			if (elapsed < min)
			{
				min = elapsed;
			}
		}

		var allocated = GC.GetTotalAllocatedBytes(precise: true) - allocatedBefore;
		return $"整篇解析 {sizeKb,4} KB | 平均 {total.TotalMilliseconds / iterations,8:0.00} ms | 最小 {min.TotalMilliseconds,8:0.00} ms | 分配 {allocated / iterations / 1024.0,9:0.0} KB/次";
	}

	/// <summary>
	/// 逐 token 追加（现状管线语义）：文档从空按 tokenLength 字符增长到目标大小，
	/// 每步 substring 出前缀 → 整篇解析 → 块级 diff。统计总耗时、步均耗时与总分配。
	/// </summary>
	private static string BenchTokenStreaming(int sizeKb, int tokenLength)
	{
		var document = DocumentBuilder.Build(sizeKb * 1024);
		var steps = document.Length / tokenLength;

		// 预热：先跑一遍完整流程（不含计时）。
		RunTokenStreaming(document, tokenLength);

		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();
		var allocatedBefore = GC.GetTotalAllocatedBytes(precise: true);

		var (total, diffTotal) = RunTokenStreaming(document, tokenLength);

		var allocated = GC.GetTotalAllocatedBytes(precise: true) - allocatedBefore;
		return $"逐 token 追加 {sizeKb,4} KB / {tokenLength,2} 字符 | 步数 {steps,6} | 总耗时 {total.TotalMilliseconds,9:0} ms | 步均 {total.TotalMilliseconds / steps,7:0.00} ms（diff 占 {diffTotal.TotalMilliseconds / total.TotalMilliseconds * 100,4:0}%）| 总分配 {allocated / 1024.0 / 1024.0,8:0.0} MB";
	}

	private static (TimeSpan Total, TimeSpan Diff) RunTokenStreaming(string document, int tokenLength)
	{
		var total = TimeSpan.Zero;
		var diffTotal = TimeSpan.Zero;
		MarkdownDocumentModel? previous = null;

		for (var end = tokenLength; end <= document.Length; end += tokenLength)
		{
			var start = Stopwatch.GetTimestamp();
			var prefix = document[..end];
			var model = MarkdownParser.Parse(prefix, Pipeline);
			if (previous is not null)
			{
				var diffStart = Stopwatch.GetTimestamp();
				_ = MarkdownDiffService.Compare(previous, model);
				diffTotal += Stopwatch.GetElapsedTime(diffStart);
			}

			total += Stopwatch.GetElapsedTime(start);
			previous = model;
		}

		return (total, diffTotal);
	}
}

/// <summary>
/// 生成确定性的代表性文档：标题/段落/列表/表格/代码块/引用/任务列表按比例循环，
/// 内容由种子随机填充保证不同块文本不同（避免哈希退化为常量）。
/// </summary>
internal static class DocumentBuilder
{
	public static string Build(int targetLength, int seed = 20261019)
	{
		var builder = new StringBuilder(targetLength + 1024);
		var random = new Random(seed);
		var chapter = 0;
		while (builder.Length < targetLength)
		{
			chapter++;
			builder.AppendLine($"# 第 {chapter} 章 {RandomWords(random, 4)}");
			for (var section = 1; section <= 3 && builder.Length < targetLength; section++)
			{
				builder.AppendLine($"## {chapter}.{section} {RandomWords(random, 3)}");
				builder.AppendLine(RandomSentence(random, 80));
				builder.AppendLine();
				builder.AppendLine(RandomSentence(random, 120));
				builder.AppendLine();
				builder.AppendLine($"- {RandomWords(random, 6)}");
				builder.AppendLine($"- {RandomWords(random, 6)}");
				builder.AppendLine($"  - {RandomWords(random, 4)}");
				builder.AppendLine();
				builder.AppendLine($"| 列一 | 列二 | 列三 |");
				builder.AppendLine($"| --- | :--: | ---: |");
				builder.AppendLine($"| {RandomWords(random, 3)} | {random.Next(1000, 9999)} | {RandomWords(random, 2)} |");
				builder.AppendLine($"| {RandomWords(random, 3)} | {random.Next(1000, 9999)} | {RandomWords(random, 2)} |");
				builder.AppendLine();
				builder.AppendLine($"```csharp");
				for (var line = 0; line < 6; line++)
				{
					builder.AppendLine($"public void Method{chapter}_{section}_{line}() => Console.WriteLine(\"{RandomWords(random, 3)}\");");
				}

				builder.AppendLine($"```");
				builder.AppendLine();
				builder.AppendLine($"> {RandomSentence(random, 50)}");
				builder.AppendLine();
				builder.AppendLine($"- [ ] {RandomWords(random, 5)}");
				builder.AppendLine($"- [x] {RandomWords(random, 5)}");
				builder.AppendLine();
			}
		}

		// 循环保证 builder.Length >= targetLength（targetLength > 0 时）
		return builder.ToString(0, Math.Min(targetLength, builder.Length));
	}

	private static string RandomWords(Random random, int count)
	{
		const string chars = "abcdefghijklmnopqrstuvwxyz";
		return new string(Enumerable.Range(0, count)
			.Select(_ => chars[random.Next(chars.Length)])
			.ToArray());
	}

	private static string RandomSentence(Random random, int wordCount)
	{
		return string.Join(' ', Enumerable.Range(0, wordCount / 4).Select(_ => RandomWords(random, 3 + random.Next(5))));
	}
}
