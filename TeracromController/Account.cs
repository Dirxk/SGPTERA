using Dapper;
using GestionProyectos.Models;
using Microsoft.Extensions.Configuration;
using System.Data;
using TeracromDatabase;
using TeracromModels;

namespace TeracromController
{
    public class Account
    {
        private readonly DapperContext _dapperContext;

        public Account(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        public async Task<RespuestaJson> VerificarUsuario(string email, string password, string user2)
        {
            RespuestaJson respuestaUser = new RespuestaJson();
            try
            {
                _dapperContext.AbrirConexion("SGP");

                // Consulta SQL para validar usuario con hash de la contraseña
                string sql = @"
                SELECT Id, NombreUsuario, ApellidoPaternoUsuario, Correo 
                FROM Usuarios 
                WHERE (Correo = @Correo OR Usuario = @Usuario) 
                AND Contrasena = HASHBYTES('SHA2_256', CONVERT(NVARCHAR(250), @Contrasena))
                AND FlgActivo = 1"; // Solo usuarios activos

                object parameters = new { Correo = email, Contrasena = password, Usuario = user2 };

                // Ejecutar la consulta usando Dapper
                var user = await _dapperContext.QueryFirstOrDefaultAsync<Usuarios>(
                    sql, parameters);

                // Validar si el usuario existe
                if (user == null)
                {
                    respuestaUser.Resultado = false;
                    respuestaUser.Mensaje = "Usuario o contraseña incorrectos.";
                    return respuestaUser;
                }

                // Si las credenciales son válidas
                respuestaUser.Resultado = true;
                respuestaUser.Mensaje = "Inicio de sesión exitoso.";
                respuestaUser.Data = new Usuarios
                {
                    Id = user.Id,
                    NombreUsuario = user.NombreUsuario,
                    ApellidoPaternoUsuario = user.ApellidoPaternoUsuario,
                    ApellidoMaternoUsuario = user.ApellidoMaternoUsuario,
                    Correo = user.Correo
                };
            }
            catch (Exception ex)
            {
                respuestaUser.Resultado = false;
                respuestaUser.Mensaje = "Ocurrió un error al verificar las credenciales.";
                respuestaUser.Errores.Add(ex.Message);
            }

            return respuestaUser;
        }

        public async Task<RespuestaJson> RegistrarUsuario(Usuarios usuario)
        {
            RespuestaJson respuesta = new RespuestaJson();

            try
            {
               

                // Validar el dominio del correo y asignar IdPuesto
                int idPuesto;
                if (usuario.Correo.EndsWith("@teracrom.com", StringComparison.OrdinalIgnoreCase))
                {
                    idPuesto = 1; // Si el correo termina con @teracrom.com
                }
                else
                {
                    idPuesto = 2; // Para cualquier otro dominio
                }

                string sql = @"
                INSERT INTO Usuarios 
                (Usuario, NombreUsuario, ApellidoPaternoUsuario, ApellidoMaternoUsuario, IdPuesto, Telefono, Correo, Contrasena) 
                VALUES 
                (@Usuario, @NombreUsuario, @ApellidoPaternoUsuario, @ApellidoMaternoUsuario, @IdPuesto, @Telefono, @Correo, 
                 HASHBYTES('SHA2_256', CONVERT(NVARCHAR(50), @Contrasena)));";

                var parametros = new DynamicParameters();
                parametros.Add("@Usuario", usuario.Usuario, DbType.String);
                parametros.Add("@NombreUsuario", usuario.NombreUsuario, DbType.String);
                parametros.Add("@ApellidoPaternoUsuario", usuario.ApellidoPaternoUsuario, DbType.String);
                parametros.Add("@ApellidoMaternoUsuario", usuario.ApellidoMaternoUsuario, DbType.String);
                parametros.Add("@IdPuesto", idPuesto, DbType.Int32); // Usamos el valor calculado
                parametros.Add("@Telefono", usuario.Telefono, DbType.String);
                parametros.Add("@Correo", usuario.Correo, DbType.String);
                parametros.Add("@Contrasena", usuario.Contrasena, DbType.String);

                // Abrir conexión y ejecutar la consulta
                _dapperContext.AbrirConexion("SGP");
                int filasAfectadas = await _dapperContext.ExecuteAsync(sql, parametros);

                if (filasAfectadas > 0)
                {
                    respuesta.Resultado = true;
                    respuesta.Mensaje = "Usuario registrado exitosamente.";
                }
                else
                {
                    respuesta.Mensaje = "No se pudo registrar el usuario.";
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al registrar el usuario: " + ex.Message;
                respuesta.Errores.Add(ex.Message);
            }
            finally
            {
                _dapperContext.Dispose();
            }

            return respuesta;
        }

    }
}
