using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia.Data.Converters;
using Dock.Model.Core;

namespace CodeWF.Avalonia.Dock.Converters;

/// <summary>
///     Adapts strongly typed Dock factory methods to commands that Avalonia can bind directly.
/// </summary>
public sealed class DockFactoryCommandConverter : IValueConverter
{
    private readonly DockFactoryOperation _operation;
    private readonly ConditionalWeakTable<IFactory, DockFactoryCommand> _commands = new();

    private DockFactoryCommandConverter(DockFactoryOperation operation)
    {
        _operation = operation;
    }

    public static IValueConverter CloseDockable { get; } =
        new DockFactoryCommandConverter(DockFactoryOperation.CloseDockable);

    public static IValueConverter PinDockable { get; } =
        new DockFactoryCommandConverter(DockFactoryOperation.PinDockable);

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is IFactory factory
            ? _commands.GetValue(factory, currentFactory => new DockFactoryCommand(currentFactory, _operation))
            : null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    private enum DockFactoryOperation
    {
        CloseDockable,
        PinDockable
    }

    private sealed class DockFactoryCommand(IFactory factory, DockFactoryOperation operation) : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter)
        {
            return parameter is IDockable;
        }

        public void Execute(object? parameter)
        {
            if (parameter is not IDockable dockable) return;

            switch (operation)
            {
                case DockFactoryOperation.CloseDockable:
                    factory.CloseDockable(dockable);
                    break;
                case DockFactoryOperation.PinDockable:
                    factory.PinDockable(dockable);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
