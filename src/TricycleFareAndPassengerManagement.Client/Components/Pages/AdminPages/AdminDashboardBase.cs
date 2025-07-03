using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using TricycleFareAndPassengerManagement.Application.Common.DTOs;
using TricycleFareAndPassengerManagement.Client.Services.Interfaces;

namespace TricycleFareAndPassengerManagement.Client.Components.Pages.AdminPages
{
    public class AdminDashboardBase : ComponentBase
    {
        #region Fields

        protected bool isLoading = false;
        protected bool hasRendered = false;

        // Dashboard statistics
        protected int totalDrivers = 0;

        protected int activeDrivers = 0;
        protected int totalPassengers = 0;
        protected int activePassengers = 0;
        protected int totalTrips = 0;
        protected int todayTrips = 0;
        protected decimal todayRevenue = 0;
        protected double revenueGrowth = 0;

        // Data collections
        protected List<TripDto> recentTrips = new();

        protected DailyReportDto? dailyReport;
        protected IJSObjectReference? module;
        protected string? result;

        #endregion Fields

        #region Properties

        [Inject] protected ITripClientService Trip { get; set; } = default!;
        [Inject] protected IDriverClientService Driver { get; set; } = default!;
        [Inject] protected IPassengerClientService Passenger { get; set; } = default!;
        [Inject] protected IReportClientService Report { get; set; } = default!;
        [Inject] protected ISnackbar Snackbar { get; set; } = default!;
        [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;

        #endregion Properties

        #region Protected Methods

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                hasRendered = true;
                await LoadDashboardData();
                StateHasChanged();
            }

            if (firstRender)
            {
                module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./js/map.js");
                if (module != null)
                {
                    result = await module.InvokeAsync<string>("load_map");
                }
            }
        }

        protected async Task LoadDashboardData()
        {
            if (!hasRendered)
                return;

            isLoading = true;
            StateHasChanged();

            try
            {
                // Add a small delay to ensure auth state is fully loaded
                await Task.Delay(100);

                // Load all data concurrently
                var driversTask = Driver.GetAllDriversAsync();
                var passengersTask = Passenger.GetAllPassengersAsync();
                var tripsTask = Trip.GetAllTripsAsync();
                var dailyReportTask = Report.GetDailyReportAsync(DateTime.Today);

                await Task.WhenAll(driversTask, passengersTask, tripsTask, dailyReportTask);

                // Process drivers data
                var drivers = await driversTask;
                totalDrivers = drivers.Count;
                activeDrivers = drivers.Count(d => d.IsActive);

                // Process passengers data
                var passengers = await passengersTask;
                totalPassengers = passengers.Count;
                activePassengers = passengers.Count(p => p.IsActive);

                // Process trips data
                var trips = await tripsTask;
                totalTrips = trips.Count;
                recentTrips = trips.OrderByDescending(t => t.TripDate).ToList();

                // Today's trips
                var today = DateTime.Today;
                var todayTripsList = trips.Where(t => t.TripDate.Date == today).ToList();
                todayTrips = todayTripsList.Count;
                todayRevenue = todayTripsList.Sum(t => t.TotalFare);

                // Calculate revenue growth
                var yesterday = today.AddDays(-1);
                var yesterdayTrips = trips.Where(t => t.TripDate.Date == yesterday).ToList();
                var yesterdayRevenue = yesterdayTrips.Sum(t => t.TotalFare);

                if (yesterdayRevenue > 0)
                {
                    revenueGrowth = (double) (((todayRevenue - yesterdayRevenue) / yesterdayRevenue) * 100);
                }

                // Load daily report
                dailyReport = await dailyReportTask;
            }
            catch (Exception ex)
            {
                await JSRuntime.InvokeVoidAsync("console.error", $"Dashboard error: {ex}");
                Snackbar.Add($"Error loading dashboard data: {ex.Message}", Severity.Error);
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }

        #endregion Protected Methods
    }
}