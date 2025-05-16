using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Fitness.Pages
{
  public partial class LoadWorkoutDialog
  {
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = default!;

    private enum SessionTypes { PUSH, PULL, SHOULDERS, LEGS };

    private void OnSessionClick(string sessionType) => MudDialog.Close(DialogResult.Ok(sessionType));

    private void Cancel() => MudDialog.Cancel();
  }
}
