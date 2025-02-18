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

        //[HttpPost]
        //public async Task<RespuestaJson> RegistrarUsuario([FromForm] Usuario usuario, IFormFile foto)
        //{
        //    return await new Vehiculos(_databaseService).RegistrarUsuario(usuario, foto);
        //}

        

        [HttpPost]
        public async Task<RespuestaJson> VerificarUsuario(string email, string password, string user2)
        {
            RespuestaJson respuestaUser = new RespuestaJson();
            try
            {
                respuestaUser = await new Account(_dapperContext).VerificarUsuario(email, password, user2);
            }
            catch (Exception ex)
            {
                respuestaUser.Resultado = false;
                respuestaUser.Mensaje = "Ocurrió un error inesperado: " + ex.Message;
                respuestaUser.Errores.Add(ex.Message); // Añadimos el error a la lista
            }

            return respuestaUser;
        }

        [HttpPost]
        public async Task<RespuestaJson> RegistrarUsuario(Usuarios User)
        {
            return await new Account(_dapperContext).RegistrarUsuario(User);
        }
    }
}
