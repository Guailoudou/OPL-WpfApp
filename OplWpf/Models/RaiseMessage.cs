using System.Windows;

namespace OplWpf.Models;

public class RaiseMessage
{
    public required string Message { get; set; }
    public required string Caption { get; set; }
    public MessageBoxButton Button { get; set; } = MessageBoxButton.OK;
    public MessageBoxImage? Icon { get; set; } = null;
};