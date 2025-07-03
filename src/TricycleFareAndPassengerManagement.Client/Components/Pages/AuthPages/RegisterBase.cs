using Microsoft.AspNetCore.Components;
using MudBlazor;
using TricycleFareAndPassengerManagement.Client.Models;
using TricycleFareAndPassengerManagement.Client.Services.Interfaces;

namespace TricycleFareAndPassengerManagement.Client.Components.Pages.AuthPages
{
    public class RegisterBase : ComponentBase
    {
        #region Properties

        [Inject] protected IAuthService AuthService { get; set; } = default!;
        [Inject] protected NavigationManager Navigation { get; set; } = default!;
        [Inject] protected ISnackbar Snackbar { get; set; } = default!;

        #endregion Properties

        #region Fields

        protected RegisterModel registerModel = new();
        protected bool _showPassword = false;
        protected bool _showConfirmPassword = false;
        protected bool _isLoading = false;

        #endregion Fields

        #region Protected Methods

        protected override async Task OnInitializedAsync()
        {
            var isAuthenticated = await AuthService.IsAuthenticatedAsync();
            Console.WriteLine($"User authenticated: {isAuthenticated}");
        }

        protected async Task HandleRegister()
        {
            _isLoading = true;
            try
            {
                var result = await AuthService.RegisterAsync(registerModel);

                if (result.Success)
                {
                    Snackbar.Add($"Welcome, {result.User?.FullName}! Your account has been created.", Severity.Success);
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        Snackbar.Add(error, Severity.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"An error occurred: {ex.Message}", Severity.Error);
            }
            finally
            {
                _isLoading = false;
            }
        }

        #endregion Protected Methods
    }
}