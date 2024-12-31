using OplWpf.Models;

namespace OplWpf.ViewModels;

[Injection(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient)]
public class LogViewModel(TextSink textSink)
{
    public TextSink TextSink { get; } = textSink;
}