using Fitness.Layout;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Fitness.Pages
{
  public partial class RestTimerDialog
  {
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = default!;

    [Parameter]
    public int Seconds { get; set; }

    [Parameter]
    public bool IsDarkMode { get; set; }

    private const int StepSeconds = 15;
    private const int MinSeconds = 15;
    private const int MaxSeconds = 60 * 60;

    private static readonly int[] Presets = { 30, 60, 90, 120, 150, 180, 240, 300 };

    private string InactivePresetStyle => IsDarkMode ? "color: #fff; border-color: #fff;" : "color: #000; border-color: #000;";

    private void Adjust(int delta) => Seconds = Math.Clamp(Seconds + delta, MinSeconds, MaxSeconds);

    private void Submit() => MudDialog.Close(DialogResult.Ok(Seconds));

    private void Cancel() => MudDialog.Cancel();
  }
}
