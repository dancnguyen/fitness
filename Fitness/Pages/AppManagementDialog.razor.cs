using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Fitness.Pages
{
  public partial class AppManagementDialog
  {
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = default!;

    private string InfoText
    {
      get
      {
        return "Coming soon! Looking to add a means to delete all of the app's browser data.";
      }
    }

    private void Close() => MudDialog.Cancel();
  }
}
