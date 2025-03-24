using Dapper;
using GestionProyectos.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using TeracromDatabase;
using TeracromModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace TeracromController
{
    public class Account
    {
        private readonly DapperContext _dapperContext;

        public Account(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        public async Task<RespuestaJson> VerificarUsuario(Usuarios User)
        {
            RespuestaJson respuesta = new RespuestaJson();

            try
            {
                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Consulta modificada para devolver el usuario completo
                    string sqlVerificarUsuario = @"
                    SELECT 
                        Id,
                        Correo,
                        Usuario
                    FROM 
                        Usuarios 
                    WHERE 
                        (Correo = @Correo OR Usuario = @Correo)
                        AND Contrasena = HASHBYTES('SHA2_256', CONVERT(NVARCHAR(50), @Contrasena))";

                    var parametros = new DynamicParameters();
                    parametros.Add("@Correo", User.Correo, DbType.String);
                    parametros.Add("@Contrasena", User.Contrasena, DbType.String);

                    // Obtenemos el usuario completo en lugar de solo el conteo
                    var usuario = await connection.QueryFirstOrDefaultAsync<Usuarios>(sqlVerificarUsuario, parametros);

                    if (usuario != null)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "Inicio de sesión exitoso";
                        respuesta.Data = usuario; // Asignamos el usuario completo
                    }
                    else
                    {
                        respuesta.Mensaje = "Credenciales incorrectas. Verifique su usuario/correo y contraseña.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Resultado = false;
                respuesta.Mensaje = "Ocurrió un error al verificar el usuario: " + ex.Message;
                respuesta.Errores.Add(ex.Message);
            }

            return respuesta;
        }

        public async Task<RespuestaJson> RegistrarUsuario(Usuarios usuario, string webRoot)
        {
            RespuestaJson respuesta = new RespuestaJson();

            using (var connection = _dapperContext.AbrirConexion("SGP"))
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Validar si el NombreUsuario ya existe
                        string sqlValidarNombreUsuario = "SELECT COUNT(*) FROM Usuarios WHERE Usuario = @Usuario;";
                        var parametrosValidarNombreUsuario = new DynamicParameters();
                        parametrosValidarNombreUsuario.Add("@Usuario", usuario.Usuario, DbType.String);

                        int nombreUsuarioExistente = await connection.ExecuteScalarAsync<int>(sqlValidarNombreUsuario, parametrosValidarNombreUsuario, transaction: transaction);

                        if (nombreUsuarioExistente > 0)
                        {
                            respuesta.Mensaje = "Este nombre de usuario ya está en uso. Por favor, elige otro.";
                            return respuesta;
                        }

                        // Validar si el Teléfono ya existe
                        string sqlValidarTelefono = "SELECT COUNT(*) FROM Usuarios WHERE Telefono = @Telefono;";
                        var parametrosValidarTelefono = new DynamicParameters();
                        parametrosValidarTelefono.Add("@Telefono", usuario.Telefono, DbType.String);

                        int telefonoExistente = await connection.ExecuteScalarAsync<int>(sqlValidarTelefono, parametrosValidarTelefono, transaction: transaction);

                        if (telefonoExistente > 0)
                        {
                            respuesta.Mensaje = "Este número de teléfono ya está registrado.";
                            return respuesta;
                        }

                        // Validar si el Correo ya existe
                        string sqlValidarCorreo = "SELECT COUNT(*) FROM Usuarios WHERE Correo = @Correo;";
                        var parametrosValidarCorreo = new DynamicParameters();
                        parametrosValidarCorreo.Add("@Correo", usuario.Correo, DbType.String);

                        int correoExistente = await connection.ExecuteScalarAsync<int>(sqlValidarCorreo, parametrosValidarCorreo, transaction: transaction);

                        if (correoExistente > 0)
                        {
                            respuesta.Mensaje = "Este correo electrónico ya está registrado.";
                            return respuesta;
                        }

                        // Asignar imagen de perfil por defecto
                        string rutaImagenPorDefecto = Path.Combine(webRoot, "lib/dashboard/assets/img/AvatarTeracrom.png");

                        if (System.IO.File.Exists(rutaImagenPorDefecto))
                        {
                            usuario.FotoPerfil = await System.IO.File.ReadAllBytesAsync(rutaImagenPorDefecto);
                        }
                        else
                        {
                            respuesta.Mensaje = "No se encontró la imagen por defecto.";
                            return respuesta;
                        }

                        // Insertar usuario
                        string sql = @"
                        INSERT INTO Usuarios 
                        (Usuario, NombreUsuario, ApellidoPaternoUsuario, ApellidoMaternoUsuario, IdPuesto, Telefono, Correo, Contrasena, FotoPerfil) 
                        VALUES 
                        (@Usuario, @NombreUsuario, @ApellidoPaternoUsuario, @ApellidoMaternoUsuario, @IdPuesto, @Telefono, @Correo, 
                        HASHBYTES('SHA2_256', CONVERT(NVARCHAR(50), @Contrasena)), @FotoPerfil);";

                        var parametros = new DynamicParameters();
                        parametros.Add("@Usuario", usuario.Usuario, DbType.String);
                        parametros.Add("@NombreUsuario", usuario.NombreUsuario, DbType.String);
                        parametros.Add("@ApellidoPaternoUsuario", usuario.ApellidoPaternoUsuario, DbType.String);
                        parametros.Add("@ApellidoMaternoUsuario", usuario.ApellidoMaternoUsuario, DbType.String);
                        parametros.Add("@IdPuesto", 1, DbType.Int32);
                        parametros.Add("@Telefono", usuario.Telefono, DbType.String);
                        parametros.Add("@Correo", usuario.Correo, DbType.String);
                        parametros.Add("@Contrasena", usuario.Contrasena, DbType.String);
                        parametros.Add("@FotoPerfil", usuario.FotoPerfil, DbType.Binary);

                        int filasAfectadas = await connection.ExecuteAsync(sql, parametros, transaction: transaction);

                        if (filasAfectadas > 0)
                        {
                            transaction.Commit();
                            respuesta.Resultado = true;
                            respuesta.Mensaje = "Usuario registrado exitosamente.";
                        }
                        else
                        {
                            respuesta.Mensaje = "No se pudo registrar el usuario.";
                        }
                    }
                    catch (SqlException ex) when (ex.Number == 2627) // Código de error para violación de UNIQUE KEY
                    {
                        transaction.Rollback();

                        // Identificar qué campo causó la violación
                        if (ex.Message.Contains("Usuario"))
                        {
                            respuesta.Mensaje = "Este nombre de usuario ya está en uso. Por favor, elige otro.";
                        }
                        else if (ex.Message.Contains("Telefono"))
                        {
                            respuesta.Mensaje = "Este número de teléfono ya está registrado.";
                        }
                        else if (ex.Message.Contains("Correo"))
                        {
                            respuesta.Mensaje = "Este correo electrónico ya está registrado.";
                        }
                        else
                        {
                            respuesta.Mensaje = "Error de duplicidad: " + ex.Message;
                        }

                        respuesta.Errores.Add(ex.Message);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        respuesta.Mensaje = "Ocurrió un error inesperado al registrar el usuario: " + ex.Message;
                        respuesta.Errores.Add(ex.Message);
                    }
                }
            }

            return respuesta;
        }
    }
}
