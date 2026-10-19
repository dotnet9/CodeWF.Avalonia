using System.Reflection;
using System.Text.Json;

using CodeWF.Avalonia.Markdown.Streaming;

using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Streaming;

/// <summary>
/// remend 处理器的数据驱动回放：用例以 JSON fixture 维护
/// （Markdown/Streaming/Fixtures/remend/），新增场景只加文件不改测试代码。
/// </summary>
public sealed class RemendFixtureTests
{
	private sealed record RemendFixture(string Name, string Tail, string Remended);

	private const string ResourcePrefix = "CodeWF.Avalonia.Content.Tests.Markdown.Streaming.Fixtures.remend.";

	public static TheoryData<string, string, string> Fixtures
	{
		get
		{
			var data = new TheoryData<string, string, string>();
			foreach (var (name, tail, remended) in LoadFixtures())
			{
				data.Add(name, tail, remended);
			}

			return data;
		}
	}

	[Theory]
	[MemberData(nameof(Fixtures))]
	public void Replay_MatchesExpectedRemendedText(string name, string tail, string remended)
	{
		var result = MarkdownTailRemender.Apply(tail);

		Assert.True(result.RemendedText == remended,
			$"{name}：期望 {Display(remended)}，实际 {Display(result.RemendedText)}");
	}

	[Fact]
	public void FixtureCount_MeetsBaseline()
	{
		// 防呆：fixture 资源没有被嵌入时测试会静默空转。
		var fixtures = LoadFixtures();
		Assert.True(fixtures.Count >= 20,
			"remend fixture 少于基线 20 条，检查 EmbeddedResource 通配；实际 ["
			+ string.Join(", ", Assembly.GetExecutingAssembly().GetManifestResourceNames()) + "]");
		Assert.All(fixtures, fixture =>
		{
			Assert.NotNull(fixture.Name);
			Assert.NotNull(fixture.Tail);
			Assert.NotNull(fixture.Remended);
		});
	}

	private static List<RemendFixture> LoadFixtures()
	{
		var assembly = Assembly.GetExecutingAssembly();
		var fixtures = new List<RemendFixture>();
		foreach (var resourceName in assembly.GetManifestResourceNames())
		{
			if (!resourceName.StartsWith(ResourcePrefix, StringComparison.Ordinal)
				|| !resourceName.EndsWith(".json", StringComparison.Ordinal))
			{
				continue;
			}

			using var stream = assembly.GetManifestResourceStream(resourceName);
			var fixture = JsonSerializer.Deserialize<RemendFixture>(stream!, new JsonSerializerOptions
			{
				// fixture 文件使用 camelCase 键。
				PropertyNameCaseInsensitive = true,
			}) ?? throw new InvalidOperationException($"fixture 反序列化失败：{resourceName}");
			fixtures.Add(fixture);
		}

		return fixtures.OrderBy(fixture => fixture.Name, StringComparer.Ordinal).ToList();
	}

	private static string Display(string text) => $"\"{text.Replace("\n", "\\n")}\"";
}
