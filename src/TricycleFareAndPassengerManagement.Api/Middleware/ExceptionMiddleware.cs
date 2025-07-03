using Azure;
using System.Net;
using System.Text.Json;
using TricycleFareAndPassengerManagement.Application.Common.Responses;
using Response = TricycleFareAndPassengerManagement.Application.Common.Responses.Response;

namespace TricycleFareAndPassengerManagement.Api.Middleware
{
    public class ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
    {
        #region Public Methods

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An Unhandled Exception Occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static int GetStatusCodeFromExceptionm(Exception exception) => exception switch
        {
            ArgumentException or ArgumentNullException => (int) HttpStatusCode.BadRequest,
            KeyNotFoundException => (int) HttpStatusCode.NotFound,
            UnauthorizedAccessException => (int) HttpStatusCode.Unauthorized,
            InvalidOperationException => (int) HttpStatusCode.BadRequest,
            _ => (int) HttpStatusCode.InternalServerError
        };

        private static string GetErrorMessage(Exception exception, int statusCode)
        {
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIROMENT") == "Development")
                return exception.Message;

            return statusCode switch
            {
                (int) HttpStatusCode.BadRequest => "The Request Was Invalid Or Cannot Be Saved.",
                (int) HttpStatusCode.NotFound => "The Requested Resources Could Not Be Found",
                (int) HttpStatusCode.Unauthorized => "Authentication Is Required Or Has Failed.",
                _ => "An Unexpected Error Occurred. Please Try Again Later."
            };
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            context.Response.StatusCode = GetStatusCodeFromExceptionm(exception);

            var errorResponse = new Response
            {
                Success = false,
                ErrorMessage = GetErrorMessage(exception, context.Response.StatusCode),
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var result = JsonSerializer.Serialize(errorResponse, options);
            await context.Response.WriteAsync(result);
        }

        #endregion Public Methods
    }
}