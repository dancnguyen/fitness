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
    private Blazored.LocalStorage.ILocalStorageService LocalStorage { get; set; } = default!;

    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private string Notification { get; set; } = string.Empty;
    private bool IsExpandSessionOptions { get; set; } = false;

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
      Storage? storage = await LocalStorage.GetItemAsync<Storage>("storage");
      Storage = storage ?? new Storage();
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
        DialogParameters<SetupWorkoutDialog> parameters = new DialogParameters<SetupWorkoutDialog> { { x => x.SessionType, sessionType } };
        IDialogReference setupPreviousWorkoutDialog = await DialogService.ShowAsync<SetupWorkoutDialog>("Setup Previous Workout", parameters, options);
        DialogResult? setupPreviousWorkoutResult = await setupPreviousWorkoutDialog.Result;
        if (setupPreviousWorkoutResult == null || setupPreviousWorkoutResult.Canceled)
          return;
        previousWorkout = setupPreviousWorkoutResult.Data as Workout ?? new();
      }
      Storage.CurrentWorkout.SessionType = sessionType;
      Storage.PreviousWorkout = previousWorkout;
      LoadCurrentWorkoutFromPrevious(sessionType);
      IsExpandSessionOptions = false;
      Snackbar.Add($"Successfully loaded {sessionType} workout!", Severity.Success);
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
      IsExpandSessionOptions = false;
      Snackbar.Add($"Successfully deleted {Storage.CurrentWorkout.SessionType} workout configuration and history!", Severity.Success);
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
      Snackbar.Add($"Successfully reset {exercise.Name} to previous workout settings!", Severity.Success);
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
      Snackbar.Add($"Successfully incremented completed sets for {exercise.Name}! Good Job!", Severity.Success);
    }

    private async Task EditExercise(Exercise exercise) 
    {
      await DisplayExercise(exercise);
    }

    private async Task AddExercise()
    {
      await DisplayExercise(new());
    }

    private async Task DisplayExercise(Exercise exercise)
    {
      DialogParameters<DisplayExerciseDialog> parameters = new DialogParameters<DisplayExerciseDialog> { { x => x.InputExercise, exercise }, { x => x.ExerciseNames, Storage.CurrentWorkout.Exercises.Select(e => e.Name).ToList() } };
      DialogOptions options = new DialogOptions { MaxWidth = MaxWidth.ExtraSmall };
      IDialogReference displayExerciseDialog = await DialogService.ShowAsync<DisplayExerciseDialog>("Exercise", parameters, options);
      DialogResult? displayExerciseResult = await displayExerciseDialog.Result;
      if (displayExerciseResult == null || displayExerciseResult.Canceled)
        return;
      Exercise resultExercise = displayExerciseResult.Data as Exercise ?? new();
      if (string.IsNullOrWhiteSpace(resultExercise.Name)) // This means that the exercise has been deleted
      {
        Storage.CurrentWorkout.Exercises.Remove(exercise);
        Snackbar.Add($"Successfully removed {exercise.Name} from current workout!", Severity.Success);
      }
      else
      {
        if (string.IsNullOrWhiteSpace(exercise.Name)) // This means that the exercise is being added
        {
          Storage.CurrentWorkout.Exercises.Add(resultExercise);
          Snackbar.Add($"Successfully added {resultExercise.Name} to current workout!", Severity.Success);
        }
        else // This means that the exercise is being modified
        {
          Exercise? currentExercise = Storage.CurrentWorkout.Exercises.Where(x => x.Name.Equals(exercise.Name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
          if (currentExercise == null)
            return;
          int indexExercise = Storage.CurrentWorkout.Exercises.IndexOf(currentExercise);
          Storage.CurrentWorkout.Exercises.Remove(currentExercise);
          Storage.CurrentWorkout.Exercises.Insert(indexExercise, resultExercise);
          Snackbar.Add($"Successfully modified {exercise.Name}!", Severity.Success);
        }
      }
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

      if (exercise.MinReps != previousExercise.MinReps || exercise.Weight != previousExercise.Weight)
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
      IsExpandSessionOptions = false;
    }

    private void ExpandSessionOptions() => IsExpandSessionOptions = !IsExpandSessionOptions;

    private async Task AddOneRepToAllInCurrentWorkout()
    {
      if (!await PromptConfirmation($"Would you like to +1 the Minimum and Maximum Reps of ALL Exercises in the Current Workout?"))
        return;

      foreach (Exercise exercise in Storage.CurrentWorkout.Exercises)
      {
        exercise.MinReps++;
        exercise.MaxReps++;
      }
      IsExpandSessionOptions = false;
      Snackbar.Add($"Successfully +1 ALL Exercise in the Current Workout! Keep at it!", Severity.Success);
    }
  }
}
