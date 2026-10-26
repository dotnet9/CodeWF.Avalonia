using Markdig;
using Markdig.Syntax;

using Xunit;
using Xunit.Abstractions;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Streaming;

public sealed class MarkdigSlicesProbeTests
{
	private readonly ITestOutputHelper _output;
	public MarkdigSlicesProbeTests(ITestOutputHelper output) => _output = output;

	private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
		.UseAdvancedExtensions()
		.Build();

	[Fact]
	public void Probe()
	{
		foreach (var source in new[]
		         {
			         "```py\na\n```",
			         "```py\na\n\n```",
			         "```python\nvar x = 1;\n\n```",
			         "```py\na\n\n\n```",
			         "```python\n```",
		         })
		{
			var doc = Markdig.Markdown.Parse(source, Pipeline);
			foreach (var block in doc)
			{
				if (block is FencedCodeBlock fenced)
				{
					var slices = new System.Collections.Generic.List<string>();
					foreach (var slice in fenced.Lines)
					{
						slices.Add(System.Text.Json.JsonSerializer.Serialize(slice.ToString()));
					}

					_output.WriteLine($"{System.Text.Json.JsonSerializer.Serialize(source)} Count={fenced.Lines.Count} ToString={System.Text.Json.JsonSerializer.Serialize(fenced.Lines.ToString())} Slices=[{string.Join(", ", slices)}]");
				}
			}
		}
	}
}
