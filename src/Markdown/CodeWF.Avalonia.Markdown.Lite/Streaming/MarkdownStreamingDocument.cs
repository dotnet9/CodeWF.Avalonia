using System.Diagnostics;
using System.Text;

using CodeWF.Avalonia.Markdown.Shared.Rendering;

using Markdig;

namespace CodeWF.Avalonia.Markdown.Streaming;

/// <summary>
/// 流式快照：某一时刻源码缓冲的完整块模型。
///
/// <see cref="FrozenBlocks"/> 中的块实例跨快照共享（冻结后只增不改），
/// <see cref="Model"/> 是冻结块 + 尾部块组装出的完整文档模型，可直接交给
/// 渲染层的块级 diff 复用。
/// </summary>
public sealed record MarkdownStreamingSnapshot(
	int Generation,
	long Version,
	string Source,
	int FrozenOffset,
	bool IsComplete,
	IReadOnlyList<MarkdownDocumentBlock> FrozenBlocks,
	MarkdownDocumentModel Model,
	TimeSpan ParseDuration)
{
	public IReadOnlyList<MarkdownDocumentBlock> Blocks => Model.Blocks;

	public static MarkdownStreamingSnapshot Empty { get; } =
		new(0, 0, string.Empty, 0, false, [], MarkdownDocumentModel.Empty, TimeSpan.Zero);
}

/// <summary>
/// 流式文档：以只追加的源码缓冲为唯一真相，把连续到达的文本切片
/// 增量变成块模型（架构见 docs/流式规则.md §2）。
///
/// <see cref="Append"/> 可在任意线程调用；解析在单个后台工作者上串行执行，
/// 进行中再来的文本只置脏标记、中间版本直接跳过；解析异常吞掉并保留
/// 上一份快照（界面不崩、停更），异常经 <see cref="Failed"/> 上抛。
/// </summary>
public sealed class MarkdownStreamingDocument : IDisposable
{
	private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
		.UseAdvancedExtensions()
		.Build();

	private readonly object _gate = new();
	private readonly MarkdownStreamSegmenter _segmenter = new();

	private string _source = string.Empty;
	private long _version;
	private int _generation;
	private bool _dirty;
	private bool _running;
	private bool _completed;
	private bool _disposed;

	// 冻结区状态：只增不改，跨快照共享。
	private int _frozenEnd;
	private readonly List<MarkdownDocumentBlock> _frozenBlocks = [];
	private readonly StringBuilder _frozenPlain = new();
	private MarkdownDependencyFlags _frozenFlags = MarkdownDependencyFlags.None;

	private MarkdownStreamingSnapshot _latest = MarkdownStreamingSnapshot.Empty;

	private readonly SynchronizationContext? _synchronizationContext;

	public MarkdownStreamingDocument()
	{
		_synchronizationContext = SynchronizationContext.Current;
	}

	/// <summary>快照就绪（在创建文档的同步上下文上回调）。</summary>
	public event EventHandler<MarkdownStreamingSnapshot>? SnapshotReady;

	/// <summary>解析失败；保留上一份快照，下一次文本到达会重新尝试。</summary>
	public event EventHandler<Exception>? Failed;

	/// <summary>流结束（<see cref="Complete"/> 之后的最终整篇解析已发布）。</summary>
	public event EventHandler? Completed;

	/// <summary>最新快照（尚未产出时为 <see cref="MarkdownStreamingSnapshot.Empty"/>）。</summary>
	public MarkdownStreamingSnapshot Latest => _latest;

	/// <summary>追加文本切片（任意线程）。</summary>
	public void Append(string text)
	{
		ArgumentNullException.ThrowIfNull(text);
		lock (_gate)
		{
			ObjectDisposedException.ThrowIf(_disposed, this);
			if (_completed)
			{
				throw new InvalidOperationException("流已完成，请先 Reset 再追加新内容。");
			}

			if (text.Length == 0)
			{
				return;
			}

			_source = _source.Length == 0 ? text : string.Concat(_source, text);
			_version++;
			_dirty = true;
		}

		EnsureWorker();
	}

	/// <summary>
	/// 结束流：整篇重解析一次并开新代，收敛跨片段的全局依赖
	/// （链接引用定义、脚注编号）。之后再调用 <see cref="Append"/> 会抛出异常。
	/// </summary>
	public void Complete()
	{
		lock (_gate)
		{
			ObjectDisposedException.ThrowIf(_disposed, this);
			_completed = true;
			_dirty = true;
		}

		EnsureWorker();
	}

	/// <summary>
	/// 重置为整串文本并开新代（语义等同现有 <c>Markdown</c> 属性的整串赋值）。
	/// </summary>
	public void Reset(string? text = null)
	{
		lock (_gate)
		{
			ObjectDisposedException.ThrowIf(_disposed, this);
			_source = text ?? string.Empty;
			_generation++;
			_version++;
			_completed = false;
			_dirty = true;
			_frozenEnd = 0;
			_frozenBlocks.Clear();
			_frozenPlain.Clear();
			_frozenFlags = MarkdownDependencyFlags.None;
			_segmenter.Reset();
		}

		EnsureWorker();
	}

	public void Dispose()
	{
		lock (_gate)
		{
			_disposed = true;
		}
	}

	private void EnsureWorker()
	{
		lock (_gate)
		{
			if (_running || _disposed)
			{
				return;
			}

			_running = true;
		}

		_ = Task.Run(ProcessLoop);
	}

	private void ProcessLoop()
	{
		while (true)
		{
			string source;
			bool complete;
			int generation;
			long version;
			lock (_gate)
			{
				if (!_dirty || _disposed)
				{
					_running = false;
					return;
				}

				_dirty = false;
				source = _source;
				complete = _completed;
				generation = _generation;
				version = _version;
			}

			MarkdownStreamingSnapshot snapshot;
			try
			{
				snapshot = BuildSnapshot(source, complete, generation, version);
			}
			catch (Exception exception)
			{
				// 解析失败保留上一份快照（规格 B2），异常上抛给宿主。
				Post(() => Failed?.Invoke(this, exception));
				continue;
			}

			lock (_gate)
			{
				_latest = snapshot;
			}

			Post(() => SnapshotReady?.Invoke(this, snapshot));
			if (complete)
			{
				// Complete 之后 Append 会抛出，本事件每次 Complete 周期只发一次。
				Post(() => Completed?.Invoke(this, EventArgs.Empty));
			}
		}
	}

	private MarkdownStreamingSnapshot BuildSnapshot(string source, bool complete, int generation, long version)
	{
		var start = Stopwatch.GetTimestamp();

		if (complete)
		{
			// 终局：整篇重解析（规格 B3），跨片段的全局依赖就此收敛。
			var finalModel = Parse(source);
			var duration = Stopwatch.GetElapsedTime(start);
			return new MarkdownStreamingSnapshot(
				generation, version, source, source.Length, true, finalModel.Blocks, finalModel, duration);
		}

		_segmenter.Scan(source, sourceComplete: false);
		var boundary = _segmenter.FrozenBoundary;

		// 冻结新定型的前缀：一次解析、块偏移映射回原始源码，实例从此跨快照共享。
		if (boundary > _frozenEnd)
		{
			var segmentText = source[_frozenEnd..boundary];
			var segmentModel = Parse(segmentText);
			foreach (var block in segmentModel.Blocks)
			{
				var plainStart = _frozenPlain.Length;
				if (block.PlainText.Length > 0)
				{
					_frozenPlain.Append(block.PlainText);
				}

				var plainEnd = _frozenPlain.Length;
				if (_frozenPlain.Length > 0)
				{
					_frozenPlain.AppendLine();
					_frozenPlain.AppendLine();
				}

				_frozenBlocks.Add(block with
				{
					SourceSpan = new MarkdownTextSpan(block.SourceSpan.Start + _frozenEnd, block.SourceSpan.End + _frozenEnd),
					PlainTextSpan = new MarkdownTextSpan(plainStart, plainEnd),
				});
			}

			_frozenFlags |= segmentModel.DependencyFlags;
			_frozenEnd = boundary;
		}

		// 尾部：容错补全后解析，块偏移经映射回到原始源码坐标。
		var tailText = source[boundary..];
		var remend = MarkdownTailRemender.Apply(tailText);
		var tailModel = Parse(remend.RemendedText);

		var tailPlain = new StringBuilder();
		var tailBlocks = new List<MarkdownDocumentBlock>(tailModel.Blocks.Count);
		var tailFlags = MarkdownDependencyFlags.None;
		foreach (var block in tailModel.Blocks)
		{
			var sourceStart = boundary + remend.OffsetMap.MapTempToOriginal(block.SourceSpan.Start);
			var sourceEnd = boundary + remend.OffsetMap.MapTempToOriginal(block.SourceSpan.End);
			var plainStart = _frozenPlain.Length + tailPlain.Length;
			if (block.PlainText.Length > 0)
			{
				tailPlain.Append(block.PlainText);
			}

			var plainEnd = _frozenPlain.Length + tailPlain.Length;
			if (tailPlain.Length > 0)
			{
				tailPlain.AppendLine();
				tailPlain.AppendLine();
			}

			tailFlags |= block.DependencyFlags;
			tailBlocks.Add(block with
			{
				SourceSpan = new MarkdownTextSpan(
					Math.Min(sourceStart, source.Length),
					Math.Clamp(sourceEnd, Math.Min(sourceStart, source.Length), source.Length)),
				PlainTextSpan = new MarkdownTextSpan(plainStart, plainEnd),
			});
		}

		var blocks = new List<MarkdownDocumentBlock>(_frozenBlocks.Count + tailBlocks.Count);
		blocks.AddRange(_frozenBlocks);
		blocks.AddRange(tailBlocks);
		var plainText = tailPlain.Length == 0
			? _frozenPlain.ToString()
			: _frozenPlain.ToString() + tailPlain.ToString();
		var model = new MarkdownDocumentModel(source, blocks, plainText.TrimEnd(), _frozenFlags | tailFlags);

		return new MarkdownStreamingSnapshot(
			generation, version, source, boundary, false, _frozenBlocks, model, Stopwatch.GetElapsedTime(start));
	}

	private static MarkdownDocumentModel Parse(string text) => MarkdownParser.Parse(text, Pipeline);

	private void Post(Action action)
	{
		if (_synchronizationContext is { } context)
		{
			// 回调抛异常不能打断后台工作者。
			context.Post(static state =>
			{
				try
				{
					((Action)state!).Invoke();
				}
				catch (Exception exception)
				{
					System.Diagnostics.Debug.WriteLine($"Markdown streaming callback failed: {exception}");
				}
			}, action);
			return;
		}

		try
		{
			action();
		}
		catch (Exception exception)
		{
			System.Diagnostics.Debug.WriteLine($"Markdown streaming callback failed: {exception}");
		}
	}
}
