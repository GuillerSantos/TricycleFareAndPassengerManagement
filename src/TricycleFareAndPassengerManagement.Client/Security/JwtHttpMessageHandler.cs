using Blazored.LocalStorage;

namespace TricycleFareAndPassengerManagement.Client.Security
{
    public class JwtHttpMessageHandler(ILocalStorageService _localStorage) : DelegatingHandler
    {
        #region Protected Methods

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (!request.Headers.Contains("bearer"))
            {
                var savedToken = await _localStorage.GetItemAsync<string>("authToken");

                if (!string.IsNullOrWhiteSpace(savedToken))
                {
                    request.Headers.Add("bearer", savedToken);
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }

        #endregion Protected Methods
    }
}