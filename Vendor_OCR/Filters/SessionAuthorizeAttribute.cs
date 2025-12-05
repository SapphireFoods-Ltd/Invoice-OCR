using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Vendor_OCR.Filters
{
    public class SessionAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly string _requiredUserType;

        public SessionAuthorizeAttribute(string requiredUserType)
        {
            _requiredUserType = requiredUserType;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userType = context.HttpContext.Session.GetString("user_type");

            if (string.IsNullOrEmpty(userType) || userType != _requiredUserType)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
            }
        }
    }
}
