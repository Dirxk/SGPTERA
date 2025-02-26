using Dapper;
using GestionProyectos.Models;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Text.RegularExpressions;
using TeracromDatabase;
using TeracromModels;

namespace TeracromController
{
    public class Catalogos
    {
        private readonly DapperContext _dapperContext;

        public Catalogos(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        //============================================================================================================================\\
        //====================================================   Modelo Clientes   =================================================== \\
        //==============================================================================================================================\\

        public async Task<RespuestaJson> GetClientes()
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Abrir la conexión a la base de datos
                _dapperContext.AbrirConexion("SGP");

                string sql = "SELECT Id, RazonSocial, RFC, Prefijo, Telefono, Logotipo, FlgActivo FROM Clientes";
                var clientes = await _dapperContext.QueryAsync<Clientes>(sql);
                if (clientes != null)
                {
                    respuesta.Resultado = true;
                    respuesta.Data = clientes.Select(s => new Clientes
                    {
                        Id = s.Id,
                        RazonSocial = s.RazonSocial,
                        RFC = s.RFC,
                        Prefijo = s.Prefijo,
                        Telefono = s.Telefono,
                        Logotipo = s.Logotipo,
                        FlgActivo = s.FlgActivo
                    }).ToList();
                }
                else
                {
                    respuesta.Mensaje = "No se encontraron clientes activos.";
                    respuesta.Data = new List<Clientes>(); // Inicializar Data para evitar null
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al obtener los datos de los clientes." + ex.Message;
                respuesta.Data = new List<Clientes>(); // Inicializar Data para evitar null
            }
            finally
            {
                // Cerrar o liberar la conexión (si es necesario)
                _dapperContext.Dispose();
            }
            return respuesta;
        }

        public async Task<RespuestaJson> AgregarCliente(Clientes cliente)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Validar que el archivo de imagen esté presente y sea válido
                if (cliente.LogotipoFile != null && cliente.LogotipoFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await cliente.LogotipoFile.CopyToAsync(memoryStream);
                        cliente.Logotipo = memoryStream.ToArray(); // Convierte la imagen a byte[]
                    }
                }

                // Convertir RFC y Prefijo a mayúsculas
                cliente.RFC = cliente.RFC?.ToUpper();
                cliente.Prefijo = cliente.Prefijo?.ToUpper();

                // Eliminar caracteres no numéricos del número de teléfono
                if (!string.IsNullOrEmpty(cliente.Telefono))
                {
                    cliente.Telefono = Regex.Replace(cliente.Telefono, @"[^\d]", "");
                }

                // Abrir conexión y validar duplicados
                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Validar si la Razon Social ya existe en la base de datos
                    string sqlValidarRazonSocial = "SELECT COUNT(*) FROM Clientes WHERE RazonSocial = @RazonSocial;";
                    var parametrosValidarRazonSocial = new DynamicParameters();
                    parametrosValidarRazonSocial.Add("@RazonSocial", cliente.RazonSocial, DbType.String);

                    int RazonSocialExistente = await connection.ExecuteScalarAsync<int>(sqlValidarRazonSocial, parametrosValidarRazonSocial);

                    if (RazonSocialExistente > 0)
                    {
                        respuesta.Mensaje = "La razón social ya existe en la base de datos.";
                        return respuesta;
                    }

                    // Validar si el Prefijo ya existe en la base de datos
                    string sqlValidarPrefijo = "SELECT COUNT(*) FROM Clientes WHERE Prefijo = @Prefijo;";
                    var parametrosValidarPrefijo = new DynamicParameters();
                    parametrosValidarPrefijo.Add("@Prefijo", cliente.Prefijo, DbType.String);

                    int PrefijoExistente = await connection.ExecuteScalarAsync<int>(sqlValidarPrefijo, parametrosValidarPrefijo);

                    if (PrefijoExistente > 0)
                    {
                        respuesta.Mensaje = "El prefijo ya existe en la base de datos.";
                        return respuesta;
                    }

                    // Validar si el RFC ya existe en la base de datos
                    string sqlValidarRFC = "SELECT COUNT(*) FROM Clientes WHERE RFC = @RFC;";
                    var parametrosValidarRFC = new DynamicParameters();
                    parametrosValidarRFC.Add("@RFC", cliente.RFC, DbType.String);

                    int rfcExistente = await connection.ExecuteScalarAsync<int>(sqlValidarRFC, parametrosValidarRFC);

                    if (rfcExistente > 0)
                    {
                        respuesta.Mensaje = "El RFC ya existe en la base de datos.";
                        return respuesta;
                    }

                    // Validar si el número de teléfono ya existe en la base de datos
                    string sqlValidarTelefono = "SELECT COUNT(*) FROM Clientes WHERE Telefono = @Telefono;";
                    var parametrosValidarTelefono = new DynamicParameters();
                    parametrosValidarTelefono.Add("@Telefono", cliente.Telefono, DbType.String);

                    int telefonoExistente = await connection.ExecuteScalarAsync<int>(sqlValidarTelefono, parametrosValidarTelefono);

                    if (telefonoExistente > 0)
                    {
                        respuesta.Mensaje = "El número de teléfono ya existe en la base de datos.";
                        return respuesta;
                    }

                    // Consulta SQL para insertar el cliente
                    string sqlInsertar = @"
                    INSERT INTO Clientes 
                    (RazonSocial, RFC, Prefijo, Telefono, Logotipo, IdUsuarioSet) 
                    VALUES 
                    (@RazonSocial, @RFC, @Prefijo, @Telefono, @Logotipo, @IdUsuarioSet);";

                    var parametrosInsertar = new DynamicParameters();
                    parametrosInsertar.Add("@RazonSocial", cliente.RazonSocial, DbType.String);
                    parametrosInsertar.Add("@RFC", cliente.RFC, DbType.String);
                    parametrosInsertar.Add("@Prefijo", cliente.Prefijo, DbType.String);
                    parametrosInsertar.Add("@Telefono", cliente.Telefono, DbType.String);
                    parametrosInsertar.Add("@Logotipo", cliente.Logotipo, DbType.Binary);
                    parametrosInsertar.Add("@IdUsuarioSet", cliente.IdUsuarioSet, DbType.Int32);

                    // Ejecutar la consulta de inserción
                    int filasAfectadas = await connection.ExecuteAsync(sqlInsertar, parametrosInsertar);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "Cliente agregado exitosamente.";
                        respuesta.Data = new { ClienteId = cliente.Id }; // Puedes devolver el ID del cliente si lo necesitas
                    }
                    else
                    {
                        respuesta.Mensaje = "No se pudo agregar el cliente.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al agregar el cliente: " + ex.Message;
                respuesta.Errores.Add(ex.Message);
            }
            finally
            {
                _dapperContext.Dispose(); // Liberar recursos
            }

            return respuesta;
        }


        public async Task<RespuestaJson> EditarCliente(Clientes cliente)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Validar que el archivo de imagen esté presente y sea válido
                if (cliente.LogotipoFile != null && cliente.LogotipoFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await cliente.LogotipoFile.CopyToAsync(memoryStream);
                        cliente.Logotipo = memoryStream.ToArray(); // Convierte la imagen a byte[]
                    }
                }
                else if (!string.IsNullOrEmpty(cliente.LogotipoBase64))
                {
                    // Si no hay una nueva imagen, pero hay una imagen existente en base64, convertirla a byte[]
                    cliente.Logotipo = Convert.FromBase64String(cliente.LogotipoBase64.Split(',')[1]);
                }

                // Convertir RFC y Prefijo a mayúsculas
                cliente.RFC = cliente.RFC?.ToUpper();
                cliente.Prefijo = cliente.Prefijo?.ToUpper();

                // Eliminar caracteres no numéricos del número de teléfono
                if (!string.IsNullOrEmpty(cliente.Telefono))
                {
                    cliente.Telefono = Regex.Replace(cliente.Telefono, @"[^\d]", "");
                }

                // Obtener la fecha y hora actual
                cliente.FechaUpd = DateTime.Now;

                // Abrir conexión y validar duplicados
                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Consulta SQL para actualizar el cliente
                    string sqlActualizar = @"
                    UPDATE Clientes 
                    SET RazonSocial = @RazonSocial, 
                        RFC = @RFC, 
                        Prefijo = @Prefijo, 
                        Telefono = @Telefono, 
                        Logotipo = @Logotipo,
                        IdUsuarioUpd = @IdUsuarioUpd,
                        FechaUpd = @FechaUpd
                    WHERE Id = @Id;";

                    var parametrosActualizar = new DynamicParameters();
                    parametrosActualizar.Add("@Id", cliente.Id, DbType.Int32);
                    parametrosActualizar.Add("@RazonSocial", cliente.RazonSocial, DbType.String);
                    parametrosActualizar.Add("@RFC", cliente.RFC, DbType.String);
                    parametrosActualizar.Add("@Prefijo", cliente.Prefijo, DbType.String);
                    parametrosActualizar.Add("@Telefono", cliente.Telefono, DbType.String);
                    parametrosActualizar.Add("@Logotipo", cliente.Logotipo, DbType.Binary);
                    parametrosActualizar.Add("@IdUsuarioUpd", cliente.IdUsuarioUpd, DbType.Int32);
                    parametrosActualizar.Add("@FechaUpd", cliente.FechaUpd, DbType.DateTime);

                    // Ejecutar la consulta de actualización
                    int filasAfectadas = await connection.ExecuteAsync(sqlActualizar, parametrosActualizar);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "Cliente actualizado exitosamente.";
                    }
                    else
                    {
                        respuesta.Mensaje = "No se pudo actualizar el cliente.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al actualizar el cliente: " + ex.Message;
                respuesta.Errores.Add(ex.Message);
            }
            finally
            {
                _dapperContext.Dispose(); // Liberar recursos
            }

            return respuesta;
        }

        public async Task<RespuestaJson> DesactivarCliente(Clientes cliente)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                
                // Obtener la fecha y hora actual
                cliente.FechaDel = DateTime.Now;

                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Consulta SQL para desactivar el cliente
                    string sql = @"
                    UPDATE Clientes 
                    SET FlgActivo = 0, 
                        IdUsuarioDel = @IdUsuarioDel, 
                        FechaDel = @FechaDel 
                    WHERE Id = @Id";

                    var parametros = new DynamicParameters();
                    parametros.Add("@Id", cliente.Id, DbType.Int32);
                    parametros.Add("@IdUsuarioDel", cliente.IdUsuarioDel, DbType.Int32);
                    parametros.Add("@FechaDel", cliente.FechaDel, DbType.DateTime);

                    // Ejecutar la consulta
                    int filasAfectadas = await connection.ExecuteAsync(sql, parametros);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "Cliente desactivado correctamente.";
                    }
                    else
                    {
                        respuesta.Mensaje = "No se encontró el cliente o ya está desactivado.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al desactivar el cliente: " + ex.Message;
            }
            finally
            {
                _dapperContext.Dispose();
            }
            return respuesta;
        }

        public async Task<RespuestaJson> ReactivarCliente(Clientes cliente)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {

                // Obtener la fecha y hora actual
                cliente.FechaUpd = DateTime.Now;

                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Consulta SQL para desactivar el cliente
                    string sql = @"
                    UPDATE Clientes 
                    SET FlgActivo = 1, 
                        IdUsuarioUpd = @IdUsuarioUpd, 
                        FechaUpd = @FechaUpd
                    WHERE Id = @Id";

                    var parametros = new DynamicParameters();
                    parametros.Add("@Id", cliente.Id, DbType.Int32);
                    parametros.Add("@IdUsuarioUpd", cliente.IdUsuarioUpd, DbType.Int32);
                    parametros.Add("@FechaUpd", cliente.FechaUpd, DbType.DateTime);

                    // Ejecutar la consulta
                    int filasAfectadas = await connection.ExecuteAsync(sql, parametros);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "Cliente reactivado correctamente.";
                    }
                    else
                    {
                        respuesta.Mensaje = "No se encontró el cliente o ya está reactivado.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al reactivar el cliente: " + ex.Message;
            }
            finally
            {
                _dapperContext.Dispose();
            }
            return respuesta;
        }

        //============================================================================================================================\\
        //====================================================   Modelo Puestos   =================================================== \\
        //==============================================================================================================================\\

        public async Task<RespuestaJson> GetPuestos()
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Abrir la conexión a la base de datos
                _dapperContext.AbrirConexion("SGP");

                string sql = "SELECT Id, Descripcion, Tipo, FlgActivo FROM Puestos";
                var puestos = await _dapperContext.QueryAsync<Puestos>(sql);
                if (puestos != null)
                {
                    respuesta.Resultado = true;
                    respuesta.Data = puestos.Select(s => new Puestos
                    {
                        Id = s.Id,
                        Descripcion = s.Descripcion,
                        Tipo = s.Tipo,
                        FlgActivo = s.FlgActivo
                    }).ToList();
                }
                else
                {
                    respuesta.Mensaje = "No se encontraron puestos activos.";
                    respuesta.Data = new List<Puestos>(); // Inicializar Data para evitar null
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al obtener los datos de los puestos." + ex.Message;
                respuesta.Data = new List<Puestos>(); // Inicializar Data para evitar null
            }
            finally
            {
                // Cerrar o liberar la conexión (si es necesario)
                _dapperContext.Dispose();
            }
            return respuesta;
        }

        public async Task<RespuestaJson> AgregarPuesto(Puestos puesto)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                
                // Abrir conexión y validar duplicados
                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {

                    // Validar si la Descripcion ya existe en la base de datos
                    string sqlValidarDescripcion = "SELECT COUNT(*) FROM Puestos WHERE Descripcion = @Descripcion;";
                    var parametrosValidarDescripcion = new DynamicParameters();
                    parametrosValidarDescripcion.Add("@Descripcion", puesto.Descripcion, DbType.String);

                    int DescripcionExistente = await connection.ExecuteScalarAsync<int>(sqlValidarDescripcion, parametrosValidarDescripcion);

                    if (DescripcionExistente > 0)
                    {
                        respuesta.Mensaje = "El prefijo ya existe en la base de datos.";
                        return respuesta;
                    }

                    // Consulta SQL para insertar el puesto
                    string sqlInsertar = @"
                    INSERT INTO Puestos 
                    (Descripcion, Tipo, IdUsuarioSet) 
                    VALUES 
                    (@Descripcion, @Tipo, @IdUsuarioSet);";

                    var parametrosInsertar = new DynamicParameters();
                    parametrosInsertar.Add("@Descripcion", puesto.Descripcion, DbType.String);
                    parametrosInsertar.Add("@Tipo", puesto.Tipo, DbType.String);
                    parametrosInsertar.Add("@IdUsuarioSet", puesto.IdUsuarioSet, DbType.Int32);

                    // Ejecutar la consulta de inserción
                    int filasAfectadas = await connection.ExecuteAsync(sqlInsertar, parametrosInsertar);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "Puesto agregado exitosamente.";
                        respuesta.Data = new { PuestoId = puesto.Id }; // Puedes devolver el ID del puesto si lo necesitas
                    }
                    else
                    {
                        respuesta.Mensaje = "No se pudo agregar el puesto.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al agregar el puesto: " + ex.Message;
                respuesta.Errores.Add(ex.Message);
            }
            finally
            {
                _dapperContext.Dispose(); // Liberar recursos
            }

            return respuesta;
        }

        public async Task<RespuestaJson> EditarPuesto(Puestos puesto)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {

                // Obtener la fecha y hora actual
                puesto.FechaUpd = DateTime.Now;

                // Abrir conexión y validar duplicados
                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Consulta SQL para actualizar el puesto
                    string sqlActualizar = @"
                    UPDATE Puestos 
                    SET Descripcion = @Descripcion, 
                        Tipo = @Tipo, 
                        IdUsuarioUpd = @IdUsuarioUpd,
                        FechaUpd = @FechaUpd
                    WHERE Id = @Id;";

                    var parametrosActualizar = new DynamicParameters();
                    parametrosActualizar.Add("@Id", puesto.Id, DbType.Int32);
                    parametrosActualizar.Add("@Descripcion", puesto.Descripcion, DbType.String);
                    parametrosActualizar.Add("@Tipo", puesto.Tipo, DbType.String);
                    parametrosActualizar.Add("@IdUsuarioUpd", puesto.IdUsuarioUpd, DbType.Int32);
                    parametrosActualizar.Add("@FechaUpd", puesto.FechaUpd, DbType.DateTime);

                    // Ejecutar la consulta de actualización
                    int filasAfectadas = await connection.ExecuteAsync(sqlActualizar, parametrosActualizar);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "Puesto actualizado exitosamente.";
                    }
                    else
                    {
                        respuesta.Mensaje = "No se pudo actualizar el puesto.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al actualizar el puesto: " + ex.Message;
                respuesta.Errores.Add(ex.Message);
            }
            finally
            {
                _dapperContext.Dispose(); // Liberar recursos
            }

            return respuesta;
        }

        public async Task<RespuestaJson> DesactivarPuesto(Puestos puesto)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Obtener la fecha y hora actual
                puesto.FechaDel = DateTime.Now;

                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Consulta SQL para desactivar el puesto
                    string sql = @"
                    UPDATE Puestos 
                    SET FlgActivo = 0, 
                        IdUsuarioDel = @IdUsuarioDel, 
                        FechaDel = @FechaDel 
                    WHERE Id = @Id";

                    var parametros = new DynamicParameters();
                    parametros.Add("@Id", puesto.Id, DbType.Int32);
                    parametros.Add("@IdUsuarioDel", puesto.IdUsuarioDel, DbType.Int32);
                    parametros.Add("@FechaDel", puesto.FechaDel, DbType.DateTime);

                    // Ejecutar la consulta
                    int filasAfectadas = await connection.ExecuteAsync(sql, parametros);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "Puesto desactivado correctamente.";
                    }
                    else
                    {
                        respuesta.Mensaje = "No se encontró el puesto o ya está desactivado.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al desactivar el puesto: " + ex.Message;
            }
            finally
            {
                _dapperContext.Dispose();
            }
            return respuesta;
        }

        public async Task<RespuestaJson> ReactivarPuesto(Puestos puesto)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Obtener la fecha y hora actual
                puesto.FechaUpd = DateTime.Now;

                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Consulta SQL para reactivar el puesto
                    string sql = @"
                    UPDATE Puestos 
                    SET FlgActivo = 1, 
                        IdUsuarioUpd = @IdUsuarioUpd, 
                        FechaUpd = @FechaUpd
                    WHERE Id = @Id";

                    var parametros = new DynamicParameters();
                    parametros.Add("@Id", puesto.Id, DbType.Int32);
                    parametros.Add("@IdUsuarioUpd", puesto.IdUsuarioUpd, DbType.Int32);
                    parametros.Add("@FechaUpd", puesto.FechaUpd, DbType.DateTime);

                    // Ejecutar la consulta
                    int filasAfectadas = await connection.ExecuteAsync(sql, parametros);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "Puesto reactivado correctamente.";
                    }
                    else
                    {
                        respuesta.Mensaje = "No se encontró el puesto o ya está reactivado.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al reactivar el puesto: " + ex.Message;
            }
            finally
            {
                _dapperContext.Dispose();
            }
            return respuesta;
        }
    }
}
