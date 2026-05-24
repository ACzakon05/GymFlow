using GymFlow.Filters;
using Microsoft.AspNetCore.Mvc;

namespace GymFlow.Controllers.Api
{
    [ApiController]
    [ApiKeyAuth]
    public abstract class ApiBaseController : ControllerBase
    {
        private const string ApiUserIdKey = "ApiUserId";
        private const string ApiUsernameKey = "ApiUsername";
        private const string ApiUserRoleKey = "ApiUserRole";

        protected int ApiUserId => HttpContext != null && HttpContext.Items[ApiUserIdKey] is int id ? id : 0;
        protected string ApiUsername => HttpContext != null && HttpContext.Items[ApiUsernameKey] is string name ? name : string.Empty;
        protected string ApiUserRole => HttpContext != null && HttpContext.Items[ApiUserRoleKey] is string role ? role : string.Empty;
    }
}
