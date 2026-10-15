using PropertyGridLanguage = CodeWF.Avalonia.TreeDataGrid.PropertyGrid.I18n.ResourceKeys;
using System.Globalization;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using VAlignment = Avalonia.Layout.VerticalAlignment;
using HAlignment = Avalonia.Layout.HorizontalAlignment;
using LOrientation = Avalonia.Layout.Orientation;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Utilities;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

/// <summary>
/// 按属性类型创建值编辑器，并统一提交、回退与校验行为。
/// 文本在失焦或按回车时提交，按 Esc 回退；百分比拖动时预览，松开后提交。
/// 带子属性的行显示汇总值。
/// </summary>
public class PropertyCellEditor : ContentControl, IWeakEventSubscriber<PropertyChangedEventArgs>
{
    private bool _rebuilding;
    private bool _synchronizing;
    private PropertyItem? _observedItem;
    private Action? _updateValue;

    /// <summary>使值编辑器填满所属单元格，行高变化时同步拉伸。</summary>
    public PropertyCellEditor()
    {
        HorizontalContentAlignment = HAlignment.Stretch;
        VerticalContentAlignment = VAlignment.Stretch;
    }

    /// <inheritdoc />
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        Rebuild();
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ObserveItem();
        SynchronizeValue();
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        StopObserving();
        base.OnDetachedFromVisualTree(e);
    }

    void IWeakEventSubscriber<PropertyChangedEventArgs>.OnEvent(object? sender, WeakEvent ev, PropertyChangedEventArgs e)
    {
        if (ReferenceEquals(sender, _observedItem)
            && (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName is nameof(PropertyItem.Value) or nameof(PropertyItem.Summary)))
        {
            SynchronizeValue();
        }
    }

    private void ObserveItem()
    {
        if (ReferenceEquals(_observedItem, Item)) return;
        StopObserving();
        _observedItem = Item;
        if (_observedItem is not null)
            WeakEvents.ThreadSafePropertyChanged.Subscribe(_observedItem, this);
    }

    private void StopObserving()
    {
        if (_observedItem is not null)
            WeakEvents.ThreadSafePropertyChanged.Unsubscribe(_observedItem, this);
        _observedItem = null;
    }

    private void SynchronizeValue()
    {
        if (_synchronizing) return;
        _synchronizing = true;
        try { _updateValue?.Invoke(); }
        finally { _synchronizing = false; }
    }

    private PropertyLeafRow? Row => DataContext as PropertyLeafRow;

    private PropertyItem? Item => Row?.Item;

    private PropertyGrid? Owner => this.FindAncestorOfType<PropertyGrid>();

    private void Rebuild()
    {
        if (_rebuilding)
        {
            return;
        }

        _rebuilding = true;
        try
        {
            StopObserving();
            _updateValue = null;
            var item = Item;
            Content = item is null ? null : BuildEditor(item);
            ObserveItem();
            SynchronizeValue();
        }
        finally
        {
            _rebuilding = false;
        }
    }

    private Control? BuildEditor(PropertyItem item)
    {
        if (Owner?.EditorFactory?.Invoke(item) is { } custom)
        {
            return custom;
        }

        if (item.HasChildren)
        {
            return BuildSummary(item);
        }

        if (item.IsReadOnly)
        {
            return BuildReadOnly(item);
        }

        return item.Kind switch
        {
            PropertyItemKind.Category => BuildReadOnly(item),
            PropertyItemKind.Text => BuildTextBox(item, multiline: false),
            PropertyItemKind.MultilineText => BuildTextBox(item, multiline: true),
            PropertyItemKind.Number => BuildNumber(item),
            PropertyItemKind.Boolean => BuildBoolean(item),
            PropertyItemKind.Choice => BuildChoice(item),
            PropertyItemKind.Radio => BuildRadio(item),
            PropertyItemKind.Percent => BuildPercent(item),
            PropertyItemKind.Color => BuildColor(item),
            PropertyItemKind.Path => BuildPath(item),
            PropertyItemKind.Button => BuildButton(item),
            PropertyItemKind.DateTime => BuildDateTime(item),
            _ => BuildReadOnly(item),
        };
    }

    private bool Commit(PropertyItem item, object? newValue)
    {
        // 模型通知只更新显示，不能再次触发业务提交；校验拒绝时立即回退所有类型的编辑器。
        if (_synchronizing) return true;
        var owner = Owner;
        var accepted = owner is null || owner.CommitValue(item, newValue);
        if (!accepted) SynchronizeValue();
        return accepted;
    }

    private TextBlock BuildSummary(PropertyItem item)
    {
        var text = new TextBlock
        {
            Classes = { "pg-summary" },
            VerticalAlignment = VAlignment.Center,
        };

        _updateValue = () => text.Text = item.Summary ?? item.Value as string ?? string.Empty;
        return text;
    }

    private TextBlock BuildReadOnly(PropertyItem item)
    {
        var text = new TextBlock
        {
            Classes = { "pg-readonly" },
            VerticalAlignment = VAlignment.Center,
            Text = item.Summary ?? item.Value?.ToString() ?? string.Empty,
        };
        _updateValue = () =>
        {
            text.Text = item.Summary ?? item.Value?.ToString() ?? string.Empty;
            ToolTip.SetTip(text, text.Text);
        };
        return text;
    }

    private Control BuildTextBox(PropertyItem item, bool multiline)
    {
        var box = new TextBox
        {
            Classes = { "pg-input" },
            Text = item.Value as string ?? string.Empty,
            PlaceholderText = item.Watermark,
            IsReadOnly = false,
            AcceptsReturn = multiline,
            TextWrapping = multiline ? TextWrapping.Wrap : TextWrapping.NoWrap,
            MinHeight = multiline ? 48 : 0,
            HorizontalContentAlignment = multiline ? HAlignment.Stretch : HAlignment.Left,
            VerticalContentAlignment = VAlignment.Center,
        };

        var committed = box.Text;
        var committing = false;
        _updateValue = () =>
        {
            committed = item.Value as string ?? string.Empty;
            box.Text = committed;
        };

        void CommitNow()
        {
            if (committing || box.Text == committed)
            {
                return;
            }

            committing = true;
            try
            {
                if (Commit(item, box.Text))
                {
                    committed = box.Text ?? string.Empty;
                }
                else
                {
                    box.Text = committed; // 预览被取消时回退显示值
                }
            }
            finally
            {
                committing = false;
            }
        }

        void Revert()
        {
            if (box.Text != committed)
            {
                box.Text = committed;
            }
        }

        box.LostFocus += (_, _) =>
        {
            if (item.CommitMode == PropertyCommitMode.FocusLoss)
            {
                CommitNow();
            }
        };
        box.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
            {
                Revert();
            }
            else if (!multiline && e.Key == Key.Enter)
            {
                CommitNow();
            }
        };

        if (!multiline && item.CommitMode == PropertyCommitMode.Immediate)
        {
            box.TextChanged += (_, _) => CommitNow();
        }

        return box;
    }

    private Control BuildNumber(PropertyItem item)
    {
        // FormatString 不可为空，Avalonia 的数值转换会直接使用该格式。
        var numeric = new NumericUpDown
        {
            Classes = { "pg-input" },
            Minimum = item.Minimum.HasValue ? (decimal)item.Minimum.Value : decimal.MinValue,
            Maximum = item.Maximum.HasValue ? (decimal)item.Maximum.Value : decimal.MaxValue,
            Increment = (decimal)(item.Increment <= 0 ? 1 : item.Increment),
            FormatString = string.IsNullOrWhiteSpace(item.Format) ? "0.###" : item.Format,
            PlaceholderText = item.Watermark,
            Value = ToDecimal(item.Value),
        };

        numeric.ValueChanged += (_, e) =>
        {
            if (e.NewValue.HasValue && !Equals(item.Value, e.NewValue.Value))
            {
                Commit(item, e.NewValue.Value);
            }
        };
        _updateValue = () => numeric.Value = ToDecimal(item.Value);

        if (string.IsNullOrEmpty(item.Unit))
        {
            return numeric;
        }

        var panel = new DockPanel { LastChildFill = true };
        var unit = new TextBlock
        {
            Text = item.Unit,
            Classes = { "pg-unit" },
            VerticalAlignment = VAlignment.Center,
            Margin = new Thickness(4, 0, 0, 0),
        };
        DockPanel.SetDock(unit, Dock.Right);
        panel.Children.Add(unit);
        panel.Children.Add(numeric);
        return panel;
    }

    private Control BuildBoolean(PropertyItem item)
    {
        var checkBox = new CheckBox
        {
            IsChecked = item.Value as bool? ?? false,
            Margin = new Thickness(6, 0),
            VerticalContentAlignment = VAlignment.Center,
        };
        checkBox.IsCheckedChanged += (_, _) =>
        {
            if (checkBox.IsChecked.HasValue && !Equals(item.Value, checkBox.IsChecked.Value))
            {
                Commit(item, checkBox.IsChecked.Value);
            }
        };
        _updateValue = () => checkBox.IsChecked = item.Value as bool? ?? false;
        return checkBox;
    }

    private Control BuildChoice(PropertyItem item)
    {
        var combo = new ComboBox
        {
            Classes = { "pg-input" },
            HorizontalContentAlignment = HAlignment.Left,
            VerticalContentAlignment = VAlignment.Center,
            ItemsSource = item.Options,
            PlaceholderText = item.Watermark ?? PropertyGridLocalization.Get(PropertyGridLanguage.SelectPlaceholder),
        };
        _updateValue = () => combo.SelectedItem = item.Value;
        combo.SelectionChanged += (_, _) =>
        {
            if (combo.SelectedItem is string value && !Equals(item.Value, value))
            {
                Commit(item, value);
            }
        };
        return combo;
    }

    private Control BuildRadio(PropertyItem item)
    {
        var panel = new StackPanel
        {
            Orientation = LOrientation.Horizontal,
            Margin = new Thickness(6, 0),
            VerticalAlignment = VAlignment.Center,
        };
        var groupName = "pg_radio_" + (item.Id ?? item.GetHashCode().ToString());
        foreach (var option in item.Options ?? [])
        {
            var radio = new RadioButton
            {
                GroupName = groupName,
                Content = option,
                IsChecked = Equals(item.Value, option),
                VerticalContentAlignment = VAlignment.Center,
            };
            radio.IsCheckedChanged += (_, _) =>
            {
                if (radio.IsChecked == true && !Equals(item.Value, option))
                {
                    if (!Commit(item, option))
                    {
                        // 单选组会在状态事件后取消旧选项，需等组内联动结束再恢复。
                        Dispatcher.UIThread.Post(() =>
                        {
                            if (ReferenceEquals(Item, item)) SynchronizeValue();
                        });
                    }
                }
            };
            panel.Children.Add(radio);
        }
        _updateValue = () =>
        {
            foreach (var radio in panel.Children.OfType<RadioButton>())
                radio.IsChecked = Equals(item.Value, radio.Content);
        };

        return panel;
    }

    private Control BuildPercent(PropertyItem item)
    {
        var slider = new Slider
        {
            Minimum = item.Minimum ?? 0,
            Maximum = item.Maximum ?? 100,
            Value = ToDouble(item.Value),
        };
        var label = new TextBlock
        {
            Classes = { "pg-unit" },
            Width = 38,
            TextAlignment = TextAlignment.Right,
            VerticalAlignment = VAlignment.Center,
            Text = FormatPercent(item.Value),
        };

        var dragging = false;
        slider.PointerPressed += (_, _) => dragging = true;
        slider.PointerReleased += (_, _) =>
        {
            if (!dragging)
            {
                return;
            }

            dragging = false;
            if (!Equals(item.Value, slider.Value))
            {
                Commit(item, slider.Value);
            }
        };
        slider.ValueChanged += (_, _) =>
        {
            if (_synchronizing) return;
            label.Text = Math.Round(slider.Value, 0) + "%";
            if (dragging)
            {
                // 拖动时仅预览，预览被取消时回退滑块值。
                var owner = Owner;
                if (owner is not null && !Equals(item.Value, slider.Value)
                    && owner.PreviewValue(item, slider.Value))
                {
                    slider.Value = ToDouble(item.Value);
                }

                return;
            }

            if (!Equals(item.Value, slider.Value))
            {
                Commit(item, slider.Value); // 非拖动操作（键盘调整）立即提交
            }
        };
        _updateValue = () =>
        {
            slider.Value = ToDouble(item.Value);
            label.Text = FormatPercent(item.Value);
        };

        var panel = new DockPanel { LastChildFill = true };
        DockPanel.SetDock(label, Dock.Right);
        panel.Children.Add(label);
        panel.Children.Add(slider);
        return panel;
    }

    private Control BuildDateTime(PropertyItem item)
    {
        var box = new TextBox
        {
            Classes = { "pg-input" },
            Text = item.Value is DateTime dt ? dt.ToString("yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture) : string.Empty,
            HorizontalContentAlignment = HAlignment.Left,
            VerticalContentAlignment = VAlignment.Center,
            PlaceholderText = item.Watermark ?? "yyyy/MM/dd HH:mm",
        };
        var committed = box.Text;
        _updateValue = () =>
        {
            committed = item.Value is DateTime value ? value.ToString("yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture) : string.Empty;
            box.Text = committed;
        };

        void CommitText()
        {
            if (box.Text == committed)
            {
                return;
            }

            if (DateTime.TryParse(box.Text, CultureInfo.CurrentCulture, DateTimeStyles.None, out var parsed)
                || DateTime.TryParse(box.Text, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
            {
                if (Commit(item, parsed))
                {
                    committed = box.Text;
                    return;
                }
            }

            box.Text = committed; // 日期解析失败或预览被取消时回退
        }

        box.LostFocus += (_, _) => CommitText();
        box.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter)
            {
                CommitText();
            }
            else if (e.Key == Key.Escape)
            {
                box.Text = committed;
            }
        };

        return box;
    }

    private Control BuildColor(PropertyItem item)
    {
        var current = item.Value as string;
        var swatch = new Border
        {
            Classes = { "pg-swatch" },
            Width = 16,
            Height = 16,
            CornerRadius = new CornerRadius(3),
            Background = ParseBrush(current),
            VerticalAlignment = VAlignment.Center,
        };
        var hex = new TextBlock
        {
            Classes = { "pg-readonly" },
            Text = current ?? string.Empty,
            VerticalAlignment = VAlignment.Center,
        };

        var picker = new global::Avalonia.Controls.ColorPicker
        {
            IsAlphaEnabled = false,
            Color = ParseColor(current),
        };
        var popup = new Popup
        {
            Placement = PlacementMode.Bottom,
            PlacementTarget = swatch,
            IsLightDismissEnabled = true,
            Child = picker,
        };
        picker.ColorChanged += (_, e) =>
        {
            var value = ToHex(e.NewColor);
            swatch.Background = new SolidColorBrush(e.NewColor);
            hex.Text = value;
            if (!Equals(item.Value, value))
            {
                Commit(item, value);
            }
        };
        _updateValue = () =>
        {
            picker.Color = ParseColor(item.Value as string);
            swatch.Background = ParseBrush(item.Value as string);
            hex.Text = item.Value as string ?? string.Empty;
        };
        swatch.PointerReleased += (_, _) => popup.IsOpen = true;

        var panel = new StackPanel
        {
            Orientation = LOrientation.Horizontal,
            Spacing = 6,
            VerticalAlignment = VAlignment.Center,
        };
        panel.Children.Add(swatch);
        panel.Children.Add(hex);
        panel.Children.Add(popup);
        return panel;
    }

    private Control BuildPath(PropertyItem item)
    {
        var box = (TextBox)BuildTextBox(item, multiline: false);
        var browse = new Button
        {
            Classes = { "pg-mini-button" },
            Content = "…",
            VerticalAlignment = VAlignment.Center,
        };
        browse.Click += (_, _) => Owner?.RaiseBrowseRequested(item);

        var panel = new DockPanel { LastChildFill = true };
        DockPanel.SetDock(browse, Dock.Right);
        panel.Children.Add(browse);
        panel.Children.Add(box);
        return panel;
    }

    private Control BuildButton(PropertyItem item)
    {
        var button = new Button
        {
            Classes = { "pg-full-button" },
            Content = item.Watermark ?? "…",
            HorizontalContentAlignment = HAlignment.Left,
            HorizontalAlignment = HAlignment.Stretch,
        };
        button.Click += (_, _) => Owner?.RaiseBrowseRequested(item);
        return button;
    }

    private static string FormatNumber(PropertyItem item, double value)
        => double.IsFinite(value)
            ? value.ToString(string.IsNullOrWhiteSpace(item.Format) ? "0.###" : item.Format, CultureInfo.InvariantCulture)
            : "0";

    private static decimal ToDecimal(object? value) => value switch
    {
        decimal d => d,
        IConvertible convertible => convertible.ToDecimal(CultureInfo.InvariantCulture),
        _ => 0m,
    };

    private static double ToDouble(object? value) => value switch
    {
        double d => d,
        IConvertible convertible => convertible.ToDouble(CultureInfo.InvariantCulture),
        _ => 0,
    };

    private static string FormatPercent(object? value)
        => Math.Round(ToDouble(value), 0).ToString(CultureInfo.InvariantCulture) + "%";

    private static Color ParseColor(string? text)
    {
        if (!string.IsNullOrEmpty(text) && Color.TryParse(text, out var color))
        {
            return color;
        }

        return Colors.Transparent;
    }

    private static IBrush ParseBrush(string? text)
        => new SolidColorBrush(ParseColor(text));

    private static string ToHex(Color color)
        => $"#{color.R:X2}{color.G:X2}{color.B:X2}";
}
