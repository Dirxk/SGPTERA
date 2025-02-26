using GestionProyectos.Models;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using TeracromController;
using TeracromDatabase;
using TeracromModels;

namespace SGPTERA.Controllers
{

    public class GestionProyectosController : Controller
    {
        private readonly DapperContext _dapperContext;
        

        public GestionProyectosController(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        public IActionResult Index()
        {
            return View();
        }


    }
}
