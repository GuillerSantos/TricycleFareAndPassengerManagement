using Microsoft.AspNetCore.Components;
using MudBlazor;
using TricycleFareAndPassengerManagement.Application.Common.DTOs;
using TricycleFareAndPassengerManagement.Application.Features.Trips.Commands;
using TricycleFareAndPassengerManagement.Client.Common.Validators.Trip;
using TricycleFareAndPassengerManagement.Client.Components.Dialogs;
using TricycleFareAndPassengerManagement.Client.Services.Interfaces;

namespace TricycleFareAndPassengerManagement.Client.Components.Pages.AdminPages
{
    public class TripsBase : ComponentBase
    {
        #region Properties

        [Inject] protected ITripClientService Trip { get; set; } = default!;
        [Inject] protected IDriverClientService Driver { get; set; } = default!;
        [Inject] protected IPassengerClientService Passenger { get; set; } = default!;
        [Inject] protected IFareClientService Fare { get; set; } = default!;
        [Inject] protected IDialogService DialogService { get; set; } = default!;
        [Inject] protected ISnackbar Snackbar { get; set; } = default!;

        #endregion Properties

        #region Fields

        protected List<TripDto> trips = new();
        protected List<DriverDto> drivers = new();
        protected List<PassengerDto> passengers = new();
        protected bool isLoading = false;
        protected bool saving = false;
        protected bool showDialog = false;
        protected bool isEditing = false;
        protected FareCalculationDto? fareCalculation;
        protected CreateTripCommand tripModel = new();
        protected MudForm? form;
        protected CreateTripValidator validator = new();

        protected DialogOptions dialogOptions = new() { MaxWidth = MaxWidth.Large, FullWidth = true };

        #endregion Fields

        protected Func<object, string, Task<IEnumerable<string>>> validationFunc => async (model, propertyName) =>
        {
            var result = await validator.ValidateAsync(FluentValidation.ValidationContext<CreateTripCommand>
                .CreateWithOptions((CreateTripCommand) model, x => x.IncludeProperties(propertyName)));
            return result.IsValid ? Array.Empty<string>() : result.Errors.Select(e => e.ErrorMessage);
        };

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
            isLoading = false;
        }

        protected async Task LoadData()
        {
            isLoading = true;
            try
            {
                var tripTask = Trip.GetAllTripsAsync();
                var driverTask = Driver.GetAllDriversAsync();
                var passengerTask = Passenger.GetAllPassengersAsync();

                await Task.WhenAll(tripTask, driverTask, passengerTask);
                await InvokeAsync(StateHasChanged);

                trips = tripTask.Result;
                drivers = driverTask.Result.Where(d => d.IsActive).ToList();
                passengers = passengerTask.Result.Where(p => p.IsActive).ToList();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error loading data: {ex.Message}", Severity.Error);
            }
            finally
            {
                isLoading = false;
            }
        }

        protected async void OpenCreateDialog()
        {
            var parameters = new DialogParameters
    {
        { "TripModel", new CreateTripCommand() },
        { "IsEditing", false },
        { "Drivers", drivers },
        { "Passengers", passengers }
    };

            var options = new DialogOptions { MaxWidth = MaxWidth.Large, FullWidth = true };
            var dialog = await DialogService.ShowAsync<TripDialog>("Create Trip", parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                var tripData = (CreateTripCommand) result.Data;
                await Trip.CreateTripAsync(tripData);
                Snackbar.Add("Trip created successfully", Severity.Success);
                await LoadData();
                await InvokeAsync(StateHasChanged);
            }
        }

        protected async void OpenEditDialog(TripDto trip)
        {
            var model = new CreateTripCommand
            {
                DriverId = trip.DriverId,
                PassengerId = trip.PassengerId,
                PickupLocation = trip.PickupLocation,
                DropoffLocation = trip.DropoffLocation,
                Distance = trip.Distance
            };

            var parameters = new DialogParameters
    {
        { "TripModel", model },
        { "IsEditing", true },
        { "Drivers", drivers },
        { "Passengers", passengers }
    };

            var options = new DialogOptions { MaxWidth = MaxWidth.Large, FullWidth = true };
            var dialog = await DialogService.ShowAsync<TripDialog>("Edit Trip", parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                // Implement your update logic here
                Snackbar.Add("Trip updated successfully", Severity.Success);
                await LoadData();
                await InvokeAsync(StateHasChanged);
            }
        }

        protected void CloseDialog()
        {
            showDialog = false;
            tripModel = new CreateTripCommand();
            fareCalculation = null;
            isEditing = false;
        }

        protected async Task OnDistanceChanged()
        {
            if (tripModel.Distance > 0)
            {
                await CalculateFare();
            }
            else
            {
                fareCalculation = null;
                await InvokeAsync(StateHasChanged);
            }
        }

        protected async Task CalculateFare()
        {
            if (tripModel.Distance > 0)
            {
                try
                {
                    fareCalculation = await Fare.CalculateFareAsync(tripModel.Distance);
                }
                catch (Exception ex)
                {
                    Snackbar.Add($"Error calculating fare: {ex.Message}", Severity.Error);
                }
            }
        }

        protected async Task SaveTrip()
        {
            if (form != null)
            {
                await form.Validate();
                if (!form.IsValid)
                    return;
            }

            saving = true;
            try
            {
                if (isEditing)
                {
                    // You'll need to implement UpdateTripAsync method in your service
                    // await Trip.UpdateTripAsync(tripModel);
                    Snackbar.Add("Trip updated successfully", Severity.Success);
                }
                else
                {
                    await Trip.CreateTripAsync(tripModel);
                    Snackbar.Add("Trip created successfully", Severity.Success);
                }

                CloseDialog();
                await LoadData();
                await InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error {(isEditing ? "updating" : "creating")} trip: {ex.Message}", Severity.Error);
            }
            finally
            {
                saving = false;
            }
        }
    }
}