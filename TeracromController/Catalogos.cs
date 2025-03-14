using Dapper;
using GestionProyectos.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Text;
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
        //====================================================   Modelo Puestos   ==================================================== \\
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
                    
                    // Validar si la Descripción ya existe en la base de datos
                    string sqlValidarDescripcion = "SELECT COUNT(*) FROM Puestos WHERE Descripcion = @Descripcion AND Tipo = @Tipo;";
                    var parametrosValidarDescripcion = new DynamicParameters();
                    parametrosValidarDescripcion.Add("@Descripcion", puesto.Descripcion, DbType.String);
                    parametrosValidarDescripcion.Add("@Tipo", puesto.Tipo, DbType.String); // Corregido: puesto.Tipo en lugar de puesto.Descripcion

                    int DescripcionExistente = await connection.ExecuteScalarAsync<int>(sqlValidarDescripcion, parametrosValidarDescripcion);

                    if (DescripcionExistente > 0)
                    {
                        respuesta.Mensaje = "El puesto ya existe en la base de datos para el tipo especificado."; // Mensaje más claro
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

                    // Validar si la Descripción ya existe en la base de datos
                    string sqlValidarDescripcion = "SELECT COUNT(*) FROM Puestos WHERE Descripcion = @Descripcion AND Tipo = @Tipo;";
                    var parametrosValidarDescripcion = new DynamicParameters();
                    parametrosValidarDescripcion.Add("@Descripcion", puesto.Descripcion, DbType.String);
                    parametrosValidarDescripcion.Add("@Tipo", puesto.Tipo, DbType.String); // Corregido: puesto.Tipo en lugar de puesto.Descripcion

                    int DescripcionExistente = await connection.ExecuteScalarAsync<int>(sqlValidarDescripcion, parametrosValidarDescripcion);

                    if (DescripcionExistente > 0)
                    {
                        respuesta.Mensaje = "El puesto ya existe en la base de datos para el tipo especificado."; // Mensaje más claro
                        return respuesta;
                    }

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

        //============================================================================================================================\\
        //====================================================   Modelo Sistemas   =================================================== \\
        //==============================================================================================================================\\

        public async Task<RespuestaJson> GetSistemas()
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Abrir la conexión a la base de datos
                _dapperContext.AbrirConexion("SGP");

                string sql = @"
                SELECT 
                    s.Id, 
                    s.Descripcion, 
                    c.RazonSocial, 
                    s.IdCliente,
                    s.Repositorio, 
                    s.Prefijo, 
                    s.FlgActivo 
                FROM 
                    Sistemas s
                INNER JOIN 
                    Clientes c ON s.IdCliente = c.Id"; 
                var sistemas = await _dapperContext.QueryAsync<Sistemas>(sql);
                if (sistemas != null)
                {
                    respuesta.Resultado = true;
                    respuesta.Data = sistemas.Select(s => new Sistemas
                    {
                        Id = s.Id,
                        Descripcion = s.Descripcion,
                        RazonSocial = s.RazonSocial, 
                        IdCliente = s.IdCliente,
                        Repositorio = s.Repositorio,
                        Prefijo = s.Prefijo,
                        FlgActivo = s.FlgActivo
                    }).ToList();
                }
                else
                {
                    respuesta.Mensaje = "No se encontraron sistemas activos.";
                    respuesta.Data = new List<Sistemas>(); // Inicializar Data para evitar null
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al obtener los datos de los sistemas." + ex.Message;
                respuesta.Data = new List<Sistemas>(); // Inicializar Data para evitar null
            }
            finally
            {
                // Cerrar o liberar la conexión (si es necesario)
                _dapperContext.Dispose();
            }
            return respuesta;
        }

        public async Task<RespuestaJson> GetSistemasClientes()
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Abrir la conexión a la base de datos
                _dapperContext.AbrirConexion("SGP");

                string sql = @"
                SELECT 
                    Id, 
                    RazonSocial 
                FROM 
                    Clientes";

                var clientes = await _dapperContext.QueryAsync<Clientes>(sql);
                if (clientes != null)
                {
                    respuesta.Resultado = true;
                    respuesta.Data = clientes.Select(c => new
                    {
                        Id = c.Id,
                        RazonSocial = c.RazonSocial
                    }).ToList();
                }
                else
                {
                    respuesta.Mensaje = "No se encontraron clientes.";
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

        public async Task<RespuestaJson> AgregarSistema(Sistemas sistema)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                sistema.Prefijo = sistema.Prefijo?.ToUpper();

                // Abrir conexión y validar duplicados
                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Validar si la Descripcion ya existe en la base de datos
                    string sqlValidarDescripcion = "SELECT COUNT(*) FROM Sistemas WHERE Descripcion = @Descripcion;";
                    var parametrosValidarDescripcion = new DynamicParameters();
                    parametrosValidarDescripcion.Add("@Descripcion", sistema.Descripcion, DbType.String);

                    int DescripcionExistente = await connection.ExecuteScalarAsync<int>(sqlValidarDescripcion, parametrosValidarDescripcion);

                    if (DescripcionExistente > 0)
                    {
                        respuesta.Mensaje = "La Descripcion ya existe en la base de datos.";
                        return respuesta;
                    }

                    // Validar si el Repositorio ya existe en la base de datos
                    string sqlValidarRepositorio = "SELECT COUNT(*) FROM Sistemas WHERE Repositorio = @Repositorio;";
                    var parametrosValidarRepositorio = new DynamicParameters();
                    parametrosValidarRepositorio.Add("@Repositorio", sistema.Repositorio, DbType.String);

                    int RepositorioExistente = await connection.ExecuteScalarAsync<int>(sqlValidarRepositorio, parametrosValidarRepositorio);

                    if (RepositorioExistente > 0)
                    {
                        respuesta.Mensaje = "El repositorio ya existe en la base de datos.";
                        return respuesta;
                    }

                    // Validar si el Prefijo ya existe en la base de datos
                    string sqlValidarPrefijo = "SELECT COUNT(*) FROM Sistemas WHERE Prefijo = @Prefijo;";
                    var parametrosValidarPrefijo = new DynamicParameters();
                    parametrosValidarPrefijo.Add("@Prefijo", sistema.Prefijo, DbType.String);

                    int PrefijoExistente = await connection.ExecuteScalarAsync<int>(sqlValidarPrefijo, parametrosValidarPrefijo);

                    if (PrefijoExistente > 0)
                    {
                        respuesta.Mensaje = "El prefijo ya existe en la base de datos.";
                        return respuesta;
                    }

                    // Consulta SQL para insertar el sistema
                    string sqlInsertar = @"
                    INSERT INTO Sistemas 
                    (Descripcion, IdCliente, Repositorio, Prefijo, IdUsuarioSet) 
                    VALUES 
                    (@Descripcion, @IdCliente, @Repositorio, @Prefijo, @IdUsuarioSet);";

                    var parametrosInsertar = new DynamicParameters();
                    parametrosInsertar.Add("@Descripcion", sistema.Descripcion, DbType.String);
                    parametrosInsertar.Add("@IdCliente", sistema.IdCliente, DbType.String);
                    parametrosInsertar.Add("@Repositorio", sistema.Repositorio, DbType.String);
                    parametrosInsertar.Add("@Prefijo", sistema.Prefijo, DbType.String);
                    parametrosInsertar.Add("@IdUsuarioSet", sistema.IdUsuarioSet, DbType.Int32);

                    // Ejecutar la consulta de inserción
                    int filasAfectadas = await connection.ExecuteAsync(sqlInsertar, parametrosInsertar);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "Sistema agregado exitosamente.";
                        respuesta.Data = new { SistemaId = sistema.Id }; // Puedes devolver el ID del sistema si lo necesitas
                    }
                    else
                    {
                        respuesta.Mensaje = "No se pudo agregar el sistema.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al agregar el sistema: " + ex.Message;
                respuesta.Errores.Add(ex.Message);
            }
            finally
            {
                _dapperContext.Dispose(); // Liberar recursos
            }

            return respuesta;
        }

        public async Task<RespuestaJson> EditarSistema(Sistemas sistema)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                sistema.Prefijo = sistema.Prefijo?.ToUpper();
                // Obtener la fecha y hora actual
                sistema.FechaUpd = DateTime.Now;

                // Abrir conexión y validar duplicados
                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Consulta SQL base para actualizar el sistema
                    string sqlActualizar = @"
                    UPDATE Sistemas 
                    SET Descripcion = @Descripcion, 
                        Repositorio = @Repositorio, 
                        Prefijo = @Prefijo, 
                        IdUsuarioUpd = @IdUsuarioUpd,
                        FechaUpd = @FechaUpd";

                    // Agregar IdCliente a la consulta solo si tiene un valor válido (> 0)
                    if (sistema.IdCliente > 0)
                    {
                        sqlActualizar += ", IdCliente = @IdCliente";
                    }

                    sqlActualizar += " WHERE Id = @Id;";

                    var parametrosActualizar = new DynamicParameters();
                    parametrosActualizar.Add("@Id", sistema.Id, DbType.Int32);
                    parametrosActualizar.Add("@Descripcion", sistema.Descripcion, DbType.String);
                    parametrosActualizar.Add("@Repositorio", sistema.Repositorio, DbType.String);
                    parametrosActualizar.Add("@Prefijo", sistema.Prefijo, DbType.String);
                    parametrosActualizar.Add("@IdUsuarioUpd", sistema.IdUsuarioUpd, DbType.Int32);
                    parametrosActualizar.Add("@FechaUpd", sistema.FechaUpd, DbType.DateTime);

                    // Agregar IdCliente a los parámetros solo si tiene un valor válido (> 0)
                    if (sistema.IdCliente > 0)
                    {
                        parametrosActualizar.Add("@IdCliente", sistema.IdCliente, DbType.Int32);
                    }

                    // Ejecutar la consulta de actualización
                    int filasAfectadas = await connection.ExecuteAsync(sqlActualizar, parametrosActualizar);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "Sistema actualizado exitosamente.";
                    }
                    else
                    {
                        respuesta.Mensaje = "No se pudo actualizar el sistema.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al actualizar el sistema: " + ex.Message;
                respuesta.Errores.Add(ex.Message);
            }
            finally
            {
                _dapperContext.Dispose(); // Liberar recursos
            }

            return respuesta;
        }


        public async Task<RespuestaJson> DesactivarSistema(Sistemas sistema)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Obtener la fecha y hora actual
                sistema.FechaDel = DateTime.Now;

                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Consulta SQL para desactivar el sistema
                    string sql = @"
                    UPDATE Sistemas 
                    SET FlgActivo = 0, 
                        IdUsuarioDel = @IdUsuarioDel, 
                        FechaDel = @FechaDel 
                    WHERE Id = @Id";

                    var parametros = new DynamicParameters();
                    parametros.Add("@Id", sistema.Id, DbType.Int32);
                    parametros.Add("@IdUsuarioDel", sistema.IdUsuarioDel, DbType.Int32);
                    parametros.Add("@FechaDel", sistema.FechaDel, DbType.DateTime);

                    // Ejecutar la consulta
                    int filasAfectadas = await connection.ExecuteAsync(sql, parametros);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "Sistema desactivado correctamente.";
                    }
                    else
                    {
                        respuesta.Mensaje = "No se encontró el sistema o ya está desactivado.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al desactivar el sistema: " + ex.Message;
            }
            finally
            {
                _dapperContext.Dispose();
            }
            return respuesta;
        }

        public async Task<RespuestaJson> ReactivarSistema(Sistemas sistema)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Obtener la fecha y hora actual
                sistema.FechaUpd = DateTime.Now;

                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Consulta SQL para reactivar el sistema
                    string sql = @"
                    UPDATE Sistemas 
                    SET FlgActivo = 1, 
                        IdUsuarioUpd = @IdUsuarioUpd, 
                        FechaUpd = @FechaUpd
                    WHERE Id = @Id";

                    var parametros = new DynamicParameters();
                    parametros.Add("@Id", sistema.Id, DbType.Int32);
                    parametros.Add("@IdUsuarioUpd", sistema.IdUsuarioUpd, DbType.Int32);
                    parametros.Add("@FechaUpd", sistema.FechaUpd, DbType.DateTime);

                    // Ejecutar la consulta
                    int filasAfectadas = await connection.ExecuteAsync(sql, parametros);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "Sistema reactivado correctamente.";
                    }
                    else
                    {
                        respuesta.Mensaje = "No se encontró el sistema o ya está reactivado.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al reactivar el sistema: " + ex.Message;
            }
            finally
            {
                _dapperContext.Dispose();
            }
            return respuesta;
        }

        //============================================================================================================================\\
        //====================================================   Modelo Usuarios   =================================================== \\
        //==============================================================================================================================\\

        public async Task<RespuestaJson> GetUsuarios()
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Abrir la conexión a la base de datos
                _dapperContext.AbrirConexion("SGP");
                string sql = @"SELECT 
                                s.Id, 
                                s.Usuario, 
                                s.NombreUsuario, 
                                s.ApellidoPaternoUsuario, 
                                s.ApellidoMaternoUsuario, 
                                c.Descripcion, 
                                s.IdPuesto, 
                                s.Telefono, 
                                s.Correo, 
                                s.FotoPerfil, 
                                s.FlgActivo 
                            FROM 
                                Usuarios s 
                            INNER JOIN 
                                Puestos c ON s.IdPuesto = c.Id;";
                var usuarios = await _dapperContext.QueryAsync<Usuarios>(sql);
                if (usuarios != null)
                {
                    respuesta.Resultado = true;
                    respuesta.Data = usuarios.Select(s => new Usuarios
                    {
                        Id = s.Id,
                        Usuario = s.Usuario,
                        NombreUsuario = s.NombreUsuario,
                        ApellidoPaternoUsuario = s.ApellidoPaternoUsuario,
                        ApellidoMaternoUsuario = s.ApellidoMaternoUsuario,
                        IdPuesto = s.IdPuesto,
                        Descripcion = s.Descripcion,
                        Telefono = s.Telefono,
                        Correo = s.Correo,
                        FotoPerfil = s.FotoPerfil,
                        FlgActivo = s.FlgActivo
                    }).ToList();
                }
                else
                {
                    respuesta.Mensaje = "No se encontraron usuarios activos.";
                    respuesta.Data = new List<Usuarios>(); // Inicializar Data para evitar null
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al obtener los datos de los usuarios." + ex.Message;
                respuesta.Data = new List<Usuarios>(); // Inicializar Data para evitar null
            }
            finally
            {
                // Cerrar o liberar la conexión (si es necesario)
                _dapperContext.Dispose();
            }
            return respuesta;
        }

        public async Task<RespuestaJson> AgregarUsuario(Usuarios usuario)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Validar que el archivo de imagen esté presente y sea válido
                if (usuario.FotoFile != null && usuario.FotoFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await usuario.FotoFile.CopyToAsync(memoryStream);
                        usuario.FotoPerfil = memoryStream.ToArray(); // Convierte la imagen a byte[]
                    }
                }
                else
                {
                    string rutaImagenPorDefecto = @"C:\Users\Teracrom 10\source\repos\SGPTERA\SGPTERA\wwwroot\lib\dashboard\assets\img\AvatarTeracrom.png";
                    if (System.IO.File.Exists(rutaImagenPorDefecto))
                    {
                        usuario.FotoPerfil = await System.IO.File.ReadAllBytesAsync(rutaImagenPorDefecto);
                    }
                    else
                    {
                        respuesta.Mensaje = "No se encontró la imagen por defecto.";
                        return respuesta;
                    }
                }
               
                // Eliminar caracteres no numéricos del número de teléfono
                if (!string.IsNullOrEmpty(usuario.Telefono))
                {
                    usuario.Telefono = Regex.Replace(usuario.Telefono, @"[^\d]", "");
                }

                // Abrir conexión y validar duplicados
                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Validar si el Usuario ya existe en la base de datos
                    string sqlValidarUsuario = "SELECT COUNT(*) FROM Usuarios WHERE Usuario = @Usuario;";
                    var parametrosValidarUsuario = new DynamicParameters();
                    parametrosValidarUsuario.Add("@Usuario", usuario.Usuario, DbType.String);

                    int usuarioExistente = await connection.ExecuteScalarAsync<int>(sqlValidarUsuario, parametrosValidarUsuario);

                    if (usuarioExistente > 0)
                    {
                        respuesta.Mensaje = "El nombre de usuario ya existe en la base de datos.";
                        return respuesta;
                    }

                    // Validar si el correo ya existe en la base de datos
                    string sqlValidarCorreo = "SELECT COUNT(*) FROM Usuarios WHERE Correo = @Correo;";
                    var parametrosValidarCorreo = new DynamicParameters();
                    parametrosValidarCorreo.Add("@Correo", usuario.Correo, DbType.String);

                    int correoExistente = await connection.ExecuteScalarAsync<int>(sqlValidarCorreo, parametrosValidarCorreo);

                    if (correoExistente > 0)
                    {
                        respuesta.Mensaje = "El correo ya existe en la base de datos.";
                        return respuesta;
                    }

                    // Validar si el número de teléfono ya existe en la base de datos
                    string sqlValidarTelefono = "SELECT COUNT(*) FROM Usuarios WHERE Telefono = @Telefono;";
                    var parametrosValidarTelefono = new DynamicParameters();
                    parametrosValidarTelefono.Add("@Telefono", usuario.Telefono, DbType.String);

                    int telefonoExistente = await connection.ExecuteScalarAsync<int>(sqlValidarTelefono, parametrosValidarTelefono);

                    if (telefonoExistente > 0)
                    {
                        respuesta.Mensaje = "El número de teléfono ya existe en la base de datos.";
                        return respuesta;
                    }

                    // Consulta SQL para insertar el usuario
                    string sqlInsertar = @"
                    INSERT INTO Usuarios 
                    (Usuario, NombreUsuario, ApellidoPaternoUsuario, ApellidoMaternoUsuario, IdPuesto, Telefono, Correo, Contrasena, FotoPerfil, IdUsuarioSet) 
                    VALUES 
                    (@Usuario, @NombreUsuario, @ApellidoPaternoUsuario, @ApellidoMaternoUsuario, @IdPuesto, @Telefono, @Correo, 
                    HASHBYTES('SHA2_256', CONVERT(NVARCHAR(50), @Contrasena)), @FotoPerfil, @IdUsuarioSet);";

                    var parametrosInsertar = new DynamicParameters();
                    parametrosInsertar.Add("@Usuario", usuario.Usuario, DbType.String);
                    parametrosInsertar.Add("@NombreUsuario", usuario.NombreUsuario, DbType.String);
                    parametrosInsertar.Add("@ApellidoPaternoUsuario", usuario.ApellidoPaternoUsuario, DbType.String);
                    parametrosInsertar.Add("@ApellidoMaternoUsuario", usuario.ApellidoMaternoUsuario, DbType.String);
                    parametrosInsertar.Add("@IdPuesto", usuario.IdPuesto, DbType.String);
                    parametrosInsertar.Add("@Telefono", usuario.Telefono, DbType.String);
                    parametrosInsertar.Add("@Correo", usuario.Correo, DbType.String);
                    parametrosInsertar.Add("@Contrasena", usuario.Contrasena, DbType.String);
                    parametrosInsertar.Add("@FotoPerfil", usuario.FotoPerfil, DbType.Binary);
                    parametrosInsertar.Add("@IdUsuarioSet", usuario.IdUsuarioSet, DbType.Int32);

                    // Ejecutar la consulta de inserción
                    int filasAfectadas = await connection.ExecuteAsync(sqlInsertar, parametrosInsertar);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "Usuario agregado exitosamente.";
                        respuesta.Data = new { UsuarioId = usuario.Id }; // Puedes devolver el ID del usuario si lo necesitas
                    }
                    else
                    {
                        respuesta.Mensaje = "No se pudo agregar el usuario.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al agregar el usuario: " + ex.Message;
                respuesta.Errores.Add(ex.Message);
            }
            finally
            {
                _dapperContext.Dispose(); // Liberar recursos
            }

            return respuesta;
        }

        public async Task<RespuestaJson> EditarUsuario(Usuarios usuario)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Validar que el archivo de imagen esté presente y sea válido
                if (usuario.FotoFile != null && usuario.FotoFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await usuario.FotoFile.CopyToAsync(memoryStream);
                        usuario.FotoPerfil = memoryStream.ToArray(); // Convierte la imagen a byte[]
                    }
                }
                else if (!string.IsNullOrEmpty(usuario.FotoBase64))
                {
                    // Si no hay una nueva imagen, pero hay una imagen existente en base64, convertirla a byte[]
                    usuario.FotoPerfil = Convert.FromBase64String(usuario.FotoBase64.Split(',')[1]);
                }

                // Eliminar caracteres no numéricos del número de teléfono
                if (!string.IsNullOrEmpty(usuario.Telefono))
                {
                    usuario.Telefono = Regex.Replace(usuario.Telefono, @"[^\d]", "");
                }

                // Obtener la fecha y hora actual
                usuario.FechaUpd = DateTime.Now;

                // Abrir conexión y validar duplicados
                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Consulta SQL base para actualizar el usuario
                    string sqlActualizar = @"
                    UPDATE Usuarios 
                    SET Usuario = @Usuario, 
                        NombreUsuario = @NombreUsuario, 
                        ApellidoPaternoUsuario = @ApellidoPaternoUsuario, 
                        ApellidoMaternoUsuario = @ApellidoMaternoUsuario,
                        Telefono = @Telefono,
                        Correo = @Correo,
                        FotoPerfil = @FotoPerfil,
                        IdUsuarioUpd = @IdUsuarioUpd,
                        FechaUpd = @FechaUpd";

                    // Agregar IdPuesto a la consulta si tiene un valor válido (> 0)
                    if (usuario.IdPuesto > 0)
                    {
                        sqlActualizar += ", IdPuesto = @IdPuesto";
                    }

                    // Agregar Contrasena a la consulta si no es nula o vacía
                    if (!string.IsNullOrEmpty(usuario.Contrasena))
                    {
                        sqlActualizar += ", Contrasena = CONVERT(NVARCHAR(50), @Contrasena))";
                    }

                    sqlActualizar += " WHERE Id = @Id;";

                    var parametrosActualizar = new DynamicParameters();
                    parametrosActualizar.Add("@Id", usuario.Id, DbType.Int32);
                    parametrosActualizar.Add("@Usuario", usuario.Usuario, DbType.String);
                    parametrosActualizar.Add("@NombreUsuario", usuario.NombreUsuario, DbType.String);
                    parametrosActualizar.Add("@ApellidoPaternoUsuario", usuario.ApellidoPaternoUsuario, DbType.String);
                    parametrosActualizar.Add("@ApellidoMaternoUsuario", usuario.ApellidoMaternoUsuario, DbType.String);
                    parametrosActualizar.Add("@Telefono", usuario.Telefono, DbType.String);
                    parametrosActualizar.Add("@Correo", usuario.Correo, DbType.String);
                    parametrosActualizar.Add("@FotoPerfil", usuario.FotoPerfil, DbType.Binary);
                    parametrosActualizar.Add("@IdUsuarioUpd", usuario.IdUsuarioUpd, DbType.Int32);
                    parametrosActualizar.Add("@FechaUpd", usuario.FechaUpd, DbType.DateTime);

                    // Agregar IdPuesto a los parámetros si tiene un valor válido (> 0)
                    if (usuario.IdPuesto > 0)
                    {
                        parametrosActualizar.Add("@IdPuesto", usuario.IdPuesto, DbType.Int32);
                    }

                    // Agregar Contrasena a los parámetros si no es nula o vacía
                    if (!string.IsNullOrEmpty(usuario.Contrasena))
                    {
                        parametrosActualizar.Add("@Contrasena", usuario.Contrasena, DbType.String);
                    }

                    // Ejecutar la consulta de actualización
                    int filasAfectadas = await connection.ExecuteAsync(sqlActualizar, parametrosActualizar);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "Usuario actualizado exitosamente.";
                    }
                    else
                    {
                        respuesta.Mensaje = "No se pudo actualizar el usuario.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al actualizar el usuario: " + ex.Message;
                respuesta.Errores.Add(ex.Message);
            }
            finally
            {
                _dapperContext.Dispose(); // Liberar recursos
            }

            return respuesta;
        }

        public async Task<RespuestaJson> DesactivarUsuario(Usuarios usuario)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Obtener la fecha y hora actual
                usuario.FechaDel = DateTime.Now;

                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Consulta SQL para desactivar el usuario
                    string sql = @"
                    UPDATE Usuarios 
                    SET FlgActivo = 0, 
                        IdUsuarioDel = @IdUsuarioDel, 
                        FechaDel = @FechaDel 
                    WHERE Id = @Id";

                    var parametros = new DynamicParameters();
                    parametros.Add("@Id", usuario.Id, DbType.Int32);
                    parametros.Add("@IdUsuarioDel", usuario.IdUsuarioDel, DbType.Int32);
                    parametros.Add("@FechaDel", usuario.FechaDel, DbType.DateTime);

                    // Ejecutar la consulta
                    int filasAfectadas = await connection.ExecuteAsync(sql, parametros);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "Usuario desactivado correctamente.";
                    }
                    else
                    {
                        respuesta.Mensaje = "No se encontró el usuario o ya está desactivado.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al desactivar el usuario: " + ex.Message;
            }
            finally
            {
                _dapperContext.Dispose();
            }
            return respuesta;
        }

        public async Task<RespuestaJson> ReactivarUsuario(Usuarios usuario)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Obtener la fecha y hora actual
                usuario.FechaUpd = DateTime.Now;

                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Consulta SQL para desactivar el usuario
                    string sql = @"
                    UPDATE Usuarios 
                    SET FlgActivo = 1, 
                        IdUsuarioUpd = @IdUsuarioUpd, 
                        FechaUpd = @FechaUpd
                    WHERE Id = @Id";

                    var parametros = new DynamicParameters();
                    parametros.Add("@Id", usuario.Id, DbType.Int32);
                    parametros.Add("@IdUsuarioUpd", usuario.IdUsuarioUpd, DbType.Int32);
                    parametros.Add("@FechaUpd", usuario.FechaUpd, DbType.DateTime);

                    // Ejecutar la consulta
                    int filasAfectadas = await connection.ExecuteAsync(sql, parametros);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "Usuario reactivado correctamente.";
                    }
                    else
                    {
                        respuesta.Mensaje = "No se encontró el usuario o ya está reactivado.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al reactivar el usuario: " + ex.Message;
            }
            finally
            {
                _dapperContext.Dispose();
            }
            return respuesta;
        }

        //============================================================================================================================\\
        //====================================================   Modelo ClientesUsuarios   =========================================== \\
        //==============================================================================================================================\\

        public async Task<RespuestaJson> GetClientesUsuarios()
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Abrir la conexión a la base de datos
                _dapperContext.AbrirConexion("SGP");
                string sql = @"SELECT 
                s.Id, 
                s.ClienteUsuario, 
                s.NombreClienteUsuario, 
                s.ApellidoPaternoClienteUsuario, 
                s.ApellidoMaternoClienteUsuario, 
                c.Descripcion, 
                s.IdPuesto,
                s.Telefono, 
                s.Correo, 
                cli.RazonSocial,
                s.IdCliente,
                s.FotoPerfil, 
                s.FlgActivo
            FROM 
                ClientesUsuarios s 
            INNER JOIN 
                Puestos c ON s.IdPuesto = c.Id
            INNER JOIN 
                Clientes cli ON s.IdCliente = cli.Id;"; 
                var clientesusuarios = await _dapperContext.QueryAsync<ClientesUsuarios>(sql);
                if (clientesusuarios != null)
                {
                    respuesta.Resultado = true;
                    respuesta.Data = clientesusuarios.Select(s => new ClientesUsuarios
                    {
                        Id = s.Id,
                        ClienteUsuario = s.ClienteUsuario,
                        NombreClienteUsuario = s.NombreClienteUsuario,
                        ApellidoPaternoClienteUsuario = s.ApellidoPaternoClienteUsuario,
                        ApellidoMaternoClienteUsuario = s.ApellidoMaternoClienteUsuario,
                        Descripcion = s.Descripcion,
                        IdPuesto = s.IdPuesto,
                        Telefono = s.Telefono,
                        Correo = s.Correo,
                        FotoPerfil = s.FotoPerfil,
                        FlgActivo = s.FlgActivo,
                        RazonSocial = s.RazonSocial,  // Asegúrate de que la clase ClientesUsuarios tenga esta propiedad
                        IdCliente = s.IdCliente
                    }).ToList();
                }
                else
                {
                    respuesta.Mensaje = "No se encontraron clientes usuarios activos.";
                    respuesta.Data = new List<ClientesUsuarios>(); // Inicializar Data para evitar null
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al obtener los datos de los clientes usuarios." + ex.Message;
                respuesta.Data = new List<ClientesUsuarios>(); // Inicializar Data para evitar null
            }
            finally
            {
                // Cerrar o liberar la conexión (si es necesario)
                _dapperContext.Dispose();
            }
            return respuesta;
        }

        public async Task<RespuestaJson> AgregarClienteUsuario(ClientesUsuarios clienteusuario)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Validar que el archivo de imagen esté presente y sea válido
                if (clienteusuario.FotoFile != null && clienteusuario.FotoFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await clienteusuario.FotoFile.CopyToAsync(memoryStream);
                        clienteusuario.FotoPerfil = memoryStream.ToArray(); // Convierte la imagen a byte[]
                    }
                }
                else
                {
                    string rutaImagenPorDefecto = @"C:\Users\Teracrom 10\source\repos\SGPTERA\SGPTERA\wwwroot\lib\dashboard\assets\img\AvatarTeracromGreen.png";
                    if (System.IO.File.Exists(rutaImagenPorDefecto))
                    {
                        clienteusuario.FotoPerfil = await System.IO.File.ReadAllBytesAsync(rutaImagenPorDefecto);
                    }
                    else
                    {
                        respuesta.Mensaje = "No se encontró la imagen por defecto.";
                        return respuesta;
                    }
                }

                // Eliminar caracteres no numéricos del número de teléfono
                if (!string.IsNullOrEmpty(clienteusuario.Telefono))
                {
                    clienteusuario.Telefono = Regex.Replace(clienteusuario.Telefono, @"[^\d]", "");
                }

                // Abrir conexión y validar duplicados
                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Validar si el ClienteUsuario ya existe en la base de datos
                    string sqlValidarClienteUsuario = "SELECT COUNT(*) FROM ClientesUsuarios WHERE ClienteUsuario = @ClienteUsuario;";
                    var parametrosValidarClienteUsuario = new DynamicParameters();
                    parametrosValidarClienteUsuario.Add("@ClienteUsuario", clienteusuario.ClienteUsuario, DbType.String);

                    int clienteusuarioExistente = await connection.ExecuteScalarAsync<int>(sqlValidarClienteUsuario, parametrosValidarClienteUsuario);

                    if (clienteusuarioExistente > 0)
                    {
                        respuesta.Mensaje = "El nombre de cliente usuario ya existe en la base de datos.";
                        return respuesta;
                    }

                    // Validar si el correo ya existe en la base de datos
                    string sqlValidarCorreo = "SELECT COUNT(*) FROM ClientesUsuarios WHERE Correo = @Correo;";
                    var parametrosValidarCorreo = new DynamicParameters();
                    parametrosValidarCorreo.Add("@Correo", clienteusuario.Correo, DbType.String);

                    int correoExistente = await connection.ExecuteScalarAsync<int>(sqlValidarCorreo, parametrosValidarCorreo);

                    if (correoExistente > 0)
                    {
                        respuesta.Mensaje = "El correo ya existe en la base de datos.";
                        return respuesta;
                    }

                    // Validar si el número de teléfono ya existe en la base de datos
                    string sqlValidarTelefono = "SELECT COUNT(*) FROM ClientesUsuarios WHERE Telefono = @Telefono;";
                    var parametrosValidarTelefono = new DynamicParameters();
                    parametrosValidarTelefono.Add("@Telefono", clienteusuario.Telefono, DbType.String);

                    int telefonoExistente = await connection.ExecuteScalarAsync<int>(sqlValidarTelefono, parametrosValidarTelefono);

                    if (telefonoExistente > 0)
                    {
                        respuesta.Mensaje = "El número de teléfono ya existe en la base de datos.";
                        return respuesta;
                    }

                    // Consulta SQL para insertar el cliente usuario
                    string sqlInsertar = @"
                    INSERT INTO ClientesUsuarios 
                    (ClienteUsuario, NombreClienteUsuario, ApellidoPaternoClienteUsuario, ApellidoMaternoClienteUsuario, IdPuesto, Telefono, Correo, Contrasena, IdCliente, FotoPerfil, IdUsuarioSet) 
                    VALUES 
                    (@ClienteUsuario, @NombreClienteUsuario, @ApellidoPaternoClienteUsuario, @ApellidoMaternoClienteUsuario, @IdPuesto, @Telefono, @Correo, 
                    HASHBYTES('SHA2_256', CONVERT(NVARCHAR(50), @Contrasena)), @IdCliente, @FotoPerfil, @IdUsuarioSet);";

                    var parametrosInsertar = new DynamicParameters();
                    parametrosInsertar.Add("@ClienteUsuario", clienteusuario.ClienteUsuario, DbType.String);
                    parametrosInsertar.Add("@NombreClienteUsuario", clienteusuario.NombreClienteUsuario, DbType.String);
                    parametrosInsertar.Add("@ApellidoPaternoClienteUsuario", clienteusuario.ApellidoPaternoClienteUsuario, DbType.String);
                    parametrosInsertar.Add("@ApellidoMaternoClienteUsuario", clienteusuario.ApellidoMaternoClienteUsuario, DbType.String);
                    parametrosInsertar.Add("@IdPuesto", clienteusuario.IdPuesto, DbType.String);
                    parametrosInsertar.Add("@Telefono", clienteusuario.Telefono, DbType.String);
                    parametrosInsertar.Add("@Correo", clienteusuario.Correo, DbType.String);
                    parametrosInsertar.Add("@Contrasena", clienteusuario.Contrasena, DbType.String);
                    parametrosInsertar.Add("@IdCliente", clienteusuario.IdCliente, DbType.String);
                    parametrosInsertar.Add("@FotoPerfil", clienteusuario.FotoPerfil, DbType.Binary);
                    parametrosInsertar.Add("@IdUsuarioSet", clienteusuario.IdUsuarioSet, DbType.Int32);

                    // Ejecutar la consulta de inserción
                    int filasAfectadas = await connection.ExecuteAsync(sqlInsertar, parametrosInsertar);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "Cliente usuario agregado exitosamente.";
                        respuesta.Data = new { ClienteUsuarioId = clienteusuario.Id }; // Puedes devolver el ID del cliente usuario si lo necesitas
                    }
                    else
                    {
                        respuesta.Mensaje = "No se pudo agregar el cliente usuario.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al agregar el cliente usuario: " + ex.Message;
                respuesta.Errores.Add(ex.Message);
            }
            finally
            {
                _dapperContext.Dispose(); // Liberar recursos
            }

            return respuesta;
        }

        public async Task<RespuestaJson> EditarClienteUsuario(ClientesUsuarios clienteusuario)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Validar que el archivo de imagen esté presente y sea válido
                if (clienteusuario.FotoFile != null && clienteusuario.FotoFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await clienteusuario.FotoFile.CopyToAsync(memoryStream);
                        clienteusuario.FotoPerfil = memoryStream.ToArray(); // Convierte la imagen a byte[]
                    }
                }
                else if (!string.IsNullOrEmpty(clienteusuario.FotoBase64))
                {
                    // Si no hay una nueva imagen, pero hay una imagen existente en base64, convertirla a byte[]
                    clienteusuario.FotoPerfil = Convert.FromBase64String(clienteusuario.FotoBase64.Split(',')[1]);
                }

                // Eliminar caracteres no numéricos del número de teléfono
                if (!string.IsNullOrEmpty(clienteusuario.Telefono))
                {
                    clienteusuario.Telefono = Regex.Replace(clienteusuario.Telefono, @"[^\d]", "");
                }

                // Obtener la fecha y hora actual
                clienteusuario.FechaUpd = DateTime.Now;

                // Abrir conexión y validar duplicados
                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Consulta SQL base para actualizar el cliente usuario
                    string sqlActualizar = @"
                    UPDATE ClientesUsuarios 
                    SET ClienteUsuario = @ClienteUsuario, 
                        NombreClienteUsuario = @NombreClienteUsuario, 
                        ApellidoPaternoClienteUsuario = @ApellidoPaternoClienteUsuario, 
                        ApellidoMaternoClienteUsuario = @ApellidoMaternoClienteUsuario,
                        Telefono = @Telefono,
                        Correo = @Correo,
                        FotoPerfil = @FotoPerfil,
                        IdUsuarioUpd = @IdUsuarioUpd,
                        FechaUpd = @FechaUpd";

                    // Agregar IdPuesto a la consulta si tiene un valor válido (> 0)
                    if (clienteusuario.IdPuesto > 0)
                    {
                        sqlActualizar += ", IdPuesto = @IdPuesto";
                    }
                    if (clienteusuario.IdCliente > 0)
                    {
                        sqlActualizar += ", IdCliente = @IdCliente";
                    }

                    // Agregar Contrasena a la consulta si no es nula o vacía
                    if (!string.IsNullOrEmpty(clienteusuario.Contrasena))
                    {
                        sqlActualizar += ", Contrasena = @Contrasena";
                    }

                    sqlActualizar += " WHERE Id = @Id;";

                    var parametrosActualizar = new DynamicParameters();
                    parametrosActualizar.Add("@Id", clienteusuario.Id, DbType.Int32);
                    parametrosActualizar.Add("@ClienteUsuario", clienteusuario.ClienteUsuario, DbType.String);
                    parametrosActualizar.Add("@NombreClienteUsuario", clienteusuario.NombreClienteUsuario, DbType.String);
                    parametrosActualizar.Add("@ApellidoPaternoClienteUsuario", clienteusuario.ApellidoPaternoClienteUsuario, DbType.String);
                    parametrosActualizar.Add("@ApellidoMaternoClienteUsuario", clienteusuario.ApellidoMaternoClienteUsuario, DbType.String);
                    parametrosActualizar.Add("@Telefono", clienteusuario.Telefono, DbType.String);
                    parametrosActualizar.Add("@Correo", clienteusuario.Correo, DbType.String);
                    parametrosActualizar.Add("@FotoPerfil", clienteusuario.FotoPerfil, DbType.Binary);
                    parametrosActualizar.Add("@IdUsuarioUpd", clienteusuario.IdUsuarioUpd, DbType.Int32);
                    parametrosActualizar.Add("@FechaUpd", clienteusuario.FechaUpd, DbType.DateTime);

                    // Agregar IdPuesto a los parámetros si tiene un valor válido (> 0)
                    if (clienteusuario.IdPuesto > 0)
                    {
                        parametrosActualizar.Add("@IdPuesto", clienteusuario.IdPuesto, DbType.Int32);
                    }
                    if (clienteusuario.IdCliente > 0)
                    {
                        parametrosActualizar.Add("@IdCliente", clienteusuario.IdCliente, DbType.Int32);
                    }

                    // Agregar Contrasena a los parámetros si no es nula o vacía
                    if (!string.IsNullOrEmpty(clienteusuario.Contrasena))
                    {
                        parametrosActualizar.Add("@Contrasena", clienteusuario.Contrasena, DbType.String);
                    }

                    // Ejecutar la consulta de actualización
                    int filasAfectadas = await connection.ExecuteAsync(sqlActualizar, parametrosActualizar);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "Cliente usuario actualizado exitosamente.";
                    }
                    else
                    {
                        respuesta.Mensaje = "No se pudo actualizar el cliente usuario.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al actualizar el cliente usuario: " + ex.Message;
                respuesta.Errores.Add(ex.Message);
            }
            finally
            {
                _dapperContext.Dispose(); // Liberar recursos
            }

            return respuesta;
        }

        public async Task<RespuestaJson> DesactivarClienteUsuario(ClientesUsuarios clienteusuario)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Obtener la fecha y hora actual
                clienteusuario.FechaDel = DateTime.Now;

                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Consulta SQL para desactivar el cliente usuario
                    string sql = @"
            UPDATE ClientesUsuarios 
            SET FlgActivo = 0, 
                IdUsuarioDel = @IdUsuarioDel, 
                FechaDel = @FechaDel 
            WHERE Id = @Id";

                    var parametros = new DynamicParameters();
                    parametros.Add("@Id", clienteusuario.Id, DbType.Int32);
                    parametros.Add("@IdUsuarioDel", clienteusuario.IdUsuarioDel, DbType.Int32);
                    parametros.Add("@FechaDel", clienteusuario.FechaDel, DbType.DateTime);

                    // Ejecutar la consulta
                    int filasAfectadas = await connection.ExecuteAsync(sql, parametros);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "Cliente usuario desactivado correctamente.";
                    }
                    else
                    {
                        respuesta.Mensaje = "No se encontró el cliente usuario o ya está desactivado.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al desactivar el cliente usuario: " + ex.Message;
            }
            finally
            {
                _dapperContext.Dispose();
            }
            return respuesta;
        }

        public async Task<RespuestaJson> ReactivarClienteUsuario(ClientesUsuarios clienteusuario)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Obtener la fecha y hora actual
                clienteusuario.FechaUpd = DateTime.Now;

                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Consulta SQL para reactivar el cliente usuario
                    string sql = @"
                    UPDATE ClientesUsuarios 
                    SET FlgActivo = 1, 
                        IdUsuarioUpd = @IdUsuarioUpd, 
                        FechaUpd = @FechaUpd
                    WHERE Id = @Id";

                    var parametros = new DynamicParameters();
                    parametros.Add("@Id", clienteusuario.Id, DbType.Int32);
                    parametros.Add("@IdUsuarioUpd", clienteusuario.IdUsuarioUpd, DbType.Int32);
                    parametros.Add("@FechaUpd", clienteusuario.FechaUpd, DbType.DateTime);

                    // Ejecutar la consulta
                    int filasAfectadas = await connection.ExecuteAsync(sql, parametros);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "Cliente usuario reactivado correctamente.";
                    }
                    else
                    {
                        respuesta.Mensaje = "No se encontró el cliente usuario o ya está reactivado.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al reactivar el cliente usuario: " + ex.Message;
            }
            finally
            {
                _dapperContext.Dispose();
            }
            return respuesta;
        }

        //============================================================================================================================\\
        //=================================================   Modelo ModuloSistemas   =================================================\\
        //==============================================================================================================================\\
        public async Task<RespuestaJson> GetModuloSistemas()
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Abrir la conexión a la base de datos
                _dapperContext.AbrirConexion("SGP");

                string sql = @"
                SELECT 
                    s.Id, 
                    s.ModuloSistemaDescripcion, 
                    c.Descripcion, 
                    s.Detalles,
                    s.IdSistema,
                    s.FlgActivo 
                FROM 
                    ModuloSistemas s
                INNER JOIN 
                    Sistemas c ON s.IdSistema = c.Id";

                var modulosistemas = await _dapperContext.QueryAsync<ModuloSistemas>(sql);
                if (modulosistemas != null && modulosistemas.Any())
                {
                    respuesta.Resultado = true;
                    respuesta.Data = modulosistemas.Select(s => new ModuloSistemas
                    {
                        Id = s.Id,
                        ModuloSistemaDescripcion = s.ModuloSistemaDescripcion,
                        Descripcion = s.Descripcion,
                        Detalles = s.Detalles,
                        IdSistema = s.IdSistema,
                        FlgActivo = s.FlgActivo
                    }).ToList();
                }
                else
                {
                    respuesta.Mensaje = "No se encontraron modulosistemas activos.";
                    respuesta.Data = new List<ModuloSistemas>(); // Inicializar Data para evitar null
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al obtener los datos de los modulosistemas: " + ex.Message;
                respuesta.Data = new List<ModuloSistemas>(); // Inicializar Data para evitar null
            }
            finally
            {
                // Cerrar o liberar la conexión (si es necesario)
                _dapperContext.Dispose();
            }
            return respuesta;
        }

        public async Task<RespuestaJson> AgregarModuloSistema(ModuloSistemas modulosistema)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {

                // Abrir conexión y validar duplicados
                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {

                    // Validar si ya existe un registro con la misma combinación de ModuloSistemaDescripcion e IdSistema
                    string sqlValidarModuloSistemas = "SELECT COUNT(*) FROM ModuloSistemas WHERE ModuloSistemaDescripcion = @ModuloSistemaDescripcion AND IdSistema = @IdSistema;";
                    var parametrosValidarModuloSistemas = new DynamicParameters();
                    parametrosValidarModuloSistemas.Add("@ModuloSistemaDescripcion", modulosistema.ModuloSistemaDescripcion, DbType.String);
                    parametrosValidarModuloSistemas.Add("@IdSistema", modulosistema.IdSistema, DbType.Int32);

                    int registrosExistentes = await connection.ExecuteScalarAsync<int>(sqlValidarModuloSistemas, parametrosValidarModuloSistemas);

                    if (registrosExistentes > 0)
                    {
                        respuesta.Mensaje = "Ya existe este módulo para ese sistema.";
                        return respuesta;
                    }

                    // Consulta SQL para insertar el modulosistema
                    string sqlInsertar = @"
                    INSERT INTO ModuloSistemas 
                    (ModuloSistemaDescripcion, Detalles, IdSistema, IdUsuarioSet) 
                    VALUES 
                    (@ModuloSistemaDescripcion, @Detalles, @IdSistema, @IdUsuarioSet);";

                    var parametrosInsertar = new DynamicParameters();
                    parametrosInsertar.Add("@ModuloSistemaDescripcion", modulosistema.ModuloSistemaDescripcion, DbType.String);
                    parametrosInsertar.Add("@Detalles", modulosistema.Detalles, DbType.String);
                    parametrosInsertar.Add("@IdSistema", modulosistema.IdSistema, DbType.Int32);
                    parametrosInsertar.Add("@IdUsuarioSet", modulosistema.IdUsuarioSet, DbType.Int32);

                    
                    // Ejecutar la consulta de inserción
                    int filasAfectadas = await connection.ExecuteAsync(sqlInsertar, parametrosInsertar);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "El módulo del sistema se agregó exitosamente.";
                        respuesta.Data = new { ModuloSistemaId = modulosistema.Id }; // Puedes devolver el ID del modulosistema si lo necesitas
                    }
                    else
                    {
                        respuesta.Mensaje = "Hubo un error al agregar el módulo del sistema.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Se produjo un error al intentar agregar el módulo del sistema: " + ex.Message;
                respuesta.Errores.Add(ex.Message);
            }
            finally
            {
                _dapperContext.Dispose(); // Liberar recursos
            }

            return respuesta;
        }

        public async Task<RespuestaJson> EditarModuloSistema(ModuloSistemas modulosistema)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Obtener la fecha y hora actual
                modulosistema.FechaUpd = DateTime.Now;

                // Abrir conexión y validar duplicados
                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Obtener el valor actual de ModuloSistemaDescripcion e IdSistema desde la base de datos
                    string sqlObtenerDatosActuales = @"
                    SELECT ModuloSistemaDescripcion, IdSistema 
                    FROM ModuloSistemas 
                    WHERE Id = @Id;";

                    var parametrosObtenerDatos = new DynamicParameters();
                    parametrosObtenerDatos.Add("@Id", modulosistema.Id, DbType.Int32);

                    var datosActuales = await connection.QueryFirstOrDefaultAsync<(string ModuloSistemaDescripcion, int IdSistema)>(sqlObtenerDatosActuales, parametrosObtenerDatos);

                    // Validar duplicados solo si ModuloSistemaDescripcion o IdSistema están siendo modificados
                    if (datosActuales.ModuloSistemaDescripcion != modulosistema.ModuloSistemaDescripcion || datosActuales.IdSistema != modulosistema.IdSistema)
                    {
                        string sqlValidarModuloSistemas = @"
                        SELECT COUNT(*) 
                        FROM ModuloSistemas 
                        WHERE ModuloSistemaDescripcion = @ModuloSistemaDescripcion 
                          AND IdSistema = @IdSistema 
                          AND Id != @Id;"; // Excluir el registro actual de la validación

                        var parametrosValidarModuloSistemas = new DynamicParameters();
                        parametrosValidarModuloSistemas.Add("@ModuloSistemaDescripcion", modulosistema.ModuloSistemaDescripcion, DbType.String);
                        parametrosValidarModuloSistemas.Add("@IdSistema", modulosistema.IdSistema, DbType.Int32);
                        parametrosValidarModuloSistemas.Add("@Id", modulosistema.Id, DbType.Int32);

                        int registrosExistentes = await connection.ExecuteScalarAsync<int>(sqlValidarModuloSistemas, parametrosValidarModuloSistemas);

                        if (registrosExistentes > 0)
                        {
                            respuesta.Mensaje = "Ya existe un módulo con la misma descripción para este sistema.";
                            return respuesta;
                        }
                    }

                    // Construir la consulta SQL dinámicamente
                    var sqlActualizar = new StringBuilder("UPDATE ModuloSistemas SET ");
                    var parametrosActualizar = new DynamicParameters();

                    if (datosActuales.ModuloSistemaDescripcion != modulosistema.ModuloSistemaDescripcion)
                    {
                        sqlActualizar.Append("ModuloSistemaDescripcion = @ModuloSistemaDescripcion, ");
                        parametrosActualizar.Add("@ModuloSistemaDescripcion", modulosistema.ModuloSistemaDescripcion, DbType.String);
                    }

                    if (!string.IsNullOrEmpty(modulosistema.Detalles))
                    {
                        sqlActualizar.Append("Detalles = @Detalles, ");
                        parametrosActualizar.Add("@Detalles", modulosistema.Detalles, DbType.String);
                    }

                    if (datosActuales.IdSistema != modulosistema.IdSistema)
                    {
                        sqlActualizar.Append("IdSistema = @IdSistema, ");
                        parametrosActualizar.Add("@IdSistema", modulosistema.IdSistema, DbType.Int32);
                    }

                    sqlActualizar.Append("IdUsuarioUpd = @IdUsuarioUpd, FechaUpd = @FechaUpd ");
                    sqlActualizar.Append("WHERE Id = @Id;");

                    parametrosActualizar.Add("@Id", modulosistema.Id, DbType.Int32);
                    parametrosActualizar.Add("@IdUsuarioUpd", modulosistema.IdUsuarioUpd, DbType.Int32);
                    parametrosActualizar.Add("@FechaUpd", modulosistema.FechaUpd, DbType.DateTime);

                    int filasAfectadas = await connection.ExecuteAsync(sqlActualizar.ToString(), parametrosActualizar);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "Módulo del sistema actualizado con éxito.";
                    }
                    else
                    {
                        respuesta.Mensaje = "No fue posible actualizar el módulo del sistema.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Se produjo un error al actualizar el módulo del sistema: " + ex.Message;
                respuesta.Errores.Add(ex.Message);
            }
            finally
            {
                _dapperContext.Dispose(); // Liberar recursos
            }

            return respuesta;
        }

        public async Task<RespuestaJson> DesactivarModuloSistema(ModuloSistemas modulosistema)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Obtener la fecha y hora actual
                modulosistema.FechaDel = DateTime.Now;

                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Consulta SQL para desactivar el modulosistema
                    string sql = @"
                    UPDATE ModuloSistemas 
                    SET FlgActivo = 0, 
                        IdUsuarioDel = @IdUsuarioDel, 
                        FechaDel = @FechaDel 
                    WHERE Id = @Id";

                    var parametros = new DynamicParameters();
                    parametros.Add("@Id", modulosistema.Id, DbType.Int32);
                    parametros.Add("@IdUsuarioDel", modulosistema.IdUsuarioDel, DbType.Int32);
                    parametros.Add("@FechaDel", modulosistema.FechaDel, DbType.DateTime);

                    // Ejecutar la consulta
                    int filasAfectadas = await connection.ExecuteAsync(sql, parametros);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "ModuloSistema desactivado correctamente.";
                    }
                    else
                    {
                        respuesta.Mensaje = "No se encontró el modulosistema o ya está desactivado.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al desactivar el modulosistema: " + ex.Message;
            }
            finally
            {
                _dapperContext.Dispose();
            }
            return respuesta;
        }

        public async Task<RespuestaJson> ReactivarModuloSistema(ModuloSistemas modulosistema)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Obtener la fecha y hora actual
                modulosistema.FechaUpd = DateTime.Now;

                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Consulta SQL para reactivar el modulosistema
                    string sql = @"
                    UPDATE ModuloSistemas 
                    SET FlgActivo = 1, 
                        IdUsuarioUpd = @IdUsuarioUpd, 
                        FechaUpd = @FechaUpd
                    WHERE Id = @Id";

                    var parametros = new DynamicParameters();
                    parametros.Add("@Id", modulosistema.Id, DbType.Int32);
                    parametros.Add("@IdUsuarioUpd", modulosistema.IdUsuarioUpd, DbType.Int32);
                    parametros.Add("@FechaUpd", modulosistema.FechaUpd, DbType.DateTime);

                    // Ejecutar la consulta
                    int filasAfectadas = await connection.ExecuteAsync(sql, parametros);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "ModuloSistema reactivado correctamente.";
                    }
                    else
                    {
                        respuesta.Mensaje = "No se encontró el modulosistema o ya está reactivado.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al reactivar el modulosistema: " + ex.Message;
            }
            finally
            {
                _dapperContext.Dispose();
            }
            return respuesta;
        }

        //============================================================================================================================\\
        //================================================   Modelo EstatusProyectos   ================================================\\
        //==============================================================================================================================\\

        public async Task<RespuestaJson> GetEstatusProyectos()
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Abrir la conexión a la base de datos
                _dapperContext.AbrirConexion("SGP");

                string sql = "SELECT Id, EstatusProyectosDescripcion, Detalles, FlgActivo FROM EstatusProyectos";

                var estatusproyectos = await _dapperContext.QueryAsync<EstatusProyectos>(sql);
                if (estatusproyectos != null)
                {
                    respuesta.Resultado = true;
                    respuesta.Data = estatusproyectos.Select(s => new EstatusProyectos
                    {
                        Id = s.Id,
                        EstatusProyectosDescripcion = s.EstatusProyectosDescripcion,
                        Detalles = s.Detalles,
                        FlgActivo = s.FlgActivo
                    }).ToList();
                }
                else
                {
                    respuesta.Mensaje = "No se encontraron estatusproyectos activos.";
                    respuesta.Data = new List<EstatusProyectos>(); // Inicializar Data para evitar null
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al obtener los datos de los estatusproyectos." + ex.Message;
                respuesta.Data = new List<EstatusProyectos>(); // Inicializar Data para evitar null
            }
            finally
            {
                // Cerrar o liberar la conexión (si es necesario)
                _dapperContext.Dispose();
            }
            return respuesta;
        }

        public async Task<RespuestaJson> AgregarEstatusProyecto(EstatusProyectos estatusproyecto)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {

                // Abrir conexión y validar duplicados
                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {

                    // Validar si ya existe un registro con el mismo IdSistema en la base de datos
                    string sqlValidarEstatusProyectosDescripcion = "SELECT COUNT(*) FROM EstatusProyectos WHERE EstatusProyectosDescripcion = @EstatusProyectosDescripcion;";
                    var parametrosValidarEstatusProyectosDescripcion = new DynamicParameters();
                    parametrosValidarEstatusProyectosDescripcion.Add("@EstatusProyectosDescripcion", estatusproyecto.EstatusProyectosDescripcion, DbType.String);


                    int EstatusProyectosDescripcionExistente = await connection.ExecuteScalarAsync<int>(sqlValidarEstatusProyectosDescripcion, parametrosValidarEstatusProyectosDescripcion);

                    if (EstatusProyectosDescripcionExistente > 0)
                    {
                        respuesta.Mensaje = "Los proyectos ya cuentan con este estatus.";
                        return respuesta;
                    }

                    // Consulta SQL para insertar el estatusproyecto
                    string sqlInsertar = @"
                    INSERT INTO EstatusProyectos 
                    (EstatusProyectosDescripcion, Detalles, IdUsuarioSet) 
                    VALUES 
                    (@EstatusProyectosDescripcion, @Detalles, @IdUsuarioSet);";

                    var parametrosInsertar = new DynamicParameters();
                    parametrosInsertar.Add("@EstatusProyectosDescripcion", estatusproyecto.EstatusProyectosDescripcion, DbType.String);
                    parametrosInsertar.Add("@Detalles", estatusproyecto.Detalles, DbType.String);
                    parametrosInsertar.Add("@IdUsuarioSet", estatusproyecto.IdUsuarioSet, DbType.Int32);

                    // Ejecutar la consulta de inserción
                    int filasAfectadas = await connection.ExecuteAsync(sqlInsertar, parametrosInsertar);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "El estatus del proyecto se agregó exitosamente.";
                        respuesta.Data = new { EstatusProyectoId = estatusproyecto.Id }; // Puedes devolver el ID del estatusproyecto si lo necesitas
                    }
                    else
                    {
                        respuesta.Mensaje = "Hubo un error al agregar el estatus del proyecto.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Se produjo un error al intentar agregar el estatus del proyecto: " + ex.Message;
                respuesta.Errores.Add(ex.Message);
            }
            finally
            {
                _dapperContext.Dispose(); // Liberar recursos
            }

            return respuesta;
        }

        public async Task<RespuestaJson> EditarEstatusProyecto(EstatusProyectos estatusproyecto)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Obtener la fecha y hora actual
                estatusproyecto.FechaUpd = DateTime.Now;

                // Abrir conexión y validar duplicados
                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Obtener el valor actual de EstatusProyectosDescripcion desde la base de datos
                    string sqlObtenerDescripcionActual = @"
                    SELECT EstatusProyectosDescripcion 
                    FROM EstatusProyectos 
                    WHERE Id = @Id;";

                    var parametrosObtenerDescripcion = new DynamicParameters();
                    parametrosObtenerDescripcion.Add("@Id", estatusproyecto.Id, DbType.Int32);

                    string descripcionActual = await connection.QueryFirstOrDefaultAsync<string>(sqlObtenerDescripcionActual, parametrosObtenerDescripcion);

                    // Validar duplicados solo si la descripción está siendo modificada
                    if (descripcionActual != estatusproyecto.EstatusProyectosDescripcion)
                    {
                        string sqlValidarEstatusProyectosDescripcion = @"
                        SELECT COUNT(*) 
                        FROM EstatusProyectos 
                        WHERE EstatusProyectosDescripcion = @EstatusProyectosDescripcion 
                        AND Id != @Id;"; // Excluir el registro actual de la validación

                        var parametrosValidar = new DynamicParameters();
                        parametrosValidar.Add("@EstatusProyectosDescripcion", estatusproyecto.EstatusProyectosDescripcion, DbType.String);
                        parametrosValidar.Add("@Id", estatusproyecto.Id, DbType.Int32);

                        int existeDescripcion = await connection.ExecuteScalarAsync<int>(sqlValidarEstatusProyectosDescripcion, parametrosValidar);

                        if (existeDescripcion > 0)
                        {
                            respuesta.Mensaje = "Ya existe un estatus con la misma descripción.";
                            return respuesta;
                        }
                    }

                    // Construir la consulta SQL dinámicamente
                    var sqlActualizar = new StringBuilder("UPDATE EstatusProyectos SET ");
                    var parametrosActualizar = new DynamicParameters();

                    sqlActualizar.Append("EstatusProyectosDescripcion = @EstatusProyectosDescripcion, ");
                    parametrosActualizar.Add("@EstatusProyectosDescripcion", estatusproyecto.EstatusProyectosDescripcion, DbType.String);

                    if (!string.IsNullOrEmpty(estatusproyecto.Detalles))
                    {
                        sqlActualizar.Append("Detalles = @Detalles, ");
                        parametrosActualizar.Add("@Detalles", estatusproyecto.Detalles, DbType.String);
                    }

                    sqlActualizar.Append("IdUsuarioUpd = @IdUsuarioUpd, FechaUpd = @FechaUpd ");
                    sqlActualizar.Append("WHERE Id = @Id;");

                    parametrosActualizar.Add("@Id", estatusproyecto.Id, DbType.Int32);
                    parametrosActualizar.Add("@IdUsuarioUpd", estatusproyecto.IdUsuarioUpd, DbType.Int32);
                    parametrosActualizar.Add("@FechaUpd", estatusproyecto.FechaUpd, DbType.DateTime);

                    int filasAfectadas = await connection.ExecuteAsync(sqlActualizar.ToString(), parametrosActualizar);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "Estatus de los proyectos actualizado con éxito.";
                    }
                    else
                    {
                        respuesta.Mensaje = "No fue posible actualizar el estatus de los proyectos.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Se produjo un error al actualizar el estatus de los proyectos: " + ex.Message;
                respuesta.Errores.Add(ex.Message);
            }
            finally
            {
                _dapperContext.Dispose(); // Liberar recursos
            }

            return respuesta;
        }

        public async Task<RespuestaJson> DesactivarEstatusProyecto(EstatusProyectos estatusproyecto)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Obtener la fecha y hora actual
                estatusproyecto.FechaDel = DateTime.Now;

                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Consulta SQL para desactivar el estatusproyecto
                    string sql = @"
                    UPDATE EstatusProyectos 
                    SET FlgActivo = 0, 
                        IdUsuarioDel = @IdUsuarioDel, 
                        FechaDel = @FechaDel 
                    WHERE Id = @Id";

                    var parametros = new DynamicParameters();
                    parametros.Add("@Id", estatusproyecto.Id, DbType.Int32);
                    parametros.Add("@IdUsuarioDel", estatusproyecto.IdUsuarioDel, DbType.Int32);
                    parametros.Add("@FechaDel", estatusproyecto.FechaDel, DbType.DateTime);

                    // Ejecutar la consulta
                    int filasAfectadas = await connection.ExecuteAsync(sql, parametros);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "EstatusProyecto desactivado correctamente.";
                    }
                    else
                    {
                        respuesta.Mensaje = "No se encontró el estatusproyecto o ya está desactivado.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al desactivar el estatusproyecto: " + ex.Message;
            }
            finally
            {
                _dapperContext.Dispose();
            }
            return respuesta;
        }

        public async Task<RespuestaJson> ReactivarEstatusProyecto(EstatusProyectos estatusproyecto)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Obtener la fecha y hora actual
                estatusproyecto.FechaUpd = DateTime.Now;

                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Consulta SQL para reactivar el estatusproyecto
                    string sql = @"
                    UPDATE EstatusProyectos 
                    SET FlgActivo = 1, 
                        IdUsuarioUpd = @IdUsuarioUpd, 
                        FechaUpd = @FechaUpd
                    WHERE Id = @Id";

                    var parametros = new DynamicParameters();
                    parametros.Add("@Id", estatusproyecto.Id, DbType.Int32);
                    parametros.Add("@IdUsuarioUpd", estatusproyecto.IdUsuarioUpd, DbType.Int32);
                    parametros.Add("@FechaUpd", estatusproyecto.FechaUpd, DbType.DateTime);

                    // Ejecutar la consulta
                    int filasAfectadas = await connection.ExecuteAsync(sql, parametros);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "EstatusProyecto reactivado correctamente.";
                    }
                    else
                    {
                        respuesta.Mensaje = "No se encontró el estatusproyecto o ya está reactivado.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al reactivar el estatusproyecto: " + ex.Message;
            }
            finally
            {
                _dapperContext.Dispose();
            }
            return respuesta;
        }

        //============================================================================================================================\\
        //=================================================   Modelo EstatusTareas   ==================================================\\
        //==============================================================================================================================\\

        public async Task<RespuestaJson> GetEstatusTareas()
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Abrir la conexión a la base de datos
                _dapperContext.AbrirConexion("SGP");

                string sql = "SELECT Id, EstatusTareaDescripcion, Detalles, FlgActivo FROM EstatusTareas";

                var estatustareas = await _dapperContext.QueryAsync<EstatusTareas>(sql);
                if (estatustareas != null)
                {
                    respuesta.Resultado = true;
                    respuesta.Data = estatustareas.Select(s => new EstatusTareas
                    {
                        Id = s.Id,
                        EstatusTareaDescripcion = s.EstatusTareaDescripcion,
                        Detalles = s.Detalles,
                        FlgActivo = s.FlgActivo
                    }).ToList();
                }
                else
                {
                    respuesta.Mensaje = "No se encontraron estatustareas activos.";
                    respuesta.Data = new List<EstatusTareas>(); // Inicializar Data para evitar null
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al obtener los datos de los estatustareas." + ex.Message;
                respuesta.Data = new List<EstatusTareas>(); // Inicializar Data para evitar null
            }
            finally
            {
                // Cerrar o liberar la conexión (si es necesario)
                _dapperContext.Dispose();
            }
            return respuesta;
        }

        public async Task<RespuestaJson> AgregarEstatusTarea(EstatusTareas estatustarea)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {

                // Abrir conexión y validar duplicados
                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {

                    // Validar si ya existe un registro con el mismo IdSistema en la base de datos
                    string sqlValidarEstatusTareasDescripcion = "SELECT COUNT(*) FROM EstatusTareas WHERE EstatusTareaDescripcion = @EstatusTareaDescripcion;";
                    var parametrosValidarEstatusTareasDescripcion = new DynamicParameters();
                    parametrosValidarEstatusTareasDescripcion.Add("@EstatusTareaDescripcion", estatustarea.EstatusTareaDescripcion, DbType.String);


                    int EstatusTareasDescripcionExistente = await connection.ExecuteScalarAsync<int>(sqlValidarEstatusTareasDescripcion, parametrosValidarEstatusTareasDescripcion);

                    if (EstatusTareasDescripcionExistente > 0)
                    {
                        respuesta.Mensaje = "Las tareas ya cuentan con este estatus.";
                        return respuesta;
                    }

                    // Consulta SQL para insertar el estatustarea
                    string sqlInsertar = @"
                    INSERT INTO EstatusTareas 
                    (EstatusTareaDescripcion, Detalles, IdUsuarioSet) 
                    VALUES 
                    (@EstatusTareaDescripcion, @Detalles, @IdUsuarioSet);";

                    var parametrosInsertar = new DynamicParameters();
                    parametrosInsertar.Add("@EstatusTareaDescripcion", estatustarea.EstatusTareaDescripcion, DbType.String);
                    parametrosInsertar.Add("@Detalles", estatustarea.Detalles, DbType.String);
                    parametrosInsertar.Add("@IdUsuarioSet", estatustarea.IdUsuarioSet, DbType.Int32);

                    // Ejecutar la consulta de inserción
                    int filasAfectadas = await connection.ExecuteAsync(sqlInsertar, parametrosInsertar);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "El estatus de la tarea se agregó exitosamente.";
                        respuesta.Data = new { EstatusTareaId = estatustarea.Id }; // Puedes devolver el ID del estatustarea si lo necesitas
                    }
                    else
                    {
                        respuesta.Mensaje = "Hubo un error al agregar el estatus de la tarea.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Se produjo un error al intentar agregar el estatus de la tarea: " + ex.Message;
                respuesta.Errores.Add(ex.Message);
            }
            finally
            {
                _dapperContext.Dispose(); // Liberar recursos
            }

            return respuesta;
        }

        public async Task<RespuestaJson> EditarEstatusTarea(EstatusTareas estatustarea)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Obtener la fecha y hora actual
                estatustarea.FechaUpd = DateTime.Now;

                // Abrir conexión y validar duplicados
                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Obtener el valor actual de EstatusTareas desde la base de datos
                    string sqlObtenerDescripcionActual = @"
                    SELECT EstatusTareaDescripcion 
                    FROM EstatusTareas 
                    WHERE Id = @Id;";

                    var parametrosObtenerDescripcion = new DynamicParameters();
                    parametrosObtenerDescripcion.Add("@Id", estatustarea.Id, DbType.Int32);

                    string descripcionActual = await connection.QueryFirstOrDefaultAsync<string>(sqlObtenerDescripcionActual, parametrosObtenerDescripcion);

                    // Validar duplicados solo si la descripción está siendo modificada
                    if (descripcionActual != estatustarea.EstatusTareaDescripcion)
                    {
                        string sqlValidarEstatusEstatusTareaDescripcion = @"
                        SELECT COUNT(*) 
                        FROM EstatusTareas  
                        WHERE EstatusTareaDescripcion = @EstatusTareaDescripcion 
                        AND Id != @Id;"; // Excluir el registro actual de la validación

                        var parametrosValidar = new DynamicParameters();
                        parametrosValidar.Add("@EstatusTareaDescripcion", estatustarea.EstatusTareaDescripcion, DbType.String);
                        parametrosValidar.Add("@Id", estatustarea.Id, DbType.Int32);

                        int existeDescripcion = await connection.ExecuteScalarAsync<int>(sqlValidarEstatusEstatusTareaDescripcion, parametrosValidar);

                        if (existeDescripcion > 0)
                        {
                            respuesta.Mensaje = "Ya existe un estatus con la misma descripción.";
                            return respuesta;
                        }
                    }

                    // Construir la consulta SQL dinámicamente
                    var sqlActualizar = new StringBuilder("UPDATE EstatusTareas SET ");
                    var parametrosActualizar = new DynamicParameters();

                    sqlActualizar.Append("EstatusTareaDescripcion = @EstatusTareaDescripcion, ");
                    parametrosActualizar.Add("@EstatusTareaDescripcion", estatustarea.EstatusTareaDescripcion, DbType.String);

                    if (!string.IsNullOrEmpty(estatustarea.Detalles))
                    {
                        sqlActualizar.Append("Detalles = @Detalles, ");
                        parametrosActualizar.Add("@Detalles", estatustarea.Detalles, DbType.String);
                    }

                    sqlActualizar.Append("IdUsuarioUpd = @IdUsuarioUpd, FechaUpd = @FechaUpd ");
                    sqlActualizar.Append("WHERE Id = @Id;");

                    parametrosActualizar.Add("@Id", estatustarea.Id, DbType.Int32);
                    parametrosActualizar.Add("@IdUsuarioUpd", estatustarea.IdUsuarioUpd, DbType.Int32);
                    parametrosActualizar.Add("@FechaUpd", estatustarea.FechaUpd, DbType.DateTime);

                    int filasAfectadas = await connection.ExecuteAsync(sqlActualizar.ToString(), parametrosActualizar);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "Estatus de los proyectos actualizado con éxito.";
                    }
                    else
                    {
                        respuesta.Mensaje = "No fue posible actualizar el estatus de los proyectos.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Se produjo un error al actualizar el estatus de los proyectos: " + ex.Message;
                respuesta.Errores.Add(ex.Message);
            }
            finally
            {
                _dapperContext.Dispose(); // Liberar recursos
            }

            return respuesta;
        }

        public async Task<RespuestaJson> DesactivarEstatusTarea(EstatusTareas estatustarea)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Obtener la fecha y hora actual
                estatustarea.FechaDel = DateTime.Now;

                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Consulta SQL para desactivar el estatustarea
                    string sql = @"
                    UPDATE EstatusTareas 
                    SET FlgActivo = 0, 
                        IdUsuarioDel = @IdUsuarioDel, 
                        FechaDel = @FechaDel 
                    WHERE Id = @Id";

                    var parametros = new DynamicParameters();
                    parametros.Add("@Id", estatustarea.Id, DbType.Int32);
                    parametros.Add("@IdUsuarioDel", estatustarea.IdUsuarioDel, DbType.Int32);
                    parametros.Add("@FechaDel", estatustarea.FechaDel, DbType.DateTime);

                    // Ejecutar la consulta
                    int filasAfectadas = await connection.ExecuteAsync(sql, parametros);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "EstatusTarea desactivado correctamente.";
                    }
                    else
                    {
                        respuesta.Mensaje = "No se encontró el estatustarea o ya está desactivado.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al desactivar el estatustarea: " + ex.Message;
            }
            finally
            {
                _dapperContext.Dispose();
            }
            return respuesta;
        }

        public async Task<RespuestaJson> ReactivarEstatusTarea(EstatusTareas estatustarea)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Obtener la fecha y hora actual
                estatustarea.FechaUpd = DateTime.Now;

                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Consulta SQL para reactivar el estatustarea
                    string sql = @"
                    UPDATE EstatusTareas 
                    SET FlgActivo = 1, 
                        IdUsuarioUpd = @IdUsuarioUpd, 
                        FechaUpd = @FechaUpd
                    WHERE Id = @Id";

                    var parametros = new DynamicParameters();
                    parametros.Add("@Id", estatustarea.Id, DbType.Int32);
                    parametros.Add("@IdUsuarioUpd", estatustarea.IdUsuarioUpd, DbType.Int32);
                    parametros.Add("@FechaUpd", estatustarea.FechaUpd, DbType.DateTime);

                    // Ejecutar la consulta
                    int filasAfectadas = await connection.ExecuteAsync(sql, parametros);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "EstatusTarea reactivado correctamente.";
                    }
                    else
                    {
                        respuesta.Mensaje = "No se encontró el estatustarea o ya está reactivado.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al reactivar el estatustarea: " + ex.Message;
            }
            finally
            {
                _dapperContext.Dispose();
            }
            return respuesta;
        }

        //============================================================================================================================\\
        //================================================   Modelo EstatusSoportes   ================================================ \\
        //==============================================================================================================================\\

        public async Task<RespuestaJson> GetEstatusSoportes()
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Abrir la conexión a la base de datos
                _dapperContext.AbrirConexion("SGP");

                string sql = "SELECT Id, EstatusSoporteDescripcion, Detalles, FlgActivo FROM EstatusSoportes";

                var estatussoportes = await _dapperContext.QueryAsync<EstatusSoportes>(sql);
                if (estatussoportes != null)
                {
                    respuesta.Resultado = true;
                    respuesta.Data = estatussoportes.Select(s => new EstatusSoportes
                    {
                        Id = s.Id,
                        EstatusSoporteDescripcion = s.EstatusSoporteDescripcion,
                        Detalles = s.Detalles,
                        FlgActivo = s.FlgActivo
                    }).ToList();
                }
                else
                {
                    respuesta.Mensaje = "No se encontraron estatussoportes activos.";
                    respuesta.Data = new List<EstatusSoportes>(); // Inicializar Data para evitar null
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al obtener los datos de los estatussoportes." + ex.Message;
                respuesta.Data = new List<EstatusSoportes>(); // Inicializar Data para evitar null
            }
            finally
            {
                // Cerrar o liberar la conexión (si es necesario)
                _dapperContext.Dispose();
            }
            return respuesta;
        }

        public async Task<RespuestaJson> AgregarEstatusSoporte(EstatusSoportes estatussoporte)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {

                // Abrir conexión y validar duplicados
                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {

                    // Validar si ya existe un registro con el mismo IdSistema en la base de datos
                    string sqlValidarEstatusSoportesDescripcion = "SELECT COUNT(*) FROM EstatusSoportes WHERE EstatusSoporteDescripcion = @EstatusSoporteDescripcion;";
                    var parametrosValidarEstatusSoportesDescripcion = new DynamicParameters();
                    parametrosValidarEstatusSoportesDescripcion.Add("@EstatusSoporteDescripcion", estatussoporte.EstatusSoporteDescripcion, DbType.String);


                    int EstatusSoportesDescripcionExistente = await connection.ExecuteScalarAsync<int>(sqlValidarEstatusSoportesDescripcion, parametrosValidarEstatusSoportesDescripcion);

                    if (EstatusSoportesDescripcionExistente > 0)
                    {
                        respuesta.Mensaje = "Los soportes ya cuentan con este estatus.";
                        return respuesta;
                    }

                    // Consulta SQL para insertar el estatussoporte
                    string sqlInsertar = @"
                    INSERT INTO EstatusSoportes 
                    (EstatusSoporteDescripcion, Detalles, IdUsuarioSet) 
                    VALUES 
                    (@EstatusSoporteDescripcion, @Detalles, @IdUsuarioSet);";

                    var parametrosInsertar = new DynamicParameters();
                    parametrosInsertar.Add("@EstatusSoporteDescripcion", estatussoporte.EstatusSoporteDescripcion, DbType.String);
                    parametrosInsertar.Add("@Detalles", estatussoporte.Detalles, DbType.String);
                    parametrosInsertar.Add("@IdUsuarioSet", estatussoporte.IdUsuarioSet, DbType.Int32);

                    // Ejecutar la consulta de inserción
                    int filasAfectadas = await connection.ExecuteAsync(sqlInsertar, parametrosInsertar);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "El estatus del soporte se agregó exitosamente.";
                        respuesta.Data = new { EstatusSoporteId = estatussoporte.Id }; // Puedes devolver el ID del estatussoporte si lo necesitas
                    }
                    else
                    {
                        respuesta.Mensaje = "Hubo un error al agregar el estatus del soporte.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Se produjo un error al intentar agregar el estatus del soporte: " + ex.Message;
                respuesta.Errores.Add(ex.Message);
            }
            finally
            {
                _dapperContext.Dispose(); // Liberar recursos
            }

            return respuesta;
        }

        public async Task<RespuestaJson> EditarEstatusSoporte(EstatusSoportes estatussoporte)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Obtener la fecha y hora actual
                estatussoporte.FechaUpd = DateTime.Now;

                // Abrir conexión y validar duplicados
                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Obtener el valor actual de EstatusTareas desde la base de datos
                    string sqlObtenerDescripcionActual = @"
                     SELECT EstatusSoporteDescripcion 
                     FROM EstatusSoportes 
                     WHERE Id = @Id;";

                    var parametrosObtenerDescripcion = new DynamicParameters();
                    parametrosObtenerDescripcion.Add("@Id", estatussoporte.Id, DbType.Int32);

                    string descripcionActual = await connection.QueryFirstOrDefaultAsync<string>(sqlObtenerDescripcionActual, parametrosObtenerDescripcion);

                    // Validar duplicados solo si la descripción está siendo modificada
                    if (descripcionActual != estatussoporte.EstatusSoporteDescripcion)
                    {
                        string sqlValidarEstatusEstatusTareaDescripcion = @"
                         SELECT COUNT(*) 
                         FROM EstatusSoportes  
                         WHERE EstatusSoporteDescripcion = @EstatusSoporteDescripcion 
                         AND Id != @Id;"; // Excluir el registro actual de la validación

                        var parametrosValidar = new DynamicParameters();
                        parametrosValidar.Add("@EstatusSoporteDescripcion", estatussoporte.EstatusSoporteDescripcion, DbType.String);
                        parametrosValidar.Add("@Id", estatussoporte.Id, DbType.Int32);

                        int existeDescripcion = await connection.ExecuteScalarAsync<int>(sqlValidarEstatusEstatusTareaDescripcion, parametrosValidar);

                        if (existeDescripcion > 0)
                        {
                            respuesta.Mensaje = "Ya existe un estatus con la misma descripción.";
                            return respuesta;
                        }
                    }

                    // Construir la consulta SQL dinámicamente
                    var sqlActualizar = new StringBuilder("UPDATE EstatusSoportes SET ");
                    var parametrosActualizar = new DynamicParameters();

                    sqlActualizar.Append("EstatusSoporteDescripcion = @EstatusSoporteDescripcion, ");
                    parametrosActualizar.Add("@EstatusSoporteDescripcion", estatussoporte.EstatusSoporteDescripcion, DbType.String);

                    if (!string.IsNullOrEmpty(estatussoporte.Detalles))
                    {
                        sqlActualizar.Append("Detalles = @Detalles, ");
                        parametrosActualizar.Add("@Detalles", estatussoporte.Detalles, DbType.String);
                    }

                    sqlActualizar.Append("IdUsuarioUpd = @IdUsuarioUpd, FechaUpd = @FechaUpd ");
                    sqlActualizar.Append("WHERE Id = @Id;");

                    parametrosActualizar.Add("@Id", estatussoporte.Id, DbType.Int32);
                    parametrosActualizar.Add("@IdUsuarioUpd", estatussoporte.IdUsuarioUpd, DbType.Int32);
                    parametrosActualizar.Add("@FechaUpd", estatussoporte.FechaUpd, DbType.DateTime);

                    int filasAfectadas = await connection.ExecuteAsync(sqlActualizar.ToString(), parametrosActualizar);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "Estatus de los proyectos actualizado con éxito.";
                    }
                    else
                    {
                        respuesta.Mensaje = "No fue posible actualizar el estatus de los proyectos.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Se produjo un error al actualizar el estatus de los proyectos: " + ex.Message;
                respuesta.Errores.Add(ex.Message);
            }
            finally
            {
                _dapperContext.Dispose(); // Liberar recursos
            }

            return respuesta;
        }

        public async Task<RespuestaJson> DesactivarEstatusSoporte(EstatusSoportes estatussoporte)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Obtener la fecha y hora actual
                estatussoporte.FechaDel = DateTime.Now;

                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Consulta SQL para desactivar el estatussoporte
                    string sql = @"
                    UPDATE EstatusSoportes 
                    SET FlgActivo = 0, 
                        IdUsuarioDel = @IdUsuarioDel, 
                        FechaDel = @FechaDel 
                    WHERE Id = @Id";

                    var parametros = new DynamicParameters();
                    parametros.Add("@Id", estatussoporte.Id, DbType.Int32);
                    parametros.Add("@IdUsuarioDel", estatussoporte.IdUsuarioDel, DbType.Int32);
                    parametros.Add("@FechaDel", estatussoporte.FechaDel, DbType.DateTime);

                    // Ejecutar la consulta
                    int filasAfectadas = await connection.ExecuteAsync(sql, parametros);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "EstatusSoporte desactivado correctamente.";
                    }
                    else
                    {
                        respuesta.Mensaje = "No se encontró el estatussoporte o ya está desactivado.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al desactivar el estatussoporte: " + ex.Message;
            }
            finally
            {
                _dapperContext.Dispose();
            }
            return respuesta;
        }

        public async Task<RespuestaJson> ReactivarEstatusSoporte(EstatusSoportes estatussoporte)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Obtener la fecha y hora actual
                estatussoporte.FechaUpd = DateTime.Now;

                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Consulta SQL para reactivar el estatussoporte
                    string sql = @"
                    UPDATE EstatusSoportes 
                    SET FlgActivo = 1, 
                        IdUsuarioUpd = @IdUsuarioUpd, 
                        FechaUpd = @FechaUpd
                    WHERE Id = @Id";

                    var parametros = new DynamicParameters();
                    parametros.Add("@Id", estatussoporte.Id, DbType.Int32);
                    parametros.Add("@IdUsuarioUpd", estatussoporte.IdUsuarioUpd, DbType.Int32);
                    parametros.Add("@FechaUpd", estatussoporte.FechaUpd, DbType.DateTime);

                    // Ejecutar la consulta
                    int filasAfectadas = await connection.ExecuteAsync(sql, parametros);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "EstatusSoporte reactivado correctamente.";
                    }
                    else
                    {
                        respuesta.Mensaje = "No se encontró el estatussoporte o ya está reactivado.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al reactivar el estatussoporte: " + ex.Message;
            }
            finally
            {
                _dapperContext.Dispose();
            }
            return respuesta;
        }

        //============================================================================================================================\\
        //=============================================   Modelo NivelServicioSoportes   ==============================================\\
        //==============================================================================================================================\\

        public async Task<RespuestaJson> GetNivelServicioSoportes()
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Abrir la conexión a la base de datos
                _dapperContext.AbrirConexion("SGP");

                string sql = "SELECT Id, NivelServicioSoporteDescripcion, Detalles, FlgActivo FROM NivelServicioSoportes";

                var nivelserviciosoportes = await _dapperContext.QueryAsync<NivelServicioSoportes>(sql);
                if (nivelserviciosoportes != null)
                {
                    respuesta.Resultado = true;
                    respuesta.Data = nivelserviciosoportes.Select(s => new NivelServicioSoportes
                    {
                        Id = s.Id,
                        NivelServicioSoporteDescripcion = s.NivelServicioSoporteDescripcion,
                        Detalles = s.Detalles,
                        FlgActivo = s.FlgActivo
                    }).ToList();
                }
                else
                {
                    respuesta.Mensaje = "No se encontraron nivelserviciosoportes activos.";
                    respuesta.Data = new List<NivelServicioSoportes>(); // Inicializar Data para evitar null
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al obtener los datos de los nivelserviciosoportes." + ex.Message;
                respuesta.Data = new List<NivelServicioSoportes>(); // Inicializar Data para evitar null
            }
            finally
            {
                // Cerrar o liberar la conexión (si es necesario)
                _dapperContext.Dispose();
            }
            return respuesta;
        }

        public async Task<RespuestaJson> AgregarNivelServicioSoporte(NivelServicioSoportes nivelserviciosoporte)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {

                // Abrir conexión y validar duplicados
                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {

                    // Validar si ya existe un registro con el mismo IdSistema en la base de datos
                    string sqlValidarNivelServicioSoportesDescripcion = "SELECT COUNT(*) FROM NivelServicioSoportes WHERE NivelServicioSoporteDescripcion = @NivelServicioSoporteDescripcion;";
                    var parametrosValidarNivelServicioSoportesDescripcion = new DynamicParameters();
                    parametrosValidarNivelServicioSoportesDescripcion.Add("@NivelServicioSoporteDescripcion", nivelserviciosoporte.NivelServicioSoporteDescripcion, DbType.String);


                    int NivelServicioSoportesDescripcionExistente = await connection.ExecuteScalarAsync<int>(sqlValidarNivelServicioSoportesDescripcion, parametrosValidarNivelServicioSoportesDescripcion);

                    if (NivelServicioSoportesDescripcionExistente > 0)
                    {
                        respuesta.Mensaje = "Los soportes ya cuentan con este nivel de servicio.";
                        return respuesta;
                    }

                    // Consulta SQL para insertar el nivelserviciosoporte
                    string sqlInsertar = @"
                    INSERT INTO NivelServicioSoportes 
                    (NivelServicioSoporteDescripcion, Detalles, IdUsuarioSet) 
                    VALUES 
                    (@NivelServicioSoporteDescripcion, @Detalles, @IdUsuarioSet);";

                    var parametrosInsertar = new DynamicParameters();
                    parametrosInsertar.Add("@NivelServicioSoporteDescripcion", nivelserviciosoporte.NivelServicioSoporteDescripcion, DbType.String);
                    parametrosInsertar.Add("@Detalles", nivelserviciosoporte.Detalles, DbType.String);
                    parametrosInsertar.Add("@IdUsuarioSet", nivelserviciosoporte.IdUsuarioSet, DbType.Int32);

                    // Ejecutar la consulta de inserción
                    int filasAfectadas = await connection.ExecuteAsync(sqlInsertar, parametrosInsertar);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "El nivel de servicio del soporte se agregó exitosamente.";
                        respuesta.Data = new { NivelServicioSoporteId = nivelserviciosoporte.Id }; // Puedes devolver el ID del nivelserviciosoporte si lo necesitas
                    }
                    else
                    {
                        respuesta.Mensaje = "Hubo un error al agregar el nivel de servicio del soporte.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Se produjo un error al intentar agregar el nivel de servicio del soporte: " + ex.Message;
                respuesta.Errores.Add(ex.Message);
            }
            finally
            {
                _dapperContext.Dispose(); // Liberar recursos
            }

            return respuesta;
        }

        public async Task<RespuestaJson> EditarNivelServicioSoporte(NivelServicioSoportes nivelserviciosoporte)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Obtener la fecha y hora actual
                nivelserviciosoporte.FechaUpd = DateTime.Now;

                // Abrir conexión y validar duplicados
                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Obtener el valor actual de EstatusTareas desde la base de datos
                    string sqlObtenerDescripcionActual = @"
                    SELECT NivelServicioSoporteDescripcion 
                    FROM NivelServicioSoportes 
                    WHERE Id = @Id;";

                    var parametrosObtenerDescripcion = new DynamicParameters();
                    parametrosObtenerDescripcion.Add("@Id", nivelserviciosoporte.Id, DbType.Int32);

                    string descripcionActual = await connection.QueryFirstOrDefaultAsync<string>(sqlObtenerDescripcionActual, parametrosObtenerDescripcion);

                    // Validar duplicados solo si la descripción está siendo modificada
                    if (descripcionActual != nivelserviciosoporte.NivelServicioSoporteDescripcion)
                    {
                        string sqlValidarNivelServicioSoporteDescripcion = @"
                        SELECT COUNT(*) 
                        FROM NivelServicioSoportes  
                        WHERE NivelServicioSoporteDescripcion = @NivelServicioSoporteDescripcion 
                        AND Id != @Id;"; // Excluir el registro actual de la validación

                        var parametrosValidar = new DynamicParameters();
                        parametrosValidar.Add("@NivelServicioSoporteDescripcion", nivelserviciosoporte.NivelServicioSoporteDescripcion, DbType.String);
                        parametrosValidar.Add("@Id", nivelserviciosoporte.Id, DbType.Int32);

                        int existeDescripcion = await connection.ExecuteScalarAsync<int>(sqlValidarNivelServicioSoporteDescripcion, parametrosValidar);

                        if (existeDescripcion > 0)
                        {
                            respuesta.Mensaje = "Ya existe un estatus con la misma descripción.";
                            return respuesta;
                        }
                    }

                    // Construir la consulta SQL dinámicamente
                    var sqlActualizar = new StringBuilder("UPDATE NivelServicioSoportes SET ");
                    var parametrosActualizar = new DynamicParameters();

                    sqlActualizar.Append("NivelServicioSoporteDescripcion = @NivelServicioSoporteDescripcion, ");
                    parametrosActualizar.Add("@NivelServicioSoporteDescripcion", nivelserviciosoporte.NivelServicioSoporteDescripcion, DbType.String);

                    if (!string.IsNullOrEmpty(nivelserviciosoporte.Detalles))
                    {
                        sqlActualizar.Append("Detalles = @Detalles, ");
                        parametrosActualizar.Add("@Detalles", nivelserviciosoporte.Detalles, DbType.String);
                    }

                    sqlActualizar.Append("IdUsuarioUpd = @IdUsuarioUpd, FechaUpd = @FechaUpd ");
                    sqlActualizar.Append("WHERE Id = @Id;");

                    parametrosActualizar.Add("@Id", nivelserviciosoporte.Id, DbType.Int32);
                    parametrosActualizar.Add("@IdUsuarioUpd", nivelserviciosoporte.IdUsuarioUpd, DbType.Int32);
                    parametrosActualizar.Add("@FechaUpd", nivelserviciosoporte.FechaUpd, DbType.DateTime);

                    int filasAfectadas = await connection.ExecuteAsync(sqlActualizar.ToString(), parametrosActualizar);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "El nivel de servicio de los soportes ha sido actualizado con éxito.";
                    }
                    else
                    {
                        respuesta.Mensaje = "No fue posible actualizar el nivel de servicio de los soportes.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Se produjo un error al actualizar el nivel de servicio de los soportes: " + ex.Message;
                respuesta.Errores.Add(ex.Message);
            }
            finally
            {
                _dapperContext.Dispose(); // Liberar recursos
            }

            return respuesta;
        }

        public async Task<RespuestaJson> DesactivarNivelServicioSoporte(NivelServicioSoportes nivelserviciosoporte)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Obtener la fecha y hora actual
                nivelserviciosoporte.FechaDel = DateTime.Now;

                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Consulta SQL para desactivar el nivelserviciosoporte
                    string sql = @"
                    UPDATE NivelServicioSoportes 
                    SET FlgActivo = 0, 
                        IdUsuarioDel = @IdUsuarioDel, 
                        FechaDel = @FechaDel 
                    WHERE Id = @Id";

                    var parametros = new DynamicParameters();
                    parametros.Add("@Id", nivelserviciosoporte.Id, DbType.Int32);
                    parametros.Add("@IdUsuarioDel", nivelserviciosoporte.IdUsuarioDel, DbType.Int32);
                    parametros.Add("@FechaDel", nivelserviciosoporte.FechaDel, DbType.DateTime);

                    // Ejecutar la consulta
                    int filasAfectadas = await connection.ExecuteAsync(sql, parametros);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "NivelServicioSoporte desactivado correctamente.";
                    }
                    else
                    {
                        respuesta.Mensaje = "No se encontró el nivelserviciosoporte o ya está desactivado.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al desactivar el nivelserviciosoporte: " + ex.Message;
            }
            finally
            {
                _dapperContext.Dispose();
            }
            return respuesta;
        }

        public async Task<RespuestaJson> ReactivarNivelServicioSoporte(NivelServicioSoportes nivelserviciosoporte)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Obtener la fecha y hora actual
                nivelserviciosoporte.FechaUpd = DateTime.Now;

                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Consulta SQL para reactivar el nivelserviciosoporte
                    string sql = @"
                    UPDATE NivelServicioSoportes 
                    SET FlgActivo = 1, 
                        IdUsuarioUpd = @IdUsuarioUpd, 
                        FechaUpd = @FechaUpd
                    WHERE Id = @Id";

                    var parametros = new DynamicParameters();
                    parametros.Add("@Id", nivelserviciosoporte.Id, DbType.Int32);
                    parametros.Add("@IdUsuarioUpd", nivelserviciosoporte.IdUsuarioUpd, DbType.Int32);
                    parametros.Add("@FechaUpd", nivelserviciosoporte.FechaUpd, DbType.DateTime);

                    // Ejecutar la consulta
                    int filasAfectadas = await connection.ExecuteAsync(sql, parametros);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "NivelServicioSoporte reactivado correctamente.";
                    }
                    else
                    {
                        respuesta.Mensaje = "No se encontró el nivelserviciosoporte o ya está reactivado.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al reactivar el nivelserviciosoporte: " + ex.Message;
            }
            finally
            {
                _dapperContext.Dispose();
            }
            return respuesta;
        }

        //============================================================================================================================\\
        //=============================================   Modelo TipoSoportes   =====================================================\\
        //==============================================================================================================================\\

        public async Task<RespuestaJson> GetTipoSoportes()
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Abrir la conexión a la base de datos
                _dapperContext.AbrirConexion("SGP");

                string sql = "SELECT Id, TipoSoporteDescripcion, Detalles, FlgActivo FROM TipoSoportes";

                var tiposoportes = await _dapperContext.QueryAsync<TipoSoportes>(sql);
                if (tiposoportes != null)
                {
                    respuesta.Resultado = true;
                    respuesta.Data = tiposoportes.Select(s => new TipoSoportes
                    {
                        Id = s.Id,
                        TipoSoporteDescripcion = s.TipoSoporteDescripcion,
                        Detalles = s.Detalles,
                        FlgActivo = s.FlgActivo
                    }).ToList();
                }
                else
                {
                    respuesta.Mensaje = "No se encontraron tiposoportes activos.";
                    respuesta.Data = new List<TipoSoportes>(); // Inicializar Data para evitar null
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al obtener los datos de los tiposoportes." + ex.Message;
                respuesta.Data = new List<TipoSoportes>(); // Inicializar Data para evitar null
            }
            finally
            {
                // Cerrar o liberar la conexión (si es necesario)
                _dapperContext.Dispose();
            }
            return respuesta;
        }

        public async Task<RespuestaJson> AgregarTipoSoporte(TipoSoportes tiposoporte)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Abrir conexión y validar duplicados
                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Validar si ya existe un registro con el mismo IdSistema en la base de datos
                    string sqlValidarTipoSoportesDescripcion = "SELECT COUNT(*) FROM TipoSoportes WHERE TipoSoporteDescripcion = @TipoSoporteDescripcion;";
                    var parametrosValidarTipoSoportesDescripcion = new DynamicParameters();
                    parametrosValidarTipoSoportesDescripcion.Add("@TipoSoporteDescripcion", tiposoporte.TipoSoporteDescripcion, DbType.String);

                    int TipoSoportesDescripcionExistente = await connection.ExecuteScalarAsync<int>(sqlValidarTipoSoportesDescripcion, parametrosValidarTipoSoportesDescripcion);

                    if (TipoSoportesDescripcionExistente > 0)
                    {
                        respuesta.Mensaje = "Los soportes ya cuentan con este tipo de soporte.";
                        return respuesta;
                    }

                    // Consulta SQL para insertar el tiposoporte
                    string sqlInsertar = @"
                    INSERT INTO TipoSoportes 
                    (TipoSoporteDescripcion, Detalles, IdUsuarioSet) 
                    VALUES 
                    (@TipoSoporteDescripcion, @Detalles, @IdUsuarioSet);";

                    var parametrosInsertar = new DynamicParameters();
                    parametrosInsertar.Add("@TipoSoporteDescripcion", tiposoporte.TipoSoporteDescripcion, DbType.String);
                    parametrosInsertar.Add("@Detalles", tiposoporte.Detalles, DbType.String);
                    parametrosInsertar.Add("@IdUsuarioSet", tiposoporte.IdUsuarioSet, DbType.Int32);

                    // Ejecutar la consulta de inserción
                    int filasAfectadas = await connection.ExecuteAsync(sqlInsertar, parametrosInsertar);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "El tipo de soporte se agregó exitosamente.";
                        respuesta.Data = new { TipoSoporteId = tiposoporte.Id }; // Puedes devolver el ID del tiposoporte si lo necesitas
                    }
                    else
                    {
                        respuesta.Mensaje = "Hubo un error al agregar el tipo de soporte.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Se produjo un error al intentar agregar el tipo de soporte: " + ex.Message;
                respuesta.Errores.Add(ex.Message);
            }
            finally
            {
                _dapperContext.Dispose(); // Liberar recursos
            }

            return respuesta;
        }

        public async Task<RespuestaJson> EditarTipoSoporte(TipoSoportes tiposoporte)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Obtener la fecha y hora actual
                tiposoporte.FechaUpd = DateTime.Now;

                // Abrir conexión y validar duplicados
                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Obtener el valor actual de TipoSoporteDescripcion desde la base de datos
                    string sqlObtenerDescripcionActual = @"
                    SELECT TipoSoporteDescripcion 
                    FROM TipoSoportes 
                    WHERE Id = @Id;";

                    var parametrosObtenerDescripcion = new DynamicParameters();
                    parametrosObtenerDescripcion.Add("@Id", tiposoporte.Id, DbType.Int32);

                    string descripcionActual = await connection.QueryFirstOrDefaultAsync<string>(sqlObtenerDescripcionActual, parametrosObtenerDescripcion);

                    // Validar duplicados solo si la descripción está siendo modificada
                    if (descripcionActual != tiposoporte.TipoSoporteDescripcion)
                    {
                        string sqlValidarTipoSoporteDescripcion = @"
                        SELECT COUNT(*) 
                        FROM TipoSoportes 
                        WHERE TipoSoporteDescripcion = @TipoSoporteDescripcion 
                        AND Id != @Id;"; // Excluir el registro actual de la validación

                        var parametrosValidar = new DynamicParameters();
                        parametrosValidar.Add("@TipoSoporteDescripcion", tiposoporte.TipoSoporteDescripcion, DbType.String);
                        parametrosValidar.Add("@Id", tiposoporte.Id, DbType.Int32);

                        int existeDescripcion = await connection.ExecuteScalarAsync<int>(sqlValidarTipoSoporteDescripcion, parametrosValidar);

                        if (existeDescripcion > 0)
                        {
                            respuesta.Mensaje = "Ya existe un tipo de soporte con la misma descripción.";
                            return respuesta;
                        }
                    }

                    // Construir la consulta SQL dinámicamente
                    var sqlActualizar = new StringBuilder("UPDATE TipoSoportes SET ");
                    var parametrosActualizar = new DynamicParameters();

                    sqlActualizar.Append("TipoSoporteDescripcion = @TipoSoporteDescripcion, ");
                    parametrosActualizar.Add("@TipoSoporteDescripcion", tiposoporte.TipoSoporteDescripcion, DbType.String);

                    if (!string.IsNullOrEmpty(tiposoporte.Detalles))
                    {
                        sqlActualizar.Append("Detalles = @Detalles, ");
                        parametrosActualizar.Add("@Detalles", tiposoporte.Detalles, DbType.String);
                    }

                    sqlActualizar.Append("IdUsuarioUpd = @IdUsuarioUpd, FechaUpd = @FechaUpd ");
                    sqlActualizar.Append("WHERE Id = @Id;");

                    parametrosActualizar.Add("@Id", tiposoporte.Id, DbType.Int32);
                    parametrosActualizar.Add("@IdUsuarioUpd", tiposoporte.IdUsuarioUpd, DbType.Int32);
                    parametrosActualizar.Add("@FechaUpd", tiposoporte.FechaUpd, DbType.DateTime);

                    int filasAfectadas = await connection.ExecuteAsync(sqlActualizar.ToString(), parametrosActualizar);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "Estatus de los proyectos actualizado con éxito.";
                    }
                    else
                    {
                        respuesta.Mensaje = "No fue posible actualizar el estatus de los proyectos.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Se produjo un error al actualizar el estatus de los proyectos: " + ex.Message;
                respuesta.Errores.Add(ex.Message);
            }
            finally
            {
                _dapperContext.Dispose(); // Liberar recursos
            }

            return respuesta;
        }

        public async Task<RespuestaJson> DesactivarTipoSoporte(TipoSoportes tiposoporte)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Obtener la fecha y hora actual
                tiposoporte.FechaDel = DateTime.Now;

                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Consulta SQL para desactivar el tiposoporte
                    string sql = @"
                    UPDATE TipoSoportes 
                    SET FlgActivo = 0, 
                        IdUsuarioDel = @IdUsuarioDel, 
                        FechaDel = @FechaDel 
                    WHERE Id = @Id";

                    var parametros = new DynamicParameters();
                    parametros.Add("@Id", tiposoporte.Id, DbType.Int32);
                    parametros.Add("@IdUsuarioDel", tiposoporte.IdUsuarioDel, DbType.Int32);
                    parametros.Add("@FechaDel", tiposoporte.FechaDel, DbType.DateTime);

                    // Ejecutar la consulta
                    int filasAfectadas = await connection.ExecuteAsync(sql, parametros);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "TipoSoporte desactivado correctamente.";
                    }
                    else
                    {
                        respuesta.Mensaje = "No se encontró el tiposoporte o ya está desactivado.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al desactivar el tiposoporte: " + ex.Message;
            }
            finally
            {
                _dapperContext.Dispose();
            }
            return respuesta;
        }

        public async Task<RespuestaJson> ReactivarTipoSoporte(TipoSoportes tiposoporte)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Obtener la fecha y hora actual
                tiposoporte.FechaUpd = DateTime.Now;

                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Consulta SQL para reactivar el tiposoporte
                    string sql = @"
                    UPDATE TipoSoportes 
                    SET FlgActivo = 1, 
                        IdUsuarioUpd = @IdUsuarioUpd, 
                        FechaUpd = @FechaUpd
                    WHERE Id = @Id";

                    var parametros = new DynamicParameters();
                    parametros.Add("@Id", tiposoporte.Id, DbType.Int32);
                    parametros.Add("@IdUsuarioUpd", tiposoporte.IdUsuarioUpd, DbType.Int32);
                    parametros.Add("@FechaUpd", tiposoporte.FechaUpd, DbType.DateTime);

                    // Ejecutar la consulta
                    int filasAfectadas = await connection.ExecuteAsync(sql, parametros);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "TipoSoporte reactivado correctamente.";
                    }
                    else
                    {
                        respuesta.Mensaje = "No se encontró el tiposoporte o ya está reactivado.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al reactivar el tiposoporte: " + ex.Message;
            }
            finally
            {
                _dapperContext.Dispose();
            }
            return respuesta;
        }

        //============================================================================================================================\\
        //=============================================   Modelo NivelComplejidadTareas   =============================================\\
        //==============================================================================================================================\\

        public async Task<RespuestaJson> GetNivelComplejidadTareas()
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Abrir la conexión a la base de datos
                _dapperContext.AbrirConexion("SGP");

                string sql = "SELECT Id, NivelComplejidadTareaDescripcion, Detalles, FlgActivo FROM NivelComplejidadTareas";

                var nivelcomplejidadtareas = await _dapperContext.QueryAsync<NivelComplejidadTareas>(sql);
                if (nivelcomplejidadtareas != null)
                {
                    respuesta.Resultado = true;
                    respuesta.Data = nivelcomplejidadtareas.Select(s => new NivelComplejidadTareas
                    {
                        Id = s.Id,
                        NivelComplejidadTareaDescripcion = s.NivelComplejidadTareaDescripcion,
                        Detalles = s.Detalles,
                        FlgActivo = s.FlgActivo
                    }).ToList();
                }
                else
                {
                    respuesta.Mensaje = "No se encontraron niveles de complejidad de las tareas activos.";
                    respuesta.Data = new List<NivelComplejidadTareas>(); // Inicializar Data para evitar null
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al obtener los datos de los niveles de complejidad de las tareas." + ex.Message;
                respuesta.Data = new List<NivelComplejidadTareas>(); // Inicializar Data para evitar null
            }
            finally
            {
                // Cerrar o liberar la conexión (si es necesario)
                _dapperContext.Dispose();
            }
            return respuesta;
        }

        public async Task<RespuestaJson> AgregarNivelComplejidadTarea(NivelComplejidadTareas nivelcomplejidadtarea)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Abrir conexión y validar duplicados
                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Validar si ya existe un registro con el mismo IdSistema en la base de datos
                    string sqlValidarNivelComplejidadTareasDescripcion = "SELECT COUNT(*) FROM NivelComplejidadTareas WHERE NivelComplejidadTareaDescripcion = @NivelComplejidadTareaDescripcion;";
                    var parametrosValidarNivelComplejidadTareasDescripcion = new DynamicParameters();
                    parametrosValidarNivelComplejidadTareasDescripcion.Add("@NivelComplejidadTareaDescripcion", nivelcomplejidadtarea.NivelComplejidadTareaDescripcion, DbType.String);

                    int NivelComplejidadTareasDescripcionExistente = await connection.ExecuteScalarAsync<int>(sqlValidarNivelComplejidadTareasDescripcion, parametrosValidarNivelComplejidadTareasDescripcion);

                    if (NivelComplejidadTareasDescripcionExistente > 0)
                    {
                        respuesta.Mensaje = "Los soportes ya cuentan con este nivel de complejidad de tarea.";
                        return respuesta;
                    }

                    // Consulta SQL para insertar el nivelcomplejidadtarea
                    string sqlInsertar = @"
                    INSERT INTO NivelComplejidadTareas 
                    (NivelComplejidadTareaDescripcion, Detalles, IdUsuarioSet) 
                    VALUES 
                    (@NivelComplejidadTareaDescripcion, @Detalles, @IdUsuarioSet);";

                    var parametrosInsertar = new DynamicParameters();
                    parametrosInsertar.Add("@NivelComplejidadTareaDescripcion", nivelcomplejidadtarea.NivelComplejidadTareaDescripcion, DbType.String);
                    parametrosInsertar.Add("@Detalles", nivelcomplejidadtarea.Detalles, DbType.String);
                    parametrosInsertar.Add("@IdUsuarioSet", nivelcomplejidadtarea.IdUsuarioSet, DbType.Int32);

                    // Ejecutar la consulta de inserción
                    int filasAfectadas = await connection.ExecuteAsync(sqlInsertar, parametrosInsertar);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "El nivel de complejidad de tarea se agregó exitosamente.";
                        respuesta.Data = new { NivelComplejidadTareaId = nivelcomplejidadtarea.Id }; // Puedes devolver el ID del nivelcomplejidadtarea si lo necesitas
                    }
                    else
                    {
                        respuesta.Mensaje = "Hubo un error al agregar el nivel de complejidad de tarea.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Se produjo un error al intentar agregar el nivel de complejidad de tarea: " + ex.Message;
                respuesta.Errores.Add(ex.Message);
            }
            finally
            {
                _dapperContext.Dispose(); // Liberar recursos
            }

            return respuesta;
        }


        public async Task<RespuestaJson> EditarNivelComplejidadTarea(NivelComplejidadTareas nivelcomplejidadtarea)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Obtener la fecha y hora actual
                nivelcomplejidadtarea.FechaUpd = DateTime.Now;

                // Abrir conexión y validar duplicados
                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Obtener el valor actual de TipoSoporteDescripcion desde la base de datos
                    string sqlObtenerDescripcionActual = @"
                    SELECT NivelComplejidadTareaDescripcion 
                    FROM NivelComplejidadTareas 
                    WHERE Id = @Id;";

                    var parametrosObtenerDescripcion = new DynamicParameters();
                    parametrosObtenerDescripcion.Add("@Id", nivelcomplejidadtarea.Id, DbType.Int32);

                    string descripcionActual = await connection.QueryFirstOrDefaultAsync<string>(sqlObtenerDescripcionActual, parametrosObtenerDescripcion);

                    // Validar duplicados solo si la descripción está siendo modificada
                    if (descripcionActual != nivelcomplejidadtarea.NivelComplejidadTareaDescripcion)
                    {
                        string sqlValidarNivelComplejidadTareaDescripcion = @"
                        SELECT COUNT(*) 
                        FROM NivelComplejidadTareas 
                        WHERE NivelComplejidadTareaDescripcion = @NivelComplejidadTareaDescripcion 
                        AND Id != @Id;"; // Excluir el registro actual de la validación

                        var parametrosValidar = new DynamicParameters();
                        parametrosValidar.Add("@NivelComplejidadTareaDescripcion", nivelcomplejidadtarea.NivelComplejidadTareaDescripcion, DbType.String);
                        parametrosValidar.Add("@Id", nivelcomplejidadtarea.Id, DbType.Int32);

                        int existeDescripcion = await connection.ExecuteScalarAsync<int>(sqlValidarNivelComplejidadTareaDescripcion, parametrosValidar);

                        if (existeDescripcion > 0)
                        {
                            respuesta.Mensaje = "Ya existe un nivel de complejidad con la misma descripción.";
                            return respuesta;
                        }
                    }

                    // Construir la consulta SQL dinámicamente
                    var sqlActualizar = new StringBuilder("UPDATE NivelComplejidadTareas SET ");
                    var parametrosActualizar = new DynamicParameters();

                    sqlActualizar.Append("NivelComplejidadTareaDescripcion = @NivelComplejidadTareaDescripcion, ");
                    parametrosActualizar.Add("@NivelComplejidadTareaDescripcion", nivelcomplejidadtarea.NivelComplejidadTareaDescripcion, DbType.String);

                    if (!string.IsNullOrEmpty(nivelcomplejidadtarea.Detalles))
                    {
                        sqlActualizar.Append("Detalles = @Detalles, ");
                        parametrosActualizar.Add("@Detalles", nivelcomplejidadtarea.Detalles, DbType.String);
                    }

                    sqlActualizar.Append("IdUsuarioUpd = @IdUsuarioUpd, FechaUpd = @FechaUpd ");
                    sqlActualizar.Append("WHERE Id = @Id;");

                    parametrosActualizar.Add("@Id", nivelcomplejidadtarea.Id, DbType.Int32);
                    parametrosActualizar.Add("@IdUsuarioUpd", nivelcomplejidadtarea.IdUsuarioUpd, DbType.Int32);
                    parametrosActualizar.Add("@FechaUpd", nivelcomplejidadtarea.FechaUpd, DbType.DateTime);

                    int filasAfectadas = await connection.ExecuteAsync(sqlActualizar.ToString(), parametrosActualizar);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "Nivel de complejidad de la tarea actualizada con éxito.";
                    }
                    else
                    {
                        respuesta.Mensaje = "No fue posible actualizar el nivel de complejidad de la tarea.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Se produjo un error al actualizar el nivel de complejidad de la tarea: " + ex.Message;
                respuesta.Errores.Add(ex.Message);
            }
            finally
            {
                _dapperContext.Dispose(); // Liberar recursos
            }

            return respuesta;
        }

        public async Task<RespuestaJson> DesactivarNivelComplejidadTarea(NivelComplejidadTareas nivelcomplejidadtarea)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Obtener la fecha y hora actual
                nivelcomplejidadtarea.FechaDel = DateTime.Now;

                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Consulta SQL para desactivar el nivelcomplejidadtarea
                    string sql = @"
                    UPDATE NivelComplejidadTareas 
                    SET FlgActivo = 0, 
                        IdUsuarioDel = @IdUsuarioDel, 
                        FechaDel = @FechaDel 
                    WHERE Id = @Id";

                    var parametros = new DynamicParameters();
                    parametros.Add("@Id", nivelcomplejidadtarea.Id, DbType.Int32);
                    parametros.Add("@IdUsuarioDel", nivelcomplejidadtarea.IdUsuarioDel, DbType.Int32);
                    parametros.Add("@FechaDel", nivelcomplejidadtarea.FechaDel, DbType.DateTime);

                    // Ejecutar la consulta
                    int filasAfectadas = await connection.ExecuteAsync(sql, parametros);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "NivelComplejidadTarea desactivado correctamente.";
                    }
                    else
                    {
                        respuesta.Mensaje = "No se encontró el nivelcomplejidadtarea o ya está desactivado.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al desactivar el nivelcomplejidadtarea: " + ex.Message;
            }
            finally
            {
                _dapperContext.Dispose();
            }
            return respuesta;
        }

        public async Task<RespuestaJson> ReactivarNivelComplejidadTarea(NivelComplejidadTareas nivelcomplejidadtarea)
        {
            RespuestaJson respuesta = new RespuestaJson();
            try
            {
                // Obtener la fecha y hora actual
                nivelcomplejidadtarea.FechaUpd = DateTime.Now;

                using (var connection = _dapperContext.AbrirConexion("SGP"))
                {
                    // Consulta SQL para reactivar el nivelcomplejidadtarea
                    string sql = @"
                    UPDATE NivelComplejidadTareas 
                    SET FlgActivo = 1, 
                        IdUsuarioUpd = @IdUsuarioUpd, 
                        FechaUpd = @FechaUpd
                    WHERE Id = @Id";

                    var parametros = new DynamicParameters();
                    parametros.Add("@Id", nivelcomplejidadtarea.Id, DbType.Int32);
                    parametros.Add("@IdUsuarioUpd", nivelcomplejidadtarea.IdUsuarioUpd, DbType.Int32);
                    parametros.Add("@FechaUpd", nivelcomplejidadtarea.FechaUpd, DbType.DateTime);

                    // Ejecutar la consulta
                    int filasAfectadas = await connection.ExecuteAsync(sql, parametros);

                    if (filasAfectadas > 0)
                    {
                        respuesta.Resultado = true;
                        respuesta.Mensaje = "NivelComplejidadTarea reactivado correctamente.";
                    }
                    else
                    {
                        respuesta.Mensaje = "No se encontró el nivelcomplejidadtarea o ya está reactivado.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Ocurrió un error al reactivar el nivelcomplejidadtarea: " + ex.Message;
            }
            finally
            {
                _dapperContext.Dispose();
            }
            return respuesta;
        }
    }
}
