using Fitness.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using System.Text;

namespace Fitness.Pages
{
  public partial class AppManagementDialog
  {
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = default!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    [Parameter]
    public Storage Storage { get; set; } = new Storage();

    private string InfoText
    {
      get
      {
        return "Source Code:";
      }
    }

    private async Task DownloadData()
    {
      StringBuilder builder = new StringBuilder();
      builder.AppendLine("Session,Exercise,Weight,MinReps,MaxReps,Sets");
      foreach (Workout workout in Storage.WorkoutHistory)
      {
        string sessionType = workout.SessionType;
        foreach (Exercise exercise in workout.Exercises)
        {
          string delimiter = ",";
          StringBuilder row = new StringBuilder();
          row.Append(sessionType + delimiter);
          row.Append(exercise.Name + delimiter);
          row.Append(exercise.FormatWeight() + delimiter);
          row.Append(exercise.MinReps + delimiter);
          row.Append(exercise.MaxReps + delimiter);
          row.Append(exercise.Sets);
          builder.AppendLine(row.ToString());
        }
      }
      string data = builder.ToString();
      string fileName = $"WorkoutData_{DateTime.Now.ToString("MM_dd_yyyy_HH_mm_ss")}.csv";
      await JSRuntime.InvokeVoidAsync("downloadFile", fileName, data);
    }

    private void Close() => MudDialog.Cancel();
  }
}
