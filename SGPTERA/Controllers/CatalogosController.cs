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
        public IActionResult ModuloSistemas()
        {
            ViewBag.Usuario = GetSessionValue("IdUsuario").ToString() ?? "";
            return View();
        }
        public IActionResult EstatusProyectos()
        {
            ViewBag.Usuario = GetSessionValue("IdUsuario").ToString() ?? "";
            return View();
        }
        public IActionResult EstatusTareas()
        {
            ViewBag.Usuario = GetSessionValue("IdUsuario").ToString() ?? "";
            return View();
        }
        public IActionResult EstatusSoportes()
        {
            ViewBag.Usuario = GetSessionValue("IdUsuario").ToString() ?? "";
            return View();
        }
        public IActionResult NivelServicioSoportes()
        {
            ViewBag.Usuario = GetSessionValue("IdUsuario").ToString() ?? "";
            return View();
        }
        public IActionResult TipoSoportes()
        {
            ViewBag.Usuario = GetSessionValue("IdUsuario").ToString() ?? "";
            return View();
        }
        public IActionResult NivelComplejidadTareas()
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
        public async Task<RespuestaJson> DesactivarUsuario(Usuarios usuarios)
        {
            usuarios.IdUsuarioDel = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).DesactivarUsuario(usuarios);
        }

        [HttpPost]
        public async Task<RespuestaJson> ReactivarUsuario(Usuarios usuarios)
        {
            usuarios.IdUsuarioUpd = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).ReactivarUsuario(usuarios);
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

        //============================================================================================================================\\
        //=========================================   Controlador de ModuloSistemas   =================================================\\
        //==============================================================================================================================\\

        [HttpPost]
        public async Task<RespuestaJson> GetModuloSistemas()
        {
            RespuestaJson respuesta = await new Catalogos(_dapperContext).GetModuloSistemas();
            return respuesta;
        }

        [HttpPost]
        public async Task<RespuestaJson> AgregarModuloSistema(ModuloSistemas modulosistema)
        {
            modulosistema.IdUsuarioSet = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).AgregarModuloSistema(modulosistema);
        }

        [HttpPost]
        public async Task<RespuestaJson> EditarModuloSistema(ModuloSistemas modulosistema)
        {
            modulosistema.IdUsuarioUpd = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).EditarModuloSistema(modulosistema);
        }

        [HttpPost]
        public async Task<RespuestaJson> DesactivarModuloSistema(ModuloSistemas modulosistema)
        {
            modulosistema.IdUsuarioDel = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).DesactivarModuloSistema(modulosistema);
        }

        [HttpPost]
        public async Task<RespuestaJson> ReactivarModuloSistema(ModuloSistemas modulosistema)
        {
            modulosistema.IdUsuarioUpd = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).ReactivarModuloSistema(modulosistema);
        }
    

        //============================================================================================================================\\
        //============================================   Controlador de EstatusProyectos   ============================================= \\
        //==============================================================================================================================\\

        [HttpPost]
        public async Task<RespuestaJson> GetEstatusProyectos()
        {
            RespuestaJson respuesta = await new Catalogos(_dapperContext).GetEstatusProyectos();
            return respuesta;
        }

        [HttpPost]
        public async Task<RespuestaJson> AgregarEstatusProyecto(EstatusProyectos estatusproyecto)
        {
            estatusproyecto.IdUsuarioSet = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).AgregarEstatusProyecto(estatusproyecto);
        }

        [HttpPost]
        public async Task<RespuestaJson> EditarEstatusProyecto(EstatusProyectos estatusproyecto)
        {
            estatusproyecto.IdUsuarioUpd = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).EditarEstatusProyecto(estatusproyecto);
        }

        [HttpPost]
        public async Task<RespuestaJson> DesactivarEstatusProyecto(EstatusProyectos estatusproyecto)
        {
            estatusproyecto.IdUsuarioDel = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).DesactivarEstatusProyecto(estatusproyecto);
        }

        [HttpPost]
        public async Task<RespuestaJson> ReactivarEstatusProyecto(EstatusProyectos estatusproyecto)
        {
            estatusproyecto.IdUsuarioUpd = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).ReactivarEstatusProyecto(estatusproyecto);
        }

        //============================================================================================================================\\
        //============================================   Controlador de EstatusTareas   ============================================= \\
        //==============================================================================================================================\\

        [HttpPost]
        public async Task<RespuestaJson> GetEstatusTareas()
        {
            RespuestaJson respuesta = await new Catalogos(_dapperContext).GetEstatusTareas();
            return respuesta;
        }

        [HttpPost]
        public async Task<RespuestaJson>  AgregarEstatusTarea(EstatusTareas estatustarea)
        {
            estatustarea.IdUsuarioSet = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).AgregarEstatusTarea(estatustarea);
        }

        [HttpPost]
        public async Task<RespuestaJson> EditarEstatusTarea(EstatusTareas estatustarea)
        {
            estatustarea.IdUsuarioUpd = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).EditarEstatusTarea(estatustarea);
        }

        [HttpPost]
        public async Task<RespuestaJson> DesactivarEstatusTarea(EstatusTareas estatustarea)
        {
            estatustarea.IdUsuarioDel = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).DesactivarEstatusTarea(estatustarea);
        }

        [HttpPost]
        public async Task<RespuestaJson> ReactivarEstatusTarea(EstatusTareas estatustarea)
        {
            estatustarea.IdUsuarioUpd = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).ReactivarEstatusTarea(estatustarea);
        }

        //============================================================================================================================\\
        //============================================   Controlador de EstatusSoportes   ============================================ \\
        //==============================================================================================================================\\

        [HttpPost]
        public async Task<RespuestaJson> GetEstatusSoportes()
        {
            RespuestaJson respuesta = await new Catalogos(_dapperContext).GetEstatusSoportes();
            return respuesta;
        }

        [HttpPost]
        public async Task<RespuestaJson> AgregarEstatusSoporte(EstatusSoportes estatussoporte)
        {
            estatussoporte.IdUsuarioSet = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).AgregarEstatusSoporte(estatussoporte);
        }

        [HttpPost]
        public async Task<RespuestaJson> EditarEstatusSoporte(EstatusSoportes estatussoporte)
        {
            estatussoporte.IdUsuarioUpd = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).EditarEstatusSoporte(estatussoporte);
        }

        [HttpPost]
        public async Task<RespuestaJson> DesactivarEstatusSoporte(EstatusSoportes estatussoporte)
        {
            estatussoporte.IdUsuarioDel = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).DesactivarEstatusSoporte(estatussoporte);
        }

        [HttpPost]
        public async Task<RespuestaJson> ReactivarEstatusSoporte(EstatusSoportes estatussoporte)
        {
            estatussoporte.IdUsuarioUpd = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).ReactivarEstatusSoporte(estatussoporte);
        }

        //============================================================================================================================\\
        //=========================================   Controlador de NivelServicioSoportes   ==========================================\\
        //==============================================================================================================================\\

        [HttpPost]
        public async Task<RespuestaJson> GetNivelServicioSoportes()
        {
            RespuestaJson respuesta = await new Catalogos(_dapperContext).GetNivelServicioSoportes();
            return respuesta;
        }

        [HttpPost]
        public async Task<RespuestaJson> AgregarNivelServicioSoporte(NivelServicioSoportes nivelserviciosoporte)
        {
            nivelserviciosoporte.IdUsuarioSet = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).AgregarNivelServicioSoporte(nivelserviciosoporte);
        }

        [HttpPost]
        public async Task<RespuestaJson> EditarNivelServicioSoporte(NivelServicioSoportes nivelserviciosoporte)
        {
            nivelserviciosoporte.IdUsuarioUpd = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).EditarNivelServicioSoporte(nivelserviciosoporte);
        }

        [HttpPost]
        public async Task<RespuestaJson> DesactivarNivelServicioSoporte(NivelServicioSoportes nivelserviciosoporte)
        {
            nivelserviciosoporte.IdUsuarioDel = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).DesactivarNivelServicioSoporte(nivelserviciosoporte);
        }

        [HttpPost]
        public async Task<RespuestaJson> ReactivarNivelServicioSoporte(NivelServicioSoportes nivelserviciosoporte)
        {
            nivelserviciosoporte.IdUsuarioUpd = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).ReactivarNivelServicioSoporte(nivelserviciosoporte);
        }

        //============================================================================================================================\\
        //=========================================   Controlador de TipoSoportes   ==================================================\\
        //==============================================================================================================================\\

        [HttpPost]
        public async Task<RespuestaJson> GetTipoSoportes()
        {
            RespuestaJson respuesta = await new Catalogos(_dapperContext).GetTipoSoportes();
            return respuesta;
        }

        [HttpPost]
        public async Task<RespuestaJson> AgregarTipoSoporte(TipoSoportes tiposoporte)
        {
            tiposoporte.IdUsuarioSet = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).AgregarTipoSoporte(tiposoporte);
        }

        [HttpPost]
        public async Task<RespuestaJson> EditarTipoSoporte(TipoSoportes tiposoporte)
        {
            tiposoporte.IdUsuarioUpd = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).EditarTipoSoporte(tiposoporte);
        }

        [HttpPost]
        public async Task<RespuestaJson> DesactivarTipoSoporte(TipoSoportes tiposoporte)
        {
            tiposoporte.IdUsuarioDel = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).DesactivarTipoSoporte(tiposoporte);
        }

        [HttpPost]
        public async Task<RespuestaJson> ReactivarTipoSoporte(TipoSoportes tiposoporte)
        {
            tiposoporte.IdUsuarioUpd = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).ReactivarTipoSoporte(tiposoporte);
        }

        //============================================================================================================================\\
        //=========================================   Controlador de NivelComplejidadTareas   =========================================\\
        //==============================================================================================================================\\

        [HttpPost]
        public async Task<RespuestaJson> GetNivelComplejidadTareas()
        {
            RespuestaJson respuesta = await new Catalogos(_dapperContext).GetNivelComplejidadTareas();
            return respuesta;
        }

        [HttpPost]
        public async Task<RespuestaJson> AgregarNivelComplejidadTarea(NivelComplejidadTareas nivelcomplejidadtarea)
        {
            nivelcomplejidadtarea.IdUsuarioSet = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).AgregarNivelComplejidadTarea(nivelcomplejidadtarea);
        }

        [HttpPost]
        public async Task<RespuestaJson> EditarNivelComplejidadTarea(NivelComplejidadTareas nivelcomplejidadtarea)
        {
            nivelcomplejidadtarea.IdUsuarioUpd = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).EditarNivelComplejidadTarea(nivelcomplejidadtarea);
        }

        [HttpPost]
        public async Task<RespuestaJson> DesactivarNivelComplejidadTarea(NivelComplejidadTareas nivelcomplejidadtarea)
        {
            nivelcomplejidadtarea.IdUsuarioDel = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).DesactivarNivelComplejidadTarea(nivelcomplejidadtarea);
        }

        [HttpPost]
        public async Task<RespuestaJson> ReactivarNivelComplejidadTarea(NivelComplejidadTareas nivelcomplejidadtarea)
        {
            nivelcomplejidadtarea.IdUsuarioUpd = int.Parse(GetSessionValue("IdUsuario").ToString() ?? "");
            return await new Catalogos(_dapperContext).ReactivarNivelComplejidadTarea(nivelcomplejidadtarea);
        }
    }
}
