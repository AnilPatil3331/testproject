using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace BVG_Wellness_Center_Promotion.Filters // तुमच्या प्रोजेक्टचे नाव इथे वापरा
{
    public class AuthorizeUserAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // १. सेशन चेक करा
            if (filterContext.HttpContext.Session["UserID"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary {
                        { "Controller", "Home" },
                        { "Action", "Login" },
                        { "area", "" }
                    });
            }

            // २. बॅक बटण काम करू नये म्हणून कॅशे सेट करा
            filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            filterContext.HttpContext.Response.Cache.SetNoStore();
            filterContext.HttpContext.Response.Cache.SetExpires(System.DateTime.UtcNow.AddHours(-1));

            base.OnActionExecuting(filterContext);
        }
    }
}