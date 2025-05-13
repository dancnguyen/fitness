using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Fitness.Pages
{
  public partial class ShowTextDialog
  {
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = default!;

    [Parameter]
    public string ShowText { get; set; } = string.Empty;

    private void Ok() => MudDialog.Close(DialogResult.Ok(true));
  }
}
