using Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Input;
using global::Avalonia.VisualTree;

namespace CodeWF.Avalonia.Markdown.Shared.Rendering;

internal sealed record MarkdownSelectionBlock(
	Control Control,
	int BlockKey,
	MarkdownTextSpan PlainTextSpan,
	string PlainText);

/// <summary>
/// 选区坐标：只存值——块键（源码起始偏移）与块内纯文本偏移。文本始终按块模型
/// 取，不依赖控件实例，所以流式重建控件、虚拟化回收之后选区与复制都不受影响。
/// </summary>
internal readonly record struct MarkdownTextPoint(int BlockKey, int Offset);

internal sealed class MarkdownSelectionController
{
	private const string SelectedBlockClass = "MdSelectedBlock";
	private readonly List<MarkdownSelectionBlock> _blocks = [];
	private Visual? _coordinateRoot;
	private MarkdownTextPoint? _anchor;
	private MarkdownTextPoint? _active;
	private bool _hasVisualSelection;

	public bool HasSelection => _hasVisualSelection
								&& _anchor.HasValue
								&& _active.HasValue
								&& SelectedText.Length > 0;

	public string SelectedText
	{
		get
		{
			if (!_hasVisualSelection || !_anchor.HasValue || !_active.HasValue || _blocks.Count == 0)
			{
				return string.Empty;
			}

			var (start, end) = GetOrderedPoints();
			var startIndex = FindIndex(start.BlockKey);
			var endIndex = FindIndex(end.BlockKey);
			if (startIndex < 0 || endIndex < 0 || endIndex < startIndex)
			{
				return string.Empty;
			}

			var parts = new List<string>();
			for (var i = startIndex; i <= endIndex; i++)
			{
				var text = _blocks[i].PlainText;
				if (text.Length == 0)
				{
					continue;
				}

				var from = i == startIndex ? Math.Clamp(start.Offset, 0, text.Length) : 0;
				var to = i == endIndex ? Math.Clamp(end.Offset, 0, text.Length) : text.Length;
				if (to > from)
				{
					parts.Add(text[from..to]);
				}
			}

			return string.Join(Environment.NewLine + Environment.NewLine, parts);
		}
	}

	/// <summary>
	/// 重建块列表。选区按块键重新定位：键仍在（同一块的控件被替换/回收）就保留，
	/// 键消失（内容真的换了）就清空。控件引用只用于绘制选中底色与命中测试。
	/// </summary>
	public void SetBlocks(
		IEnumerable<MarkdownSelectionBlock> blocks,
		Visual? coordinateRoot = null,
		bool preserveSelection = false)
	{
		ClearVisualState();
		_blocks.Clear();
		_blocks.AddRange(blocks.Where(block => block.PlainTextSpan.Length > 0));
		_coordinateRoot = coordinateRoot;
		if (!preserveSelection)
		{
			_anchor = null;
			_active = null;
			_hasVisualSelection = false;
			return;
		}

		if (_anchor is { } anchor && FindIndex(anchor.BlockKey) < 0
			|| _active is { } active && FindIndex(active.BlockKey) < 0)
		{
			_anchor = null;
			_active = null;
			_hasVisualSelection = false;
			return;
		}

		ApplyVisualState();
	}

	public bool Begin(Point point)
	{
		var index = HitTestBlock(point);
		if (index < 0)
		{
			Clear();
			return false;
		}

		ClearVisualState();
		var block = _blocks[index];
		_anchor = new MarkdownTextPoint(block.BlockKey, 0);
		_active = new MarkdownTextPoint(block.BlockKey, block.PlainText.Length);
		return true;
	}

	public bool Extend(Point point)
	{
		if (!_anchor.HasValue)
		{
			return false;
		}

		var index = ResolveExtensionIndex(point);
		if (index < 0)
		{
			return false;
		}

		var block = _blocks[index];
		var movingDown = FindIndex(_anchor.Value.BlockKey) <= index;
		var next = new MarkdownTextPoint(block.BlockKey, movingDown ? block.PlainText.Length : 0);
		if (_active == next)
		{
			return false;
		}

		_active = next;
		ApplyVisualState();
		return true;
	}

	public void CommitClickWithoutDrag()
	{
		Clear();
	}

	public void Clear()
	{
		ClearVisualState();
		_anchor = null;
		_active = null;
		_hasVisualSelection = false;
	}

	private int FindIndex(int blockKey)
	{
		for (var i = 0; i < _blocks.Count; i++)
		{
			if (_blocks[i].BlockKey == blockKey)
			{
				return i;
			}
		}

		return -1;
	}

	private int HitTestBlock(Point documentPoint)
	{
		for (var i = 0; i < _blocks.Count; i++)
		{
			var bounds = GetDocumentBounds(_blocks[i].Control);
			if (documentPoint.Y >= bounds.Top && documentPoint.Y <= bounds.Bottom)
			{
				return i;
			}
		}

		return -1;
	}

	private Rect GetDocumentBounds(Control control)
	{
		if (_coordinateRoot is not null
			&& control.TranslatePoint(new Point(0, 0), _coordinateRoot) is { } origin)
		{
			return new Rect(origin, control.Bounds.Size);
		}

		return control.Bounds;
	}

	private int ResolveExtensionIndex(Point documentPoint)
	{
		return HitTestBlock(documentPoint);
	}

	private (MarkdownTextPoint Start, MarkdownTextPoint End) GetOrderedPoints()
	{
		var anchor = _anchor.GetValueOrDefault();
		var active = _active.GetValueOrDefault();
		var anchorIndex = FindIndex(anchor.BlockKey);
		var activeIndex = FindIndex(active.BlockKey);
		if (anchorIndex > activeIndex
			|| anchorIndex == activeIndex && anchor.Offset > active.Offset)
		{
			return (active, anchor);
		}

		return (anchor, active);
	}

	private void ApplyVisualState()
	{
		ClearVisualState();
		if (!_anchor.HasValue || !_active.HasValue)
		{
			return;
		}

		var start = FindIndex(_anchor.Value.BlockKey);
		var end = FindIndex(_active.Value.BlockKey);
		if (start < 0 || end < 0)
		{
			return;
		}

		if (start > end)
		{
			(start, end) = (end, start);
		}

		for (var i = start; i <= end; i++)
		{
			_blocks[i].Control.Classes.Add(SelectedBlockClass);
		}

		_hasVisualSelection = true;
	}

	private void ClearVisualState()
	{
		foreach (var block in _blocks)
		{
			block.Control.Classes.Remove(SelectedBlockClass);
		}

		_hasVisualSelection = false;
	}
}

internal readonly record struct MarkdownPointerSelectionState(Point StartPoint)
{
	public bool IsDragging(Point currentPoint)
	{
		return Math.Abs(currentPoint.X - StartPoint.X) > 4
			   || Math.Abs(currentPoint.Y - StartPoint.Y) > 4;
	}
}
