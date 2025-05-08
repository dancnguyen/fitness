using Fitness.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using MudBlazor;
using System.Text;
using static MudBlazor.CategoryTypes;

namespace Fitness.Pages
{
  public partial class Home
  {
    [Inject]
    private Blazored.LocalStorage.ILocalStorageService LocalStorage { get; set; }

    [Inject]
    private IDialogService DialogService { get; set; }

    [Inject]
    private ISnackbar Snackbar { get; set; }

    private string Notification { get; set; } = string.Empty;

    private Storage Storage { get; set; } = new Storage();

    protected override async Task OnInitializedAsync()
    {
      Storage? storage = await LocalStorage.GetItemAsync<Storage>("storage");
      Storage = storage ?? new Storage();
    }

    private async Task SaveProgress()
    {
      Workout? sessionInHistory = Storage.WorkoutHistory.Where(x => x.SessionType.Equals(Storage.CurrentWorkout.SessionType)).FirstOrDefault();
      if (sessionInHistory != null) 
        Storage.WorkoutHistory.Remove(sessionInHistory);
      Storage.WorkoutHistory.Add(Storage.CurrentWorkout);
      await LocalStorage.SetItemAsync("storage", Storage);
      Notification = $"Progress Saved! {DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")}";
      Snackbar.Add(Notification, Severity.Success);
    }

    private async Task LoadWorkout()
    {
      Notification = string.Empty;
      DialogOptions options = new DialogOptions { MaxWidth = MaxWidth.ExtraSmall };
      IDialogReference loadWorkoutDialog = await DialogService.ShowAsync<LoadWorkoutDialog>("Load Workout", options);
      DialogResult? loadWorkoutResult = await loadWorkoutDialog.Result;
      if (loadWorkoutResult == null || loadWorkoutResult.Canceled)
        return;
      string sessionType = loadWorkoutResult.Data as string ?? string.Empty;
      Workout? previousWorkout = Storage.WorkoutHistory.Where(x => x.SessionType.Equals(sessionType)).FirstOrDefault();
      if (previousWorkout == null)
      {
        DialogParameters<SetupPreviousWorkoutDialog> parameters = new DialogParameters<SetupPreviousWorkoutDialog> { { x => x.SessionType, sessionType } };
        IDialogReference setupPreviousWorkoutDialog = await DialogService.ShowAsync<SetupPreviousWorkoutDialog>("Setup Previous Workout", parameters, options);
        DialogResult? setupPreviousWorkoutResult = await setupPreviousWorkoutDialog.Result;
        if (setupPreviousWorkoutResult == null || setupPreviousWorkoutResult.Canceled)
          return;
        previousWorkout = setupPreviousWorkoutResult.Data as Workout ?? new();
      }
      Storage.CurrentWorkout.SessionType = sessionType;
      Storage.PreviousWorkout = previousWorkout;
      LoadCurrentWorkoutFromPrevious(sessionType);
    }

    private async Task DeleteSessionTypeData()
    {
      if (!await PromptConfirmation($"Would you like to delete the {Storage.CurrentWorkout.SessionType} session configuration and history?"))
        return;
      if (!await PromptConfirmation("Are you sure???"))
        return;
      if (!await PromptConfirmation("Are you sure you're sure???"))
        return;

      Workout? sessionInHistory = Storage.WorkoutHistory.Where(x => x.SessionType.Equals(Storage.CurrentWorkout.SessionType)).FirstOrDefault();
      if (sessionInHistory != null)
        Storage.WorkoutHistory.Remove(sessionInHistory);
      Storage.CurrentWorkout = new();
      Storage.PreviousWorkout = new();
      await LocalStorage.SetItemAsync("storage", Storage);
    }

    private async Task ResetToPreviousWorkout(Exercise exercise)
    {
      if (!await PromptConfirmation($"Would you like to reset {exercise.Name} back to the previous {exercise.Name} settings?"))
        return;

      Exercise? previousExercise = Storage.PreviousWorkout.Exercises.Where(x => x.Name.Equals(exercise.Name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
      Exercise? currentExercise = Storage.CurrentWorkout.Exercises.Where(x => x.Name.Equals(exercise.Name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
      if (previousExercise == null || currentExercise == null)
        return;
      Exercise resetExercise = new()
      {
        Name = previousExercise.Name,
        Weight = previousExercise.Weight,
        Measurement = previousExercise.Measurement,
        MinReps = previousExercise.MinReps,
        MaxReps = previousExercise.MaxReps,
        Sets = previousExercise.Sets,
        CompletedSets = 0,
      };
      int indexExercise = Storage.CurrentWorkout.Exercises.IndexOf(currentExercise);
      Storage.CurrentWorkout.Exercises.Remove(currentExercise);
      Storage.CurrentWorkout.Exercises.Insert(indexExercise, resetExercise);
    }

    private void LoadCurrentWorkoutFromPrevious(string sessionType)
    {
      List<Exercise> currentExercises = new();
      foreach (Exercise previousExercise in Storage.PreviousWorkout.Exercises)
      {
        Exercise currentExercise = new()
        {
          Name = previousExercise.Name,
          Weight = previousExercise.Weight,
          Measurement = previousExercise.Measurement,
          MinReps = previousExercise.MinReps,
          MaxReps = previousExercise.MaxReps,
          Sets = previousExercise.Sets,
          CompletedSets = 0,
        };
        currentExercises.Add(currentExercise);
      }
      Storage.CurrentWorkout = new()
      {
        SessionType = sessionType,
        Exercises = currentExercises,
      };
    }

    private void IncrementCompletedSet(Exercise exercise)
    {
      if (exercise.CompletedSets >= exercise.Sets)
      {
        exercise.CompletedSets = 0;
        return;
      }
      exercise.CompletedSets++;
    }

    private async Task IncrementRep(Exercise exercise) 
    {
      if (!await PromptConfirmation("Would you like to increment a rep?"))
        return;

      exercise.MinReps++;
      exercise.MaxReps++;
    }

    private async Task IncrementWeight(Exercise exercise, int increment)
    {
      if (!await PromptConfirmation("Would you like to increment the weight?"))
        return;

      exercise.Weight += increment;
    }

    private async Task<bool> PromptConfirmation(string confirmationText)
    {
      DialogParameters<ConfirmationDialog> parameters = new DialogParameters<ConfirmationDialog> { { x => x.ConfirmationText, confirmationText } };
      DialogOptions options = new DialogOptions { MaxWidth = MaxWidth.ExtraSmall };
      IDialogReference confirmationDialog = await DialogService.ShowAsync<ConfirmationDialog>("Confirm", parameters, options);
      DialogResult? confirmationDialogResult = await confirmationDialog.Result;
      if (confirmationDialogResult == null || confirmationDialogResult.Canceled)
        return false;
      else
        return true;
    }

    private Func<Exercise, string> ChangedRepsCellStyleFunc => exercise =>
    {
      string style = "";

      Exercise? previousExercise = Storage.PreviousWorkout.Exercises.Where(x => x.Name.Equals(exercise.Name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
      if (previousExercise == null)
        return style;

      if (exercise.MinReps != previousExercise.MinReps)
        style += "color:#FFD700;background-color:#000000;";

      return style;
    };

    private Func<Exercise, string> ChangedWeightCellStyleFunc => exercise =>
    {
      string style = "";

      Exercise? previousExercise = Storage.PreviousWorkout.Exercises.Where(x => x.Name.Equals(exercise.Name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
      if (previousExercise == null)
        return style;

      if (exercise.Weight != previousExercise.Weight)
        style += "color:#FFD700;background-color:#8B0000;";

      return style;
    };

    private async Task ShowCurrentWorkoutAsText()
    {
      StringBuilder builder = new StringBuilder();
      foreach (Exercise exercise in Storage.CurrentWorkout.Exercises)
      {
        for (int i = 1; i <= exercise.CompletedSets; i++)
          builder.Append(i);
        builder.Append(" - ");
        builder.Append(exercise.FormatReps() + " reps");
        builder.Append(" - ");
        builder.Append(exercise.FormatWeight());
        builder.Append(" - ");
        builder.Append(exercise.Name);
        builder.Append(Environment.NewLine);
        builder.Append(Environment.NewLine);
      }
      DialogParameters<ShowTextDialog> parameters = new DialogParameters<ShowTextDialog> { { x => x.ShowText, builder.ToString() } };
      DialogOptions options = new DialogOptions { MaxWidth = MaxWidth.Large };
      IDialogReference showTextDialog = await DialogService.ShowAsync<ShowTextDialog>("Show Text", parameters, options);
      await showTextDialog.Result;
    }
  }
}
