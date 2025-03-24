using GestionProyectos.Models;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeracromController;
using TeracromDatabase;

namespace SGPTERA.Controllers
{
    public class AuthController : Controller
    {
        private readonly DapperContext _dapperContext;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
       
        public List<string> UrlGet = new List<string>
        {
        "Home-Index"
        };
        public AuthController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }
        public override async void OnActionExecuting(ActionExecutingContext filterContext)
        {
            ViewBag.controllerName = ControllerContext.RouteData.Values["controller"] as string ?? "";
            ViewBag.actionName = ControllerContext.RouteData.Values["action"] as string ?? "";
            ViewBag.fullNamePage = $"{ViewBag.controllerName}-{ViewBag.actionName}";

            if (filterContext.HttpContext.Request.Method == HttpMethods.Get && UrlGet.Contains(ViewBag.fullNamePage as string ?? "") && GetSessionValue("IdUsuario") == null)
            {
                var Login = Url.Action("Login", "Login");
                filterContext.HttpContext.Response.Redirect(Login);
            }
            base.OnActionExecuting(filterContext);
        }

        public object GetSessionValue(string Key)
        {
            object value = _httpContextAccessor.HttpContext?.Session.GetString(Key) ?? null;
            return value;
        }
    }
}
