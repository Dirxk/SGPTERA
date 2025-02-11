using GestionProyectos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeracromController;

namespace SGPTERA.Controllers
{
    public class AuthController : Controller
    {
        private readonly IConfiguration _configuration;
        public List<string> UrlGet = new List<string>
        {
        "ControlPersonal-Index"
        };
        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public override async void OnActionExecuting(ActionExecutingContext filterContext)
        {
            ViewBag.controllerName = ControllerContext.RouteData.Values["controller"] as string ?? "";
            ViewBag.actionName = ControllerContext.RouteData.Values["action"] as string ?? "";
            ViewBag.fullNamePage = $"{ViewBag.controllerName}-{ViewBag.actionName}";

            //if (filterContext.HttpContext.Request.Method == HttpMethods.Get && UrlGet.Contains(ViewBag.fullNamePage as string ?? ""))
            //{
            //    //=== Recuperamos los datos del empleado ===\\
            //    string[] UserPC = User.Identity?.Name.ToString().Split('\\');
            //    string user = UserPC.Length == 2 ? UserPC[1] : "";

            //    if (user == "Teracrom 05" || user == "hp" || user.Contains("teracrom"))
            //    {
            //        user = "rdelgado";
            //    }

            //    HttpContext.Session.SetString("RHs_UserPC", user);

            //    if (HttpContext.Session.GetString("RHs_Gid") == null || HttpContext.Session.GetString("RHs_Gid")?.ToString() == "")
            //    {
            //        Usuarios usuarios = await new Account(_configuration).GetUsuarios(user);
            //        HttpContext.Session.SetString("RHs_Empleado", usuarios.NombreUsuario.ToString());
            //        HttpContext.Session.SetString("RHs_Gid", usuarios.Gid.Gid);
            //        HttpContext.Session.SetString("RHs_NombreCompleto", $"{usuarios.Nombre} {usuarios.ApellidoPaterno} {usuarios.ApellidoMaterno}");
            //        HttpContext.Session.SetString("RHs_CentroCosto", usuarios.CentroCosto.CentroCosto);
            //    }

            //    //=== Validamos si tiene los permisos del FP2 ===\\
            //    RespuestaJson respuesta = new RespuestaJson();
            //    if (respuesta.Resultado)
            //    {
            //        filterContext.Result = new RedirectToRouteResult(
            //            new RouteValueDictionary {
            //            { "controller", "ControlPersonal" },
            //            { "action", "Error" },
            //            { "error", "No cuentas con los permisos para acceder a esta aplicación, avisa a IT para que te los proporcione." },
            //            { "urlRedirigir", $"{Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}" }
            //            });
            //    }
            //}

            base.OnActionExecuting(filterContext);
        }

        public object GetSessionValue()
        {
            return new { };
        }
    }
}
