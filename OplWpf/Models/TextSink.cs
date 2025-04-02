using CommunityToolkit.Mvvm.ComponentModel;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;
using System.IO;

namespace OplWpf.Models;

public partial class TextSink(string format) : ObservableObject, ILogEventSink
{
    [ObservableProperty] public partial string LogText { get; set; } = "";

    private readonly StringWriter _stringWriter = new();

    private readonly MessageTemplateTextFormatter _formatter = new(format);

    public void Emit(LogEvent logEvent)
    {
        _formatter.Format(logEvent, _stringWriter);
        LogText = _stringWriter.ToString();
    }
}