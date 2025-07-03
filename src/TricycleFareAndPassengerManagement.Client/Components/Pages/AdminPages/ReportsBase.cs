using Microsoft.AspNetCore.Components;
using MudBlazor;
using TricycleFareAndPassengerManagement.Application.Common.DTOs;
using TricycleFareAndPassengerManagement.Client.Services.Interfaces;

namespace TricycleFareAndPassengerManagement.Client.Components.Pages.AdminPages
{
    public class ReportsBase : ComponentBase
    {
        #region Fields

        protected List<DailyReportDto> reports = new();
        protected bool isLoading = false;

        #endregion Fields

        #region Properties

        [Inject] protected IReportClientService ReportService { get; set; } = default!;
        [Inject] protected ISnackbar Snackbar { get; set; } = default!;

        #endregion Properties

        #region Protected Methods

        protected override async Task OnInitializedAsync()
        {
            await LoadReports();
        }

        #endregion Protected Methods

        #region Private Methods

        private async Task LoadReports()
        {
            isLoading = true;
            try
            {
                var report = await ReportService.GetDailyReportAsync(DateTime.Today);
                reports = new List<DailyReportDto> { report };
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Failed to load reports: {ex.Message}", Severity.Error);
            }
            finally
            {
                isLoading = false;
                await InvokeAsync(StateHasChanged);
            }
        }

        #endregion Private Methods
    }
}