using System.Windows.Input;
using ReactiveUI;

namespace CodeWF.Avalonia.Demo.ViewModels.Pages;

public sealed class ContentExpansionViewModel : ReactiveObject
{
    private bool _isExpanded = true;
    private bool _animate = true;
    private double _milliseconds = 300;
    private double _size = 200;

    public ContentExpansionViewModel() => ToggleCommand = ReactiveCommand.Create(() => IsExpanded = !IsExpanded);

    public bool IsExpanded { get => _isExpanded; set => this.RaiseAndSetIfChanged(ref _isExpanded, value); }
    public bool Animate { get => _animate; set => this.RaiseAndSetIfChanged(ref _animate, value); }
    public double ExpandedSize { get => _size; set => this.RaiseAndSetIfChanged(ref _size, value); }
    public double Milliseconds
    {
        get => _milliseconds;
        set
        {
            this.RaiseAndSetIfChanged(ref _milliseconds, value);
            this.RaisePropertyChanged(nameof(Duration));
        }
    }
    public TimeSpan Duration => TimeSpan.FromMilliseconds(Milliseconds);
    public ICommand ToggleCommand { get; }
}
