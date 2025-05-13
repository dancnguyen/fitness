using Fitness.Pages;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Fitness.Layout
{
  public partial class MainLayout
  {
    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private MudTheme Theme = new()
    {
      PaletteLight = new()
      {
        Primary = "#8B0000",
      }
    };

    private async Task ShowAppManagement()
    {
      DialogOptions options = new DialogOptions { MaxWidth = MaxWidth.ExtraSmall };
      IDialogReference appManagementDialog = await DialogService.ShowAsync<AppManagementDialog>("Confirm", options);
      DialogResult? appManagementDialogResult = await appManagementDialog.Result;
      if (appManagementDialogResult == null || appManagementDialogResult.Canceled)
        return;
    }
  }
}
