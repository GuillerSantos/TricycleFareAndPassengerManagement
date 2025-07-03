using Microsoft.AspNetCore.Components;
using MudBlazor;
using TricycleFareAndPassengerManagement.Application.Common.DTOs;
using TricycleFareAndPassengerManagement.Application.Features.Passengers.Commands;
using TricycleFareAndPassengerManagement.Client.Common.Validators.Passenger;
using TricycleFareAndPassengerManagement.Client.Components.Dialogs;
using TricycleFareAndPassengerManagement.Client.Services.Interfaces;

namespace TricycleFareAndPassengerManagement.Client.Components.Pages.AdminPages
{
    public class PassengersBase : ComponentBase
    {
        #region Fields

        protected List<PassengerDto> passengers = new();
        protected List<PassengerDto> filteredPassengers = new();
        protected string searchString = "";
        protected bool isLoading = false;
        protected bool showDialog = false;
        protected bool isEdit = false;
        protected CreatePassengerCommand passengerModel = new();
        protected UpdatePassengerCommand updatePassengerModel = new();
        protected CreatePassengerValidator createValidator = new();
        protected UpdatePassengerValidator updateValidator = new();

        #endregion Fields

        #region Properties

        [Inject] protected IPassengerClientService TricycleService { get; set; } = default!;
        [Inject] protected IDialogService DialogService { get; set; } = default!;
        [Inject] protected ISnackbar Snackbar { get; set; } = default!;

        protected Func<object, string, Task<IEnumerable<string>>> validationFunc => async (model, propertyName) =>
        {
            if (isEdit)
            {
                var result = await updateValidator.ValidateAsync(
                    FluentValidation.ValidationContext<UpdatePassengerCommand>.CreateWithOptions(
                        (UpdatePassengerCommand) model, x => x.IncludeProperties(propertyName)));
                return result.IsValid ? Array.Empty<string>() : result.Errors.Select(e => e.ErrorMessage);
            }
            else
            {
                var result = await createValidator.ValidateAsync(
                    FluentValidation.ValidationContext<CreatePassengerCommand>.CreateWithOptions(
                        (CreatePassengerCommand) model, x => x.IncludeProperties(propertyName)));
                return result.IsValid ? Array.Empty<string>() : result.Errors.Select(e => e.ErrorMessage);
            }
        };

        #endregion Properties

        #region Protected Methods

        protected override async Task OnInitializedAsync()
        {
            await LoadPassengers();
            isLoading = false;
        }

        protected async Task LoadPassengers()
        {
            isLoading = true;
            try
            {
                passengers = (await TricycleService.GetAllPassengersAsync()).ToList();
                FilterPassengers();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error loading passengers: {ex.Message}", Severity.Error);
            }
            finally
            {
                isLoading = false;
            }
        }

        protected void FilterPassengers()
        {
            if (string.IsNullOrWhiteSpace(searchString))
            {
                filteredPassengers = passengers;
            }
            else
            {
                filteredPassengers = passengers.Where(p =>
                    p.FullName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    p.PhoneNumber.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
            }
        }

        protected async void OpenCreateDialog()
        {
            var parameters = new DialogParameters
        {
            { "Model", new CreatePassengerCommand() },
            { "IsEdit", false },
            { "ValidationFunc", validationFunc }
        };

            var options = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true };
            var dialog = await DialogService.ShowAsync<PassengerDialog>("Create Passenger", parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled && result.Data is CreatePassengerCommand passengerData)
            {
                try
                {
                    await TricycleService.CreatePassengerAsync(passengerData);
                    Snackbar.Add("Passenger created successfully", Severity.Success);
                    await LoadPassengers();
                    await InvokeAsync(StateHasChanged);
                }
                catch (Exception ex)
                {
                    Snackbar.Add($"Error creating passenger: {ex.Message}", Severity.Error);
                }
            }
        }

        protected async void OpenEditDialog(PassengerDto passenger)
        {
            var createModel = new CreatePassengerCommand
            {
                FullName = passenger.FullName,
                PhoneNumber = passenger.PhoneNumber
            };

            var updateModel = new UpdatePassengerCommand
            {
                Id = passenger.Id,
                FullName = passenger.FullName,
                PhoneNumber = passenger.PhoneNumber,
                IsActive = passenger.IsActive
            };

            var parameters = new DialogParameters
        {
            { "Model", createModel },
            { "UpdateModel", updateModel },
            { "IsEdit", true },
            { "ValidationFunc", validationFunc }
        };

            var options = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true };
            var dialog = await DialogService.ShowAsync<PassengerDialog>("Edit Passenger", parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled && result.Data is UpdatePassengerCommand updatedData)
            {
                try
                {
                    await TricycleService.UpdatePassengerAsync(updatedData.Id, updatedData);
                    Snackbar.Add("Passenger updated successfully", Severity.Success);
                    await LoadPassengers();
                    await InvokeAsync(StateHasChanged);
                }
                catch (Exception ex)
                {
                    Snackbar.Add($"Error updating passenger: {ex.Message}", Severity.Error);
                }
            }
        }

        #endregion Protected Methods
    }
}