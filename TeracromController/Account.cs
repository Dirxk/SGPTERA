using GestionProyectos.Models;
using Microsoft.Extensions.Configuration;
using TeracromDatabase;

namespace TeracromController
{
    public class Account
    {
        private readonly IConfiguration _configuration;

        public Account(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<Usuarios> GetUsuarios(string gid = "")
        {
            Usuarios usuarios = new Usuarios();

            //using (var cnn = new DapperContext(_configuration))
            //{
            //    try
            //    {
            //        cnn.AbrirConexion("SGP");

            //        usuarios = await cnn.QueryFirstOrDefaultAsync<Usuarios>(
            //            "SELECT * FROM Empleados WHERE Nombre = @gid;",
            //            new { gid }
            //        );

            //        if (usuarios == null)
            //        {
            //            usuarios = new Usuarios
            //            {
            //                usuarios = 0,
            //                Gid = new Gids { Gid = "" }
            //            };
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        usuarios = new Usuarios
            //        {
            //            usuarios = 0,
            //            Gid = new Gids { Gid = "" }
            //        };
            //    }
            //}

            return usuarios;
        }

    }
}
