using System.ComponentModel.DataAnnotations;
using System.Net;
using Google;
using Google.Apis.Auth.OAuth2.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace file_management.api.Filters
{
    public class HttpGlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<HttpGlobalExceptionFilter> _logger;

        public HttpGlobalExceptionFilter(ILogger<HttpGlobalExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "Exception occurred: {Message}", context.Exception.Message);

            if (context.Exception is GoogleApiException googleApiException)
            {
                HandleGoogleApiException(context, googleApiException);
            }
            else
            {
                switch (context.Exception)
                {
                    case ValidationException:
                        HandleValidationException(context);
                        break;

                    case UnauthorizedAccessException:
                        HandleUnauthorizedAccessException(context);
                        break;
                    
                    case FileNotFoundException:
                        HandleFileNotFoundException(context);
                        break;

                    default:
                        HandleGeneralException(context);
                        break;
                }
            }

            context.ExceptionHandled = true;
        }

        private static void HandleGoogleApiException(ExceptionContext context, GoogleApiException exception)
        {
            var problemDetails = new ProblemDetails
            {
                Instance = context.HttpContext.Request.Path,
                Status = StatusCodes.Status500InternalServerError,
                Title = "Google API Error",
                Detail = "An error occurred while interacting with the Google API."
            };

            if (exception.InnerException is TokenResponseException)
            {
                problemDetails.Detail = "Authentication token is invalid or expired.";
            }

            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Result = new ObjectResult(problemDetails);
        }

        private static void HandleValidationException(ExceptionContext context)
        {
            var problemDetails = new ValidationProblemDetails
            {
                Instance = context.HttpContext.Request.Path,
                Status = StatusCodes.Status400BadRequest,
                Detail = "Please refer to the errors property for additional details."
            };

            var exceptionType = context.Exception.GetType().Name;
            problemDetails.Errors.Add($"{exceptionType}", new[] { context.Exception.Message });

            context.Result = new BadRequestObjectResult(problemDetails);
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        }

        private static void HandleUnauthorizedAccessException(ExceptionContext context)
        {
            var problemDetails = new ProblemDetails
            {
                Instance = context.HttpContext.Request.Path,
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = "You are not authorized to access this resource."
            };

            context.Result = new UnauthorizedObjectResult(problemDetails);
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        }
        
        private static void HandleFileNotFoundException(ExceptionContext context)
        {
            var problemDetails = new ProblemDetails
            {
                Instance = context.HttpContext.Request.Path,
                Status = StatusCodes.Status401Unauthorized,
                Title = "Not Found",
                Detail = "File not found."
            };

            context.Result = new UnauthorizedObjectResult(problemDetails);
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
        }

        private static void HandleGeneralException(ExceptionContext context)
        {
            GenerateInternalServerErrorContext(context, "An unexpected error occurred. Please refer to the errors property for additional details.");
        }

        private static void GenerateInternalServerErrorContext(ExceptionContext context, string message)
        {
            var problemDetails = new ProblemDetails
            {
                Instance = context.HttpContext.Request.Path,
                Status = StatusCodes.Status500InternalServerError,
                Detail = message
            };
            
            var exceptionType = context.Exception.GetType().Name;
            problemDetails.Extensions.Add("exceptionType", exceptionType);
            problemDetails.Extensions.Add("exceptionMessage", context.Exception.Message);

            context.Result = new InternalServerErrorObjectResult(problemDetails);
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }

        private class InternalServerErrorObjectResult : ObjectResult
        {
            public InternalServerErrorObjectResult(object error)
                : base(error)
            {
                StatusCode = StatusCodes.Status500InternalServerError;
            }
        }
    }
}
