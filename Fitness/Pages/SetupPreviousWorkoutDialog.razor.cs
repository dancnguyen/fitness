using Fitness.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Fitness.Pages
{
  public partial class SetupPreviousWorkoutDialog
  {
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; }

    [Inject]
    private ISnackbar Snackbar { get; set; }

    [Parameter]
    public string SessionType { get; set; }

    private MudForm Form { get; set; }

    private string ErrorMessage { get; set; } = string.Empty;

    private Workout PreviousWorkout { get; set; } = new();

    private List<Exercise> ExerciseList { get; set; } = new();

    private Exercise Exercise { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
      await base.OnInitializedAsync();
      PreviousWorkout.SessionType = SessionType;
    }

    private async Task AddExercise()
    {
      await Form.Validate();
      if (!Form.IsValid)
        return;
      ErrorMessage = string.Empty;
      if (ExerciseList.Where(x => x.Name.Equals(Exercise.Name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault() != null)
      {
        string error = $"Exercise {Exercise.Name} has already been added to the session!";
        ErrorMessage = error;
        Snackbar.Add(error, Severity.Error);
        return;
      }
      ExerciseList.Add(Exercise);
      Exercise = new();
      Snackbar.Add("Successfully added exercise!", Severity.Success);
    }

    private void DeleteExercise(Exercise exercise)
    {
      ExerciseList.Remove(exercise);
      Snackbar.Add("Successfully removed exercise!", Severity.Success);
    }

    private void Submit() 
    {
      if (ExerciseList.Count == 0)
      {
        string error = "Please add at least one exercise!";
        ErrorMessage = error;
        Snackbar.Add(error, Severity.Error);
        return;
      }
      PreviousWorkout.Exercises = ExerciseList;
      MudDialog.Close(DialogResult.Ok(PreviousWorkout)); 
    }


    private void Cancel() => MudDialog.Cancel();
  }
}
