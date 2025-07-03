using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using TricycleFareAndPassengerManagement.Client.Models;
using TricycleFareAndPassengerManagement.Client.Security;
using TricycleFareAndPassengerManagement.Client.Services.Interfaces;
using TricycleFareAndPassengerManagement.Client.Shared.Constants;

namespace TricycleFareAndPassengerManagement.Client.Components.Pages.AuthPages
{
    public class LoginBase : ComponentBase
    {
        #region Fields

        protected LoginModel loginModel = new();
        protected AuthResult authResult = new();
        protected bool isLoading = false;
        protected bool _hasRendered = false;

        #endregion Fields

        #region Properties

        [Inject] protected CustomAuthStateProvider CustomAuthStateProvider { get; set; } = default!;
        [Inject] protected NavigationManager Navigation { get; set; } = default!;
        [Inject] protected IAuthService AuthService { get; set; } = default!;
        [Inject] protected ISnackbar Snackbar { get; set; } = default!;
        [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;

        #endregion Properties

        #region Protected Methods

        protected async Task HandleLogin()
        {
            isLoading = true;
            await InvokeAsync(StateHasChanged);

            try
            {
                var result = await AuthService.LoginAsync(loginModel);

                if (result?.Success == true && result.User != null)
                {
                    // Navigate without forceLoad to avoid server-side rendering issues
                    var role = result.User.Roles?.FirstOrDefault() ?? string.Empty;

                    if (role == RoleConstants.Admin)
                    {
                        Navigation.NavigateTo("/admindashboard");
                    }
                    else if (role == RoleConstants.DefaultUser)
                    {
                        Navigation.NavigateTo("/userdashboard");
                    }
                    else
                    {
                        Snackbar.Add($"Unrecognized role: {role}", Severity.Warning);
                        Navigation.NavigateTo("/");
                    }
                }
                else
                {
                    var errorMessage = result?.Errors?.FirstOrDefault() ?? "Login failed";
                    Snackbar.Add(errorMessage, Severity.Error);
                }
            }
            catch (Exception ex)
            {
                await JSRuntime.InvokeVoidAsync("console.error", $"Login exception: {ex}");
                Snackbar.Add($"An error occurred during login: {ex.Message}", Severity.Error);
            }
            finally
            {
                isLoading = false;
                await InvokeAsync(StateHasChanged);
            }
        }

        #endregion Protected Methods
    }
}