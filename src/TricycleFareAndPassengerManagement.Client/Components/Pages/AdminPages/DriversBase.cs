using Microsoft.AspNetCore.Components;
using MudBlazor;
using TricycleFareAndPassengerManagement.Application.Common.DTOs;
using TricycleFareAndPassengerManagement.Application.Features.Driver.Commands;
using TricycleFareAndPassengerManagement.Client.Common.Validators.Driver;
using TricycleFareAndPassengerManagement.Client.Components.Dialogs;
using TricycleFareAndPassengerManagement.Client.Services.Interfaces;

namespace TricycleFareAndPassengerManagement.Client.Components.Pages.AdminPages
{
    public class DriversBase : ComponentBase
    {
        #region Fields

        protected List<DriverDto> drivers = new();
        protected bool isLoading = false;
        protected bool isEdit = false;
        protected CreateDriverCommand passengerModel = new();
        protected UpdateDriverCommand updateDriverModel = new();
        protected CreateDriverValidator createValidator = new();
        protected UpdateDriverValidator updateValidator = new();
        [Inject] protected IDriverClientService DriverService { get; set; } = default!;
        [Inject] protected IDialogService DialogService { get; set; } = default!;
        [Inject] protected ISnackbar Snackbar { get; set; } = default!;

        #endregion Fields

        #region Properties

        protected Func<object, string, Task<IEnumerable<string>>> validationFunc => async (model, propertyName) =>
        {
            if (isEdit)
            {
                var result = await updateValidator.ValidateAsync(
                    FluentValidation.ValidationContext<UpdateDriverCommand>.CreateWithOptions(
                        (UpdateDriverCommand) model, x => x.IncludeProperties(propertyName)));
                return result.IsValid ? Array.Empty<string>() : result.Errors.Select(e => e.ErrorMessage);
            }
            else
            {
                var result = await createValidator.ValidateAsync(
                    FluentValidation.ValidationContext<CreateDriverCommand>.CreateWithOptions(
                        (CreateDriverCommand) model, x => x.IncludeProperties(propertyName)));
                return result.IsValid ? Array.Empty<string>() : result.Errors.Select(e => e.ErrorMessage);
            }
        };

        #endregion Properties

        #region Protected Methods

        protected override async Task OnInitializedAsync()
        {
            await LoadDrivers();
            isLoading = false;
        }

        #endregion Protected Methods

        #region Private Methods

        protected async Task LoadDrivers()
        {
            isLoading = true;
            try
            {
                drivers = (await DriverService.GetAllDriversAsync()).ToList();
                await InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error loading drivers: {ex.Message}", Severity.Error);
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
            { "CreateModel", new CreateDriverCommand() },
            { "IsEditing", false },
            { "ValidationFunc", validationFunc }
        };

            var options = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true };
            var dialog = await DialogService.ShowAsync<DriversDialog>("Add Driver", parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled && result.Data is CreateDriverCommand driverData)
            {
                try
                {
                    await DriverService.CreateDriverAsync(driverData);
                    Snackbar.Add("Driver created successfully", Severity.Success);
                    await LoadDrivers();
                    await InvokeAsync(StateHasChanged);
                }
                catch (Exception ex)
                {
                    Snackbar.Add($"Error creating driver: {ex.Message}", Severity.Error);
                }
            }
        }

        protected async void OpenEditDialog(DriverDto driver)
        {
            var createModel = new CreateDriverCommand
            {
                FullName = driver.FullName,
                LicenseNumber = driver.LicenseNumber,
                PhoneNumber = driver.PhoneNumber,
                TricycleNumber = driver.TricycleNumber
            };

            var updateModel = new UpdateDriverCommand
            {
                Id = driver.Id,
                FullName = driver.FullName,
                LicenseNumber = driver.LicenseNumber,
                PhoneNumber = driver.PhoneNumber,
                TricycleNumber = driver.TricycleNumber,
                IsActive = driver.IsActive
            };

            var parameters = new DialogParameters
        {
            { "CreateModel", createModel },
            { "UpdateModel", updateModel },
            { "IsEditing", true },
            { "DriverId", driver.Id }
        };

            var options = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true };
            var dialog = await DialogService.ShowAsync<DriversDialog>("Edit Driver", parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled && result.Data is UpdateDriverCommand updateData)
            {
                try
                {
                    await DriverService.UpdateDriverAsync(driver.Id, updateData);
                    Snackbar.Add("Driver updated successfully", Severity.Success);
                    await LoadDrivers();
                    await InvokeAsync(StateHasChanged);
                }
                catch (Exception ex)
                {
                    Snackbar.Add($"Error updating driver: {ex.Message}", Severity.Error);
                }
            }
        }

        #endregion Private Methods
    }
}