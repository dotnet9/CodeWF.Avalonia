using Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Input;
using global::Avalonia.VisualTree;

namespace CodeWF.Avalonia.Markdown.Shared.Rendering;

internal sealed record MarkdownSelectionBlock(
	Control Control,
	MarkdownTextSpan PlainTextSpan,
	string PlainText);

internal sealed class MarkdownSelectionController
{
	private const string SelectedBlockClass = "MdSelectedBlock";
	private readonly List<MarkdownSelectionBlock> _blocks = [];
	private Visual? _coordinateRoot;
	private int? _anchorIndex;
	private int? _activeIndex;
	private bool _hasVisualSelection;

	public bool HasSelection => _hasVisualSelection
								&& _anchorIndex.HasValue
								&& _activeIndex.HasValue
								&& SelectedText.Length > 0;

	public string SelectedText
	{
		get
		{
			if (!_hasVisualSelection || !_anchorIndex.HasValue || !_activeIndex.HasValue || _blocks.Count == 0)
			{
				return string.Empty;
			}

			var (start, end) = GetOrderedIndexes();
			var parts = _blocks
				.Skip(start)
				.Take(end - start + 1)
				.Select(block => block.PlainText)
				.Where(text => !string.IsNullOrEmpty(text));
			return string.Join(Environment.NewLine + Environment.NewLine, parts);
		}
	}

	public void SetBlocks(IEnumerable<MarkdownSelectionBlock> blocks, Visual? coordinateRoot = null, bool preserveSelection = false)
	{
		// 控件实例未重建（如流式尾块原地更新）时按控件引用恢复块级选区；
		// 控件被替换时引用对不上，恢复自然落空。
		var selectedControls = preserveSelection ? CaptureSelectedControls() : null;
		ClearVisualState();
		_blocks.Clear();
		_blocks.AddRange(blocks.Where(block => block.PlainTextSpan.Length > 0));
		_coordinateRoot = coordinateRoot;
		_anchorIndex = null;
		_activeIndex = null;
		_hasVisualSelection = false;
		RestoreSelection(selectedControls);
	}

	private List<Control>? CaptureSelectedControls()
	{
		if (!_hasVisualSelection || !_anchorIndex.HasValue || !_activeIndex.HasValue)
		{
			return null;
		}

		var (start, end) = GetOrderedIndexes();
		var controls = new List<Control>();
		for (var i = start; i <= end && i < _blocks.Count; i++)
		{
			controls.Add(_blocks[i].Control);
		}

		return controls;
	}

	private void RestoreSelection(List<Control>? selectedControls)
	{
		if (selectedControls is not { Count: > 0 })
		{
			return;
		}

		var indexes = new List<int>(selectedControls.Count);
		foreach (var control in selectedControls)
		{
			var index = _blocks.FindIndex(block => ReferenceEquals(block.Control, control));
			if (index < 0)
			{
				return;
			}

			indexes.Add(index);
		}

		indexes.Sort();
		_anchorIndex = indexes[0];
		_activeIndex = indexes[^1];
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
		_anchorIndex = index;
		_activeIndex = index;
		return true;
	}

	public bool Extend(Point point)
	{
		if (!_anchorIndex.HasValue)
		{
			return false;
		}

		var index = ResolveExtensionIndex(point);
		if (index < 0)
		{
			return false;
		}

		if (_activeIndex == index)
		{
			return false;
		}

		_activeIndex = index;
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
		_anchorIndex = null;
		_activeIndex = null;
		_hasVisualSelection = false;
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

	private (int Start, int End) GetOrderedIndexes()
	{
		var anchor = _anchorIndex.GetValueOrDefault();
		var active = _activeIndex.GetValueOrDefault();
		return anchor <= active ? (anchor, active) : (active, anchor);
	}

	private void ApplyVisualState()
	{
		ClearVisualState();
		if (!_anchorIndex.HasValue || !_activeIndex.HasValue)
		{
			return;
		}

		var (start, end) = GetOrderedIndexes();
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
