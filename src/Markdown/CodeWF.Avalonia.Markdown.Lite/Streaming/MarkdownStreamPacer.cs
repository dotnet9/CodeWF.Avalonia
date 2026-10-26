using System.Globalization;
using System.Text;

namespace CodeWF.Avalonia.Markdown.Streaming;

/// <summary>
/// 流式输入节奏器：把宿主一次性收到的大分片（网络突发、粘贴）整理成
/// 按帧平滑释放的显示节奏，插在 <see cref="MarkdownStreamingDocument.Append"/> 之前。
///
/// 纯算法、无计时器：时间由调用方注入（通常为渲染帧时间戳），<see cref="Push"/> 与
/// <see cref="Tick"/> 须在同一线程调用（非线程安全）。释放速率取「到达速率滑动平均」
/// 与「积压/目标滞留时长」的较大者，滞留超过上限的分片强制纳入当帧保证尾部不饿死；
/// 释放只在字素簇边界切分，不会拆开代理对与 emoji 组合序列；积压超过直通阈值的
/// 整段立即放行（粘贴场景不等平滑）。
///
/// 典型接线：到达线程 <see cref="Push"/>，渲染帧 <see cref="Tick"/> 并把返回文本
/// 追加进流式文档；到达结束 <see cref="Finish"/>，<see cref="IsDrained"/> 后再
/// <see cref="MarkdownStreamingDocument.Complete"/>。
/// 算法移植自 StreamMark 的 StreamPacer（MIT License）。
/// </summary>
public sealed class MarkdownStreamPacer
{
	/// <summary>平滑档位：目标滞留时长越短跟手性越好，越长越平滑。</summary>
	public enum PacingPreset
	{
		Responsive,
		Balanced,
		Smooth,
	}

	private const double MaxLagMilliseconds = 400;
	private const int BypassThreshold = 4096;
	private const double FinishDrainMilliseconds = 350;

	private readonly double _targetLagMilliseconds;
	private readonly Queue<PacedChunk> _backlog = new();

	// 到达速率（字符/毫秒）指数滑动平均：0.8 旧值 + 0.2 新值。
	private double? _arrivalRate;
	private TimeSpan _lastPushAt;
	private TimeSpan _lastTickAt;
	private double _carry;
	private bool _hasPushed;
	private bool _hasTicked;
	private bool _finished;
	private bool _bypass;
	private int _backlogChars;

	private sealed class PacedChunk
	{
		public string Text = string.Empty;
		public TimeSpan ArrivedAt;
	}

	/// <summary>按档位创建节奏器。</summary>
	public MarkdownStreamPacer(PacingPreset preset = PacingPreset.Balanced)
		: this(TimeSpan.FromMilliseconds(preset switch
		{
			PacingPreset.Responsive => 40,
			PacingPreset.Smooth => 200,
			_ => 120,
		}))
	{
	}

	/// <summary>按自定义目标滞留时长创建节奏器。</summary>
	public MarkdownStreamPacer(TimeSpan targetLag)
	{
		if (targetLag <= TimeSpan.Zero)
		{
			throw new ArgumentOutOfRangeException(nameof(targetLag));
		}

		_targetLagMilliseconds = targetLag.TotalMilliseconds;
	}

	/// <summary>尚未释放的积压字符数。</summary>
	public int BacklogLength => _backlogChars;

	/// <summary>到达是否已结束（<see cref="Finish"/> 之后）。</summary>
	public bool IsFinished => _finished;

	/// <summary>积压是否已清空（<see cref="Finish"/> 之后为 true 时宿主可结束流）。</summary>
	public bool IsDrained => _backlogChars == 0;

	/// <summary>
	/// 推入一个到达分片（任意大小）；<see cref="Finish"/> 之后到达的文本被忽略。
	/// 积压超过直通阈值时置直通标记，下一帧整段放行。
	/// </summary>
	public void Push(string text, TimeSpan now)
	{
		ArgumentNullException.ThrowIfNull(text);
		if (text.Length == 0 || _finished)
		{
			return;
		}

		if (_hasPushed)
		{
			var delta = (now - _lastPushAt).TotalMilliseconds;
			if (delta > 0)
			{
				var rate = text.Length / delta;
				_arrivalRate = _arrivalRate is { } current ? current * 0.8 + rate * 0.2 : rate;
			}
		}
		else
		{
			// 首片无法估计到达速率，释放速率暂由积压/目标滞留兜底。
			_hasPushed = true;
		}

		_lastPushAt = now;
		_backlog.Enqueue(new PacedChunk { Text = text, ArrivedAt = now });
		_backlogChars += text.Length;
		if (_backlogChars > BypassThreshold)
		{
			_bypass = true;
		}
	}

	/// <summary>
	/// 推进一帧，返回本帧应显示的文本（可能为空串）。帧间隔由两次调用的
	/// 时间差决定，掉帧一步补齐，不丢内容。
	/// </summary>
	public string Tick(TimeSpan now)
	{
		var delta = _hasTicked ? (now - _lastTickAt).TotalMilliseconds : 0;
		if (delta < 0)
		{
			delta = 0;
		}

		_lastTickAt = now;
		_hasTicked = true;
		if (_backlogChars == 0)
		{
			_carry = 0;
			_bypass = false;
			return string.Empty;
		}

		var rate = _arrivalRate ?? 0;
		var lagRate = _backlogChars / _targetLagMilliseconds;
		if (lagRate > rate)
		{
			rate = lagRate;
		}

		if (_finished)
		{
			// 结束后的剩余积压在约 350ms 内排空。
			var drainRate = _backlogChars / FinishDrainMilliseconds;
			if (drainRate > rate)
			{
				rate = drainRate;
			}
		}

		_carry += rate * delta;
		if (_bypass)
		{
			_carry = _backlogChars;
		}

		// 滞留超过上限的整段强制纳入本帧，保证尾部不饿死。
		var lagCutoff = now - TimeSpan.FromMilliseconds(MaxLagMilliseconds);
		var forced = 0;
		foreach (var chunk in _backlog)
		{
			if (chunk.ArrivedAt >= lagCutoff)
			{
				break;
			}

			forced += chunk.Text.Length;
		}

		if (forced > _carry)
		{
			_carry = forced;
		}

		var released = Release(Math.Min(_carry, _backlogChars));
		_carry = Math.Max(0, _carry - released.Length);
		if (_backlogChars == 0)
		{
			_carry = 0;
			_bypass = false;
		}

		return released;
	}

	/// <summary>到达结束：不再接受新文本，剩余积压按排空速率继续释放。</summary>
	public void Finish() => _finished = true;

	/// <summary>清空内容状态（复用实例开始新流时调用）；节拍时钟保留，时间连续性不受影响。</summary>
	public void Reset()
	{
		_backlog.Clear();
		_backlogChars = 0;
		_arrivalRate = null;
		_carry = 0;
		_hasPushed = false;
		_finished = false;
		_bypass = false;
	}

	/// <summary>按字素簇边界释放约 <paramref name="targetChars"/> 个字符。</summary>
	private string Release(double targetChars)
	{
		var quota = (int)targetChars;
		if (quota <= 0)
		{
			return string.Empty;
		}

		var builder = new StringBuilder();
		var taken = 0;
		while (taken < quota && _backlog.Count > 0)
		{
			var chunk = _backlog.Peek();
			var take = GraphemeLength(chunk.Text, quota - taken);
			builder.Append(chunk.Text, 0, take);
			taken += take;
			_backlogChars -= take;
			if (take >= chunk.Text.Length)
			{
				_backlog.Dequeue();
			}
			else
			{
				chunk.Text = chunk.Text[take..];
			}
		}

		return builder.ToString();
	}

	/// <summary>取不超过 <paramref name="maxChars"/> 个字符的字素簇前缀长度（至少一个完整簇）。</summary>
	private static int GraphemeLength(string text, int maxChars)
	{
		var index = 0;
		var clusters = 0;
		while (index < text.Length && clusters < maxChars)
		{
			index += StringInfo.GetNextTextElementLength(text.AsSpan(index));
			clusters++;
		}

		return index;
	}
}
