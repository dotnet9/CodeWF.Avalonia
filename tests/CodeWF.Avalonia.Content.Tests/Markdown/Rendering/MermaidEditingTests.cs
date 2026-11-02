using CodeWF.Avalonia.Markdown.Mermaid;
using Markdig;
using Markdig.Syntax;
using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Rendering;

public sealed class MermaidEditingTests
{
    [Theory]
    [InlineData("```mermaid", "```", "\n")]
    [InlineData("~~~~MERMAID", "~~~~", "\r\n")]
    [InlineData("````mermaid", "````", "\n")]
    public void ReplacesOnlyTheTargetBodyAndPreservesFenceAndLineEndings(string opening, string closing, string newline)
    {
        var markdown = string.Join(newline, "# Before", "", "```mermaid", "flowchart LR", "FIRST --> ONE", "```", "",
            opening, "flowchart LR", "SECOND --> TWO", closing, "", "After.");
        var blocks = global::Markdig.Markdown.Parse(markdown).OfType<FencedCodeBlock>().ToArray();
        var state = new MermaidDiagramViewModel(markdown, blocks[1]);
        Assert.Null(state.CreateEdit());
        state.Source = "flowchart LR\nSECOND --> THREE\nTHREE --> FOUR";
        var edit = Assert.IsType<MermaidEditEventArgs>(state.CreateEdit());
        Assert.Equal(markdown, edit.OriginalMarkdown);
        Assert.Equal(markdown.Replace("SECOND --> TWO", "SECOND --> THREE" + newline + "THREE --> FOUR"), edit.Markdown);
        Assert.Equal(markdown.IndexOf("flowchart LR", markdown.IndexOf(opening, markdown.IndexOf("FIRST --> ONE", StringComparison.Ordinal), StringComparison.Ordinal), StringComparison.Ordinal), edit.Start);
    }

    [Theory]
    [InlineData("> ```mermaid\n> flowchart LR\n> A --> B\n> ```\n\nEnd", "> A --> C\n> C --> D")]
    [InlineData("- Item\n\n  ```mermaid\n  flowchart LR\n  A --> B\n  ```\n\nEnd", "  A --> C\n  C --> D")]
    public void KeepsQuoteAndListContinuationPrefixes(string markdown, string expected)
    {
        var block = global::Markdig.Markdown.Parse(markdown).Descendants<FencedCodeBlock>().Single();
        var state = new MermaidDiagramViewModel(markdown, block) { Source = "flowchart LR\nA --> C\nC --> D" };
        var edit = state.CreateEdit()!;
        Assert.Contains(expected, edit.Markdown);
        Assert.EndsWith("\n\nEnd", edit.Markdown);
        Assert.Single(global::Markdig.Markdown.Parse(edit.Markdown).Descendants<FencedCodeBlock>());
    }

    [Fact]
    public void ChunkRelativeSpansAreTranslatedIntoTheFullDocument()
    {
        const string prefix = "# Introduction\n\nParagraph before the chunk.\n\n";
        const string chunk = "```mermaid\nflowchart LR\nA --> B\n```\n\nAfter.";
        var block = global::Markdig.Markdown.Parse(chunk).OfType<FencedCodeBlock>().Single();
        var state = new MermaidDiagramViewModel(prefix + chunk, block, prefix.Length) { Source = "flowchart LR\nA --> C" };
        Assert.Equal(prefix + chunk.Replace("A --> B", "A --> C"), state.CreateEdit()!.Markdown);
    }
}
