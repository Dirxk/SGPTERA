using GestionProyectos.Models;
using Microsoft.AspNetCore.Mvc;
using TeracromController;
using TeracromDatabase;
using TeracromModels;

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

        //============================================================================================================================\\
        //=================================================   Vistas de los Catálogos  =============================================== \\
        //==============================================================================================================================\\

        public IActionResult Clientes()
        {
            ViewBag.Usuario = GetSessionValue("IdUsuario").ToString() ?? "";
            return View();
        }

        public IActionResult Puestos()
        {
            ViewBag.Usuario = GetSessionValue("IdUsuario").ToString() ?? "";
            return View();
        }

        //============================================================================================================================\\
        //================================================   Controlador de Clientes   =============================================== \\
        //==============================================================================================================================\\

        [HttpPost]
        public async Task<RespuestaJson> GetClientes()
        {
            RespuestaJson respuesta = await new Catalogos(_dapperContext).GetClientes();
            return respuesta;
        }

        [HttpPost]
        public async Task<RespuestaJson> AgregarCliente(Clientes cliente)
        {
            cliente.IdUsuarioSet =  int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).AgregarCliente(cliente);

        }

        [HttpPost]
        public async Task<RespuestaJson> EditarCliente(Clientes cliente)
        {
            cliente.IdUsuarioUpd = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).EditarCliente(cliente);

        }

        [HttpPost]
        public async Task<RespuestaJson> DesactivarCliente(Clientes cliente)
        {
            cliente.IdUsuarioDel = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).DesactivarCliente(cliente);

        }

        [HttpPost]
        public async Task<RespuestaJson> ReactivarCliente(Clientes cliente)
        {
            cliente.IdUsuarioUpd = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).ReactivarCliente(cliente);

        }

        //============================================================================================================================\\
        //=================================================   Controlador de Puestos   =============================================== \\
        //==============================================================================================================================\\

        [HttpPost]
        public async Task<RespuestaJson> GetPuestos()
        {
            RespuestaJson respuesta = await new Catalogos(_dapperContext).GetPuestos();
            return respuesta;
        }

        [HttpPost]
        public async Task<RespuestaJson> AgregarPuesto(Puestos puesto)
        {
            puesto.IdUsuarioSet = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).AgregarPuesto(puesto);
        }

        [HttpPost]
        public async Task<RespuestaJson> EditarPuesto(Puestos puesto)
        {
            puesto.IdUsuarioUpd = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).EditarPuesto(puesto);
        }

        [HttpPost]
        public async Task<RespuestaJson> DesactivarPuesto(Puestos puesto)
        {
            puesto.IdUsuarioDel = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).DesactivarPuesto(puesto);
        }

        [HttpPost]
        public async Task<RespuestaJson> ReactivarPuesto(Puestos puesto)
        {
            puesto.IdUsuarioUpd = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).ReactivarPuesto(puesto);
        }
    }
}
