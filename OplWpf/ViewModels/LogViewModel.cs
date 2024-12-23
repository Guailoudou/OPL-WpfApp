using CommunityToolkit.Mvvm.ComponentModel;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;
using System.IO;

namespace OplWpf.ViewModels;

public partial class LogViewModel : ObservableObject, ILogEventSink
{
    [ObservableProperty] public partial string LogText { get; set; } = "";

    private readonly StringWriter _stringWriter = new();

    private readonly MessageTemplateTextFormatter _formatter;

    public LogViewModel()
    {
        const string logFormat = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] {Message:lj}{NewLine}{Exception}";
        _formatter = new MessageTemplateTextFormatter(logFormat);
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Sink(this)
            .WriteTo.File(Path.Combine(AppContext.BaseDirectory, "bin", "log", "opl.log"),
                outputTemplate: logFormat)
            .CreateLogger();
    }

    public void Emit(LogEvent logEvent)
    {
        _formatter.Format(logEvent, _stringWriter);
        LogText = _stringWriter.ToString();
    }
}