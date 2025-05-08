using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Fitness.Pages
{
  public partial class ConfirmationDialog
  {
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; }

    [Parameter]
    public string ConfirmationText { get; set; } = string.Empty;

    private void Submit() => MudDialog.Close(DialogResult.Ok(true));

    private void Cancel() => MudDialog.Cancel();
  }
}
