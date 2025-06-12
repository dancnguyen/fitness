using Fitness.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Fitness.Pages
{
  public partial class LoadWorkoutDialog
  {
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = default!;

    [Inject]
    private Blazored.LocalStorage.ILocalStorageService LocalStorage { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private List<string> PreviousSessionTypes { get; set; } = new List<string>();

    private string SessionNewType { get; set; } = string.Empty;

    private Storage Storage { get; set; } = new Storage();

    protected override async Task OnInitializedAsync()
    {
      Storage? storage = await LocalStorage.GetItemAsync<Storage>("storage");
      Storage = storage ?? new Storage();
      foreach (Workout workout in Storage.WorkoutHistory)
        PreviousSessionTypes.Add(workout.SessionType);
    }

    private void CreateSessionType()
    {
      if (string.IsNullOrWhiteSpace(SessionNewType))
      {
        Snackbar.Add("Unable to Create New Session Type! Session Type Name is blank.", Severity.Error);
        return;
      }

      string capitalizedSession = SessionNewType.Trim().ToUpper();
      if (Storage.WorkoutHistory.Select(x => x.SessionType).ToList().Contains(capitalizedSession))
      {
        Snackbar.Add("Unable to Create New Session Type! Session Type Name already exists and must be unique.", Severity.Error);
        return;
      }

      MudDialog.Close(DialogResult.Ok(capitalizedSession));
    }

    private void OnSessionClick(string sessionType) => MudDialog.Close(DialogResult.Ok(sessionType));

    private void Cancel() => MudDialog.Cancel();
  }
}
