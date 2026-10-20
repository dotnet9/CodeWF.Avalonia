using CodeWF.Avalonia.Markdown;
using System.IO.Compression;
using System.Text;
using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Export;

public sealed class MarkdownDocumentExporterTests
{
	[Theory]
	[InlineData("A4", 595, 842)]
	[InlineData("Letter", 612, 792)]
	[InlineData("A5", 420, 595)]
	public void ExportPdf_UsesSelectedPaperAndWatermark(string paper, int width, int height)
	{
		var path = Path.Combine(Path.GetTempPath(), $"codewf-pdf-options-{Guid.NewGuid():N}.pdf");
		try
		{
			MarkdownDocumentExporter.ExportPdf(new MarkdownExportDocument("# Page options\n\nSelectable body text."), path,
				options: new MarkdownPdfExportOptions(PageSize: paper, MarginMillimeters: 20, WatermarkText: "Private 维刻", WatermarkOpacity: 0.12));
			var pdf = Encoding.Latin1.GetString(File.ReadAllBytes(path));
			Assert.Matches($@"/MediaBox\s*\[0\s+0\s+{width}\s+{height}\]", pdf);
			Assert.Contains("/ExtGState", pdf);
			Assert.Contains("/ToUnicode", pdf);
			Assert.DoesNotContain("/Subtype /Image", pdf);
		}
		finally { File.Delete(path); }
	}

	[Fact]
	public void ExportPdf_LargerMarginRepaginatesContent()
	{
		var first = Path.Combine(Path.GetTempPath(), $"codewf-pdf-margin-{Guid.NewGuid():N}.pdf");
		var second = first + ".large.pdf";
		try
		{
			var document = new MarkdownExportDocument(string.Join("\n\n", Enumerable.Repeat("A paragraph that wraps and stays inside the selected page content bounds.", 100)));
			MarkdownDocumentExporter.ExportPdf(document, first, options: new MarkdownPdfExportOptions(MarginMillimeters: 10));
			MarkdownDocumentExporter.ExportPdf(document, second, options: new MarkdownPdfExportOptions(MarginMillimeters: 30));
			int Pages(string path) => System.Text.RegularExpressions.Regex.Matches(Encoding.Latin1.GetString(File.ReadAllBytes(path)), @"/Type /Page\b").Count;
			Assert.True(Pages(second) > Pages(first));
		}
		finally { File.Delete(first); File.Delete(second); }
	}

	[Fact]
	public void ExportPdf_WhenMarkdownContainsText_DoesNotEmbedWholePageImage()
	{
		var path = Path.Combine(Path.GetTempPath(), $"codewf-markdown-export-{Guid.NewGuid():N}.pdf");
		try
		{
			MarkdownDocumentExporter.ExportMarkdown(
				"""
				# Selectable PDF

				This text should stay selectable in the exported PDF.

				- Copy this list item
				- 复制这一行中文内容
				""",
				ExportKind.Pdf,
				MarkdownExportStyle.Resolve(null, null),
				path);

			var pdf = Encoding.Latin1.GetString(File.ReadAllBytes(path));
			Assert.StartsWith("%PDF-", pdf);
			Assert.Contains("/Font", pdf);
			Assert.Contains("/ToUnicode", pdf);
			Assert.DoesNotContain("/Subtype /Image", pdf);
		}
		finally
		{
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}
	}

	[Fact]
	public void ExportWord_WhenMarkdownContainsDataImage_EmbedsImagePart()
	{
		var path = Path.Combine(Path.GetTempPath(), $"codewf-markdown-export-{Guid.NewGuid():N}.docx");
		try
		{
			var document = new MarkdownExportDocument(
				"""
				# Export test

				![pixel](data:image/gif;base64,R0lGODlhAQABAPAAAP///wAAACH5BAAAAAAALAAAAAABAAEAAAICRAEAOw==)
				""",
				fileName: "export-test.md");

			MarkdownDocumentExporter.ExportWord(document, path);

			using var archive = ZipFile.OpenRead(path);
			Assert.NotNull(archive.GetEntry("word/document.xml"));
			Assert.NotNull(archive.GetEntry("word/_rels/document.xml.rels"));
			Assert.Contains(archive.Entries, entry => entry.FullName.StartsWith("word/media/image", StringComparison.Ordinal));
		}
		finally
		{
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}
	}

	[Fact]
	public void ExportMarkdown_WhenKindIsWord_WritesDocx()
	{
		var path = Path.Combine(Path.GetTempPath(), $"codewf-markdown-export-{Guid.NewGuid():N}.docx");
		try
		{
			MarkdownDocumentExporter.ExportMarkdown(
				"# Export facade",
				ExportKind.Word,
				MarkdownExportStyle.Resolve(null, null),
				path);

			using var archive = ZipFile.OpenRead(path);
			Assert.NotNull(archive.GetEntry("word/document.xml"));
		}
		finally
		{
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}
	}

	[Fact]
	public void ExportFile_WhenKindIsWord_ReadsMarkdownFromDisk()
	{
		var markdownPath = Path.Combine(Path.GetTempPath(), $"codewf-markdown-export-{Guid.NewGuid():N}.md");
		var docxPath = Path.Combine(Path.GetTempPath(), $"codewf-markdown-export-{Guid.NewGuid():N}.docx");
		try
		{
			File.WriteAllText(markdownPath, "# Export file");

			MarkdownDocumentExporter.ExportFile(
				markdownPath,
				ExportKind.Word,
				MarkdownExportStyle.Resolve(null, null),
				docxPath);

			using var archive = ZipFile.OpenRead(docxPath);
			Assert.NotNull(archive.GetEntry("word/document.xml"));
		}
		finally
		{
			if (File.Exists(markdownPath))
			{
				File.Delete(markdownPath);
			}

			if (File.Exists(docxPath))
			{
				File.Delete(docxPath);
			}
		}
	}
}
