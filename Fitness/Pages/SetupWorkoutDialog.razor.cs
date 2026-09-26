using Fitness.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Fitness.Pages
{
  public partial class SetupWorkoutDialog
  {
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    [Parameter]
    public string SessionType { get; set; } = default!;

    private MudForm Form { get; set; } = default!;

    private string ErrorMessage { get; set; } = string.Empty;

    private Workout PreviousWorkout { get; set; } = new();

    private List<Exercise> ExerciseList { get; set; } = new();

    private Exercise Exercise { get; set; } = new();

    private bool ResetValidationPending { get; set; }

    private MudTextField<string> NameField { get; set; } = default!;

    private MudTextField<int?> MaxRepsField { get; set; } = default!;

    private bool CheckMaxReps { get; set; }

    protected override async Task OnInitializedAsync()
    {
      await base.OnInitializedAsync();
      PreviousWorkout.SessionType = SessionType;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
      await base.OnAfterRenderAsync(firstRender);
      if (ResetValidationPending)
      {
        ResetValidationPending = false;
        await Form.ResetValidationAsync();
        await NameField.FocusAsync();
      }
    }

    private async Task AddExercise()
    {
      CheckMaxReps = true;
      await Form.ValidateAsync();
      CheckMaxReps = false;
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
      ResetValidationPending = true;
      Snackbar.Add("Successfully added exercise!", Severity.Success);
    }

    private const string MaxRepsErrorText = "Maximum Reps cannot be less than Minimum Reps";

    private string? ValidateMaxReps(int? maxReps)
    {
      if (CheckMaxReps && maxReps < Exercise.MinReps)
        return MaxRepsErrorText;
      return null;
    }

    private async Task ClearMaxRepsError()
    {
      if (MaxRepsField.ValidationErrors.Contains(MaxRepsErrorText))
        await MaxRepsField.ResetValidationAsync();
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
