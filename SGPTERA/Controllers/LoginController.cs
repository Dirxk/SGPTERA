using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TeracromController;
using TeracromDatabase;
using TeracromModels;
using GestionProyectos.Models;
using System.Text.Json;

namespace SGPTERA.Controllers
{
    public class LoginController : AuthController 
    {
        private readonly DapperContext _dapperContext;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LoginController(DapperContext dapperContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor, dapperContext)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _dapperContext = dapperContext;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<RespuestaJson> VerificarUsuario(string email, string password, string user2)
        {
            RespuestaJson respuestaUser = new RespuestaJson();
            try
            {
                respuestaUser = await new Account(_dapperContext).VerificarUsuario(email, password, user2);
                //object usuario = respuestaUser.Data;
                Usuarios usuarios = (Usuarios)respuestaUser.Data;
                HttpContext.Session.SetString("IdUsuario", usuarios.Id.ToString());
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
