using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace CodeWF.Avalonia.Controls.Animations;

/// <summary>在控件变为有效可见时播放入场动画，只改变透明度和合成变换。</summary>
public sealed class EntranceMotion : AvaloniaObject
{
    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<EntranceMotion, Control, bool>("IsEnabled");
    public static readonly AttachedProperty<bool> AnimateProperty =
        AvaloniaProperty.RegisterAttached<EntranceMotion, Control, bool>("Animate", true);
    public static readonly AttachedProperty<TimeSpan> DurationProperty =
        AvaloniaProperty.RegisterAttached<EntranceMotion, Control, TimeSpan>("Duration", TimeSpan.FromMilliseconds(140),
            validate: value => value >= TimeSpan.Zero);
    public static readonly AttachedProperty<double> OffsetYProperty =
        AvaloniaProperty.RegisterAttached<EntranceMotion, Control, double>("OffsetY", validate: double.IsFinite);
    public static readonly AttachedProperty<double> InitialScaleProperty =
        AvaloniaProperty.RegisterAttached<EntranceMotion, Control, double>("InitialScale", 1,
            validate: value => double.IsFinite(value) && value > 0);
    public static readonly AttachedProperty<Easing> EasingProperty =
        AvaloniaProperty.RegisterAttached<EntranceMotion, Control, Easing>("Easing", new CubicEaseOut());
    private static readonly AttachedProperty<State?> StateProperty =
        AvaloniaProperty.RegisterAttached<EntranceMotion, Control, State?>("State");

    static EntranceMotion()
    {
        IsEnabledProperty.Changed.AddClassHandler<Control>((control, _) =>
        {
            control.GetValue(StateProperty)?.Dispose();
            control.SetValue(StateProperty, GetIsEnabled(control) ? new State(control) : null);
        });
        AnimateProperty.Changed.AddClassHandler<Control>((control, _) =>
        {
            if (!GetAnimate(control)) control.GetValue(StateProperty)?.Cancel();
        });
        DurationProperty.Changed.AddClassHandler<Control>((control, _) =>
        {
            if (GetDuration(control) == TimeSpan.Zero) control.GetValue(StateProperty)?.Cancel();
        });
    }

    public static bool GetIsEnabled(Control control) => control.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(Control control, bool value) => control.SetValue(IsEnabledProperty, value);
    public static bool GetAnimate(Control control) => control.GetValue(AnimateProperty);
    public static void SetAnimate(Control control, bool value) => control.SetValue(AnimateProperty, value);
    public static TimeSpan GetDuration(Control control) => control.GetValue(DurationProperty);
    public static void SetDuration(Control control, TimeSpan value) => control.SetValue(DurationProperty, value);
    public static double GetOffsetY(Control control) => control.GetValue(OffsetYProperty);
    public static void SetOffsetY(Control control, double value) => control.SetValue(OffsetYProperty, value);
    public static double GetInitialScale(Control control) => control.GetValue(InitialScaleProperty);
    public static void SetInitialScale(Control control, double value) => control.SetValue(InitialScaleProperty, value);
    public static Easing GetEasing(Control control) => control.GetValue(EasingProperty);
    public static void SetEasing(Control control, Easing value) => control.SetValue(EasingProperty, value);

    private sealed class State : IDisposable
    {
        private readonly Control _control;
        private readonly List<Visual> _ancestors = [];
        private CancellationTokenSource? _run;
        private bool _attached;
        private bool _visible;
        private bool _checkPending;
        private bool _disposed;

        public State(Control control)
        {
            _control = control;
            control.AttachedToVisualTree += Attached;
            control.DetachedFromVisualTree += Detached;
            if (control.IsAttachedToVisualTree()) Attach();
        }

        private void Attached(object? sender, VisualTreeAttachmentEventArgs e) => Attach();
        private void Attach()
        {
            if (_attached) return;
            _attached = true;
            for (Visual? current = _control; current is not null; current = current.GetVisualParent())
            {
                _ancestors.Add(current);
                current.PropertyChanged += VisibilityChanged;
            }
            QueueVisibilityCheck();
        }
        private void Detached(object? sender, VisualTreeAttachmentEventArgs e) => Detach();
        private void Detach()
        {
            _attached = false;
            foreach (var visual in _ancestors) visual.PropertyChanged -= VisibilityChanged;
            _ancestors.Clear();
            _visible = false;
            Cancel();
        }
        private void VisibilityChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == Visual.IsVisibleProperty) QueueVisibilityCheck();
        }
        private void QueueVisibilityCheck()
        {
            if (_checkPending || _disposed) return;
            _checkPending = true;
            Dispatcher.UIThread.Post(() =>
            {
                _checkPending = false;
                if (!_disposed) CheckVisibility();
            }, DispatcherPriority.Loaded);
        }
        private void CheckVisibility()
        {
            var visible = _attached && _control.IsEffectivelyVisible;
            if (_visible == visible) return;
            _visible = visible;
            if (visible) Start(); else Cancel();
        }
        public void Cancel()
        {
            _run?.Cancel();
            _run?.Dispose();
            _run = null;
        }
        private void Start()
        {
            Cancel();
            if (!GetAnimate(_control) || GetDuration(_control) == TimeSpan.Zero) return;
            var first = new KeyFrame { Cue = new Cue(0), Setters = { new Setter(Visual.OpacityProperty, 0d) } };
            var last = new KeyFrame { Cue = new Cue(1), Setters = { new Setter(Visual.OpacityProperty, _control.Opacity) } };
            var offset = GetOffsetY(_control);
            var scale = GetInitialScale(_control);
            if (offset != 0 || scale != 1)
            {
                first.Setters.Add(new Setter(TranslateTransform.YProperty, offset));
                first.Setters.Add(new Setter(ScaleTransform.ScaleXProperty, scale));
                first.Setters.Add(new Setter(ScaleTransform.ScaleYProperty, scale));
                last.Setters.Add(new Setter(TranslateTransform.YProperty, 0d));
                last.Setters.Add(new Setter(ScaleTransform.ScaleXProperty, 1d));
                last.Setters.Add(new Setter(ScaleTransform.ScaleYProperty, 1d));
            }
            var animation = new Animation
            {
                Duration = GetDuration(_control), Easing = GetEasing(_control), Children = { first, last }
            };
            _run = new CancellationTokenSource();
            _ = Run(animation, _run.Token);
        }
        private async Task Run(Animation animation, CancellationToken token)
        {
            try { await animation.RunAsync(_control, token); }
            catch (OperationCanceledException) { }
        }
        public void Dispose()
        {
            _disposed = true;
            Detach();
            _control.AttachedToVisualTree -= Attached;
            _control.DetachedFromVisualTree -= Detached;
        }
    }
}
