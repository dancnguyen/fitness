using Fitness.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Fitness.Pages
{
  public partial class DisplayExerciseDialog
  {
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = default!;

    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    [Parameter]
    public Exercise InputExercise { get; set; } = default!;

    [Parameter]
    public List<string> ExerciseNames { get; set; } = default!;

    
    private Exercise Exercise { get; set; } = default!;
    private Exercise UnmodifiedExercise { get; set; } = default!;

    private string DialogTitle
    {
      get
      {
        if (string.IsNullOrWhiteSpace(Exercise.Name))
          return "New Exercise";
        else
          return "Edit Exercise";
      }
    }

    private string ErrorMessage { get; set; } = string.Empty;

    private bool IsExerciseModified { get; set; } = false;

    private MudForm Form { get; set; } = default!;

    protected override void OnInitialized()
    {
      Exercise = new()
      {
        Name = InputExercise.Name,
        Weight = InputExercise.Weight,
        Measurement = InputExercise.Measurement,
        MinReps = InputExercise.MinReps,
        MaxReps = InputExercise.MaxReps,
        Sets = InputExercise.Sets,
      };
      UnmodifiedExercise = new()
      {
        Name = Exercise.Name,
        Weight = Exercise.Weight,
        Measurement = Exercise.Measurement,
        MinReps = Exercise.MinReps,
        MaxReps = Exercise.MaxReps,
        Sets = Exercise.Sets,
      };
    }

    private void OnNameKeyUp(KeyboardEventArgs e) => IsExerciseModified = true;

    private bool CheckExerciseIsModified()
    {
      if (UnmodifiedExercise == null)
        return false;
      else if (!UnmodifiedExercise.Name.Equals(Exercise.Name))
        return true;
      else if (UnmodifiedExercise.Weight != Exercise.Weight)
        return true;
      else if (!UnmodifiedExercise.Measurement.ToString().Equals(Exercise.Measurement.ToString()))
        return true;
      else if (UnmodifiedExercise.MinReps != Exercise.MinReps)
        return true;
      else if (UnmodifiedExercise.MaxReps != Exercise.MaxReps)
        return true;
      else if (UnmodifiedExercise.Sets != Exercise.Sets)
        return true;
      else
        return false;
    }

    private async Task DeleteExercise()
    {
      if (!await PromptConfirmation("Are you sure you would like to delete this exercise from your workout session?"))
        return;
      MudDialog.Close(DialogResult.Ok(new Exercise()));
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

    private async Task Submit()
    {
      await Form.Validate();
      if (!Form.IsValid)
        return;

      if (!UnmodifiedExercise.Name.Equals(Exercise.Name) && ExerciseNames.Where(x => x.Equals(Exercise.Name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault() != null)
      {
        string error = "Another exercise with the same name cannot be added!";
        ErrorMessage = error;
        Snackbar.Add(error, Severity.Error);
        return;
      }

      MudDialog.Close(DialogResult.Ok(Exercise));
    }


    private void Cancel() => MudDialog.Cancel();
  }
}
