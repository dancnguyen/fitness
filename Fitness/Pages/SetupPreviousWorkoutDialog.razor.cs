using Fitness.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Fitness.Pages
{
  public partial class SetupPreviousWorkoutDialog
  {
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; }

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
      ExerciseList.Add(Exercise);
      Exercise = new();
    }

    private void DeleteExercise(Exercise exercise)
    {
      ExerciseList.Remove(exercise);
    }

    private void Submit() 
    {
      if (ExerciseList.Count == 0)
      {
        ErrorMessage = "Please add at least one exercise!";
        return;
      }
      PreviousWorkout.Exercises = ExerciseList;
      MudDialog.Close(DialogResult.Ok(PreviousWorkout)); 
    }


    private void Cancel() => MudDialog.Cancel();
  }
}
