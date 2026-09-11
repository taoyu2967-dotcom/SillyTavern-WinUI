using System.Windows.Input;

namespace SillyTavernWinUI.Services;

/// <summary>最小化 ICommand 实现，用于 H.NotifyIcon 的 DoubleClickCommand。</summary>
internal sealed class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;

    public RelayCommand(Action<object?> execute)
    {
        _execute = execute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add { }
        remove { }
    }

    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter) => _execute(parameter);
}
