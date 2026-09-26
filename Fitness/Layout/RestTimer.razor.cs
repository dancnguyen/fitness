using Fitness.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace Fitness.Layout
{
  public partial class RestTimer : IDisposable
  {
    [Inject]
    private Blazored.LocalStorage.ILocalStorageService LocalStorage { get; set; } = default!;

    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    [Parameter]
    public bool IsDarkMode { get; set; }

    private enum TimerState { Idle, Running, Paused, Alarming }

    private const string RestTimerSecondsKey = "restTimerSeconds";
    private const int DefaultSeconds = 90;
    private const int AlarmMaxSeconds = 30;

    private TimerState State { get; set; } = TimerState.Idle;
    private int DurationSeconds { get; set; } = DefaultSeconds;
    private DateTime EndTimeUtc { get; set; }
    private TimeSpan Remaining { get; set; }
    private Timer? Ticker { get; set; }

    private string DisplayTime => FormatTime(State == TimerState.Idle ? TimeSpan.FromSeconds(DurationSeconds) : Remaining);

    private string MainIcon => State switch
    {
      TimerState.Running => Icons.Material.Filled.Pause,
      TimerState.Paused => Icons.Material.Filled.PlayArrow,
      TimerState.Alarming => Icons.Material.Filled.NotificationsActive,
      _ => Icons.Material.Filled.Timer,
    };

    private string MainLabel => State switch
    {
      TimerState.Running => "Pause rest timer",
      TimerState.Paused => "Resume rest timer",
      TimerState.Alarming => "Dismiss rest timer alarm",
      _ => "Start rest timer",
    };

    protected override async Task OnInitializedAsync()
    {
      int? seconds = await LocalStorage.GetItemAsync<int?>(RestTimerSecondsKey);
      if (seconds > 0)
        DurationSeconds = seconds.Value;
    }

    public static string FormatTime(TimeSpan time)
    {
      int totalSeconds = (int)Math.Ceiling(Math.Max(0, time.TotalSeconds));
      return $"{totalSeconds / 60}:{totalSeconds % 60:00}";
    }

    private async Task OnMainClick()
    {
      switch (State)
      {
        case TimerState.Idle:
          await Start(TimeSpan.FromSeconds(DurationSeconds));
          break;
        case TimerState.Running:
          await Pause();
          break;
        case TimerState.Paused:
          await Start(Remaining);
          break;
        case TimerState.Alarming:
          await Reset();
          break;
      }
    }

    private async Task OnSecondaryClick()
    {
      if (State == TimerState.Idle)
        await ShowSettings();
      else
        await Reset();
    }

    private async Task Start(TimeSpan duration)
    {
      EndTimeUtc = DateTime.UtcNow + duration;
      Remaining = duration;
      State = TimerState.Running;
      Ticker ??= new Timer(_ => _ = InvokeAsync(Tick), null, 0, 250);
      await JSRuntime.InvokeVoidAsync("restTimer.setKeepAwake", true);
    }

    private async Task Pause()
    {
      Remaining = EndTimeUtc - DateTime.UtcNow;
      StopTicker();
      State = TimerState.Paused;
      await JSRuntime.InvokeVoidAsync("restTimer.setKeepAwake", false);
    }

    private async Task Reset()
    {
      StopTicker();
      if (State == TimerState.Alarming)
        await JSRuntime.InvokeVoidAsync("restTimer.stopAlarm");
      State = TimerState.Idle;
      await JSRuntime.InvokeVoidAsync("restTimer.setKeepAwake", false);
    }

    private async Task Tick()
    {
      if (State != TimerState.Running)
        return;
      Remaining = EndTimeUtc - DateTime.UtcNow;
      if (Remaining <= TimeSpan.Zero)
      {
        Remaining = TimeSpan.Zero;
        StopTicker();
        State = TimerState.Alarming;
        await JSRuntime.InvokeVoidAsync("restTimer.startAlarm", AlarmMaxSeconds);
      }
      StateHasChanged();
    }

    private void StopTicker()
    {
      Ticker?.Dispose();
      Ticker = null;
    }

    private async Task ShowSettings()
    {
      DialogParameters<RestTimerDialog> parameters = new DialogParameters<RestTimerDialog> { { x => x.Seconds, DurationSeconds }, { x => x.IsDarkMode, IsDarkMode } };
      DialogOptions options = new DialogOptions { MaxWidth = MaxWidth.ExtraSmall, FullWidth = true };
      IDialogReference restTimerDialog = await DialogService.ShowAsync<RestTimerDialog>("Rest Timer", parameters, options);
      DialogResult? restTimerResult = await restTimerDialog.Result;
      if (restTimerResult == null || restTimerResult.Canceled || restTimerResult.Data is not int seconds)
        return;
      DurationSeconds = seconds;
      await LocalStorage.SetItemAsync(RestTimerSecondsKey, DurationSeconds);
    }

    public void Dispose() => StopTicker();
  }
}
