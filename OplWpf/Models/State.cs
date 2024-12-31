using CommunityToolkit.Mvvm.ComponentModel;

namespace OplWpf.Models;

public enum State
{
    Stop,
    Loading,
    Running
}

[Injection(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
public partial class StateManager : ObservableObject
{
    [ObservableProperty]
    public partial State MainState { get; set; } = State.Stop;

    public Dictionary<string, State> AppState { get; } = [];

    public State this[string ip]
    {
        get => AppState[ip];
        set
        {
            AppState[ip] = value;
            OnPropertyChanged(nameof(AppState));
        }
    }

    public void Clear()
    {
        AppState.Clear();
        OnPropertyChanged(nameof(AppState));
    }
}
