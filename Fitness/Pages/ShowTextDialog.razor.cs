using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace Fitness.Pages
{
  public partial class ShowTextDialog
  {
    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = default!;

    [Parameter]
    public string ShowText { get; set; } = string.Empty;

    private async Task CopyToClipboard()
    {
      bool isCopied = await JSRuntime.InvokeAsync<bool>("copyToClipboard", ShowText);
      if (isCopied)
        Snackbar.Add("Successfully copied text to clipboard!", Severity.Success);
      else
        Snackbar.Add("Unable to copy text to clipboard.", Severity.Error);
    }

    private void Ok() => MudDialog.Close(DialogResult.Ok(true));
  }
}
