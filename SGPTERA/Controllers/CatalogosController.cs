using Microsoft.AspNetCore.Mvc;
using TeracromDatabase;

namespace SGPTERA.Controllers
{
    public class CatalogosController : AuthController
    {

        private readonly DapperContext _dapperContext;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CatalogosController(DapperContext dapperContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor, dapperContext)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _dapperContext = dapperContext;
        }

        public  IActionResult Clientes()
        {
            ViewBag.Usuario = GetSessionValue("IdUsuario").ToString() ?? "";
            return View();
        }


    }
}
