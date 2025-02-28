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
        public IActionResult Sistemas()
        {
            ViewBag.Usuario = GetSessionValue("IdUsuario").ToString() ?? "";
            return View();
        }
        public IActionResult Usuarios()
        {
            ViewBag.Usuario = GetSessionValue("IdUsuario").ToString() ?? "";
            return View();
        }

        public IActionResult ClientesUsuarios()
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

        //============================================================================================================================\\
        //================================================   Controlador de Sistemas   =============================================== \\
        //==============================================================================================================================\\

        [HttpPost]
        public async Task<RespuestaJson> GetSistemas()
        {
            RespuestaJson respuesta = await new Catalogos(_dapperContext).GetSistemas();
            return respuesta;
        }

        [HttpPost]
        public async Task<RespuestaJson> GetSistemasClientes()
        {
            RespuestaJson respuesta = await new Catalogos(_dapperContext).GetSistemasClientes();
            return respuesta;
        }

        [HttpPost]
        public async Task<RespuestaJson> AgregarSistema(Sistemas sistema)
        {
            sistema.IdUsuarioSet = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).AgregarSistema(sistema);
        }

        [HttpPost]
        public async Task<RespuestaJson> EditarSistema(Sistemas sistema)
        {
            sistema.IdUsuarioUpd = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).EditarSistema(sistema);
        }

        [HttpPost]
        public async Task<RespuestaJson> DesactivarSistema(Sistemas sistema)
        {
            sistema.IdUsuarioDel = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).DesactivarSistema(sistema);
        }

        [HttpPost]
        public async Task<RespuestaJson> ReactivarSistema(Sistemas sistema)
        {
            sistema.IdUsuarioUpd = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).ReactivarSistema(sistema);
        }

        //============================================================================================================================\\
        //================================================   Controlador de Usuarios   =============================================== \\
        //==============================================================================================================================\\

        [HttpPost]
        public async Task<RespuestaJson> GetUsuarios()
        {
            RespuestaJson respuesta = await new Catalogos(_dapperContext).GetUsuarios();
            return respuesta;
        }

        [HttpPost]
        public async Task<RespuestaJson> GetUsuariosPuestos()
        {
            RespuestaJson respuesta = await new Catalogos(_dapperContext).GetUsuariosPuestos();
            return respuesta;
        }

        [HttpPost]
        public async Task<RespuestaJson> AgregarUsuario(Usuarios usuarios)
        {
            usuarios.IdUsuarioSet = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).AgregarUsuario(usuarios);
        }

        [HttpPost]
        public async Task<RespuestaJson> EditarUsuario(Usuarios usuarios)
        {
            usuarios.IdUsuarioUpd = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).EditarUsuario(usuarios);
        }

        [HttpPost]
        public async Task<RespuestaJson> DesactivarUsuario(Usuarios usuario)
        {
            usuario.IdUsuarioDel = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).DesactivarUsuario(usuario);
        }

        [HttpPost]
        public async Task<RespuestaJson> ReactivarUsuario(Usuarios usuario)
        {
            usuario.IdUsuarioUpd = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).ReactivarUsuario(usuario);
        }

        //============================================================================================================================\\
        //============================================   Controlador de ClientesUsuarios   =========================================== \\
        //==============================================================================================================================\\

        [HttpPost]
        public async Task<RespuestaJson> GetClientesUsuarios()
        {
            RespuestaJson respuesta = await new Catalogos(_dapperContext).GetClientesUsuarios();
            return respuesta;
        }

        [HttpPost]
        public async Task<RespuestaJson> GetClientesUsuariosPuestos()
        {
            RespuestaJson respuesta = await new Catalogos(_dapperContext).GetClientesUsuariosPuestos();
            return respuesta;
        }

        [HttpPost]
        public async Task<RespuestaJson> AgregarClienteUsuario(ClientesUsuarios clientesusuarios)
        {
            clientesusuarios.IdUsuarioSet = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).AgregarClienteUsuario(clientesusuarios);
        }

        [HttpPost]
        public async Task<RespuestaJson> EditarClienteUsuario(ClientesUsuarios clientesusuarios)
        {
            clientesusuarios.IdUsuarioUpd = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).EditarClienteUsuario(clientesusuarios);
        }

        [HttpPost]
        public async Task<RespuestaJson> DesactivarClienteUsuario(ClientesUsuarios clienteusuario)
        {
            clienteusuario.IdUsuarioDel = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).DesactivarClienteUsuario(clienteusuario);
        }

        [HttpPost]
        public async Task<RespuestaJson> ReactivarClienteUsuario(ClientesUsuarios clienteusuario)
        {
            clienteusuario.IdUsuarioUpd = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).ReactivarClienteUsuario(clienteusuario);
        }
    }
}
