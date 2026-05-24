using GymFlow.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GymFlow.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class ApiKeyAuthAttribute : Attribute, IAsyncActionFilter
    {
        private const string ApiUserHeader = "X-Api-Username";
        private const string ApiKeyHeader = "X-Api-Key";

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpContext = context.HttpContext;
            var request = httpContext.Request;

            var username = request.Headers[ApiUserHeader].FirstOrDefault()
                ?? request.Query["username"].FirstOrDefault();
            var apiKey = request.Headers[ApiKeyHeader].FirstOrDefault()
                ?? request.Query["apikey"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(apiKey))
            {
                context.Result = new JsonResult(new { error = "Missing API credentials." })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }

            var dbContext = httpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
            var user = await dbContext.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.ApiKey == apiKey && u.IsActive);

            if (user == null)
            {
                context.Result = new JsonResult(new { error = "Invalid API credentials." })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }

            httpContext.Items[ApiUserIdKey] = user.Id;
            httpContext.Items[ApiUsernameKey] = user.Username;
            httpContext.Items[ApiUserRoleKey] = user.Role.ToString();

            await next();
        }

        private const string ApiUserIdKey = "ApiUserId";
        private const string ApiUsernameKey = "ApiUsername";
        private const string ApiUserRoleKey = "ApiUserRole";
    }
}
