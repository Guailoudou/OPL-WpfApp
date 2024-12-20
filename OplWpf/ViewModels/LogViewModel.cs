using CommunityToolkit.Mvvm.ComponentModel;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;
using System.IO;

namespace OplWpf.ViewModels;

public partial class LogViewModel : ObservableObject, ILogEventSink
{
    public string LogText => stringWriter.ToString();

    private readonly StringWriter stringWriter = new();

    private readonly MessageTemplateTextFormatter formatter;

    public LogViewModel()
    {
        var logFormat = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] {Message:lj}{NewLine}{Exception}";
        formatter = new MessageTemplateTextFormatter(logFormat, null);
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Sink(this)
            .WriteTo.File(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "log", "opl.log"), outputTemplate: logFormat)
            .CreateLogger();
    }

    public void Emit(LogEvent logEvent)
    {
        formatter.Format(logEvent, stringWriter);
        OnPropertyChanged(nameof(LogText));
    }
}
