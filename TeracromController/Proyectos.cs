﻿using Dapper;
using GestionProyectos.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Text.RegularExpressions;
using TeracromDatabase;
using TeracromModels;

//namespace TeracromController
//{
//    public class Catalogos
//    {
//        private readonly DapperContext _dapperContext;

//        public Catalogos(DapperContext dapperContext)
//        {
//            _dapperContext = dapperContext;
//        }

//        //============================================================================================================================\\
//        //====================================================   Modelo Clientes   =================================================== \\
//        //==============================================================================================================================\\

//        public async Task<RespuestaJson> GetClientes()
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {
//                // Abrir la conexión a la base de datos
//                _dapperContext.AbrirConexion("SGP");

//                string sql = "SELECT Id, RazonSocial, RFC, Prefijo, Telefono, Logotipo, FlgActivo FROM Clientes";
//                var clientes = await _dapperContext.QueryAsync<Clientes>(sql);
//                if (clientes != null)
//                {
//                    respuesta.Resultado = true;
//                    respuesta.Data = clientes.Select(s => new Clientes
//                    {
//                        Id = s.Id,
//                        RazonSocial = s.RazonSocial,
//                        RFC = s.RFC,
//                        Prefijo = s.Prefijo,
//                        Telefono = s.Telefono,
//                        Logotipo = s.Logotipo,
//                        FlgActivo = s.FlgActivo
//                    }).ToList();
//                }
//                else
//                {
//                    respuesta.Mensaje = "No se encontraron clientes activos.";
//                    respuesta.Data = new List<Clientes>(); // Inicializar Data para evitar null
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al obtener los datos de los clientes." + ex.Message;
//                respuesta.Data = new List<Clientes>(); // Inicializar Data para evitar null
//            }
//            finally
//            {
//                // Cerrar o liberar la conexión (si es necesario)
//                _dapperContext.Dispose();
//            }
//            return respuesta;
//        }

//        public async Task<RespuestaJson> AgregarCliente(Clientes cliente)
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {
//                // Validar que el archivo de imagen esté presente y sea válido
//                if (cliente.LogotipoFile != null && cliente.LogotipoFile.Length > 0)
//                {
//                    using (var memoryStream = new MemoryStream())
//                    {
//                        await cliente.LogotipoFile.CopyToAsync(memoryStream);
//                        cliente.Logotipo = memoryStream.ToArray(); // Convierte la imagen a byte[]
//                    }
//                }

//                // Convertir RFC y Prefijo a mayúsculas
//                cliente.RFC = cliente.RFC?.ToUpper();
//                cliente.Prefijo = cliente.Prefijo?.ToUpper();

//                // Eliminar caracteres no numéricos del número de teléfono
//                if (!string.IsNullOrEmpty(cliente.Telefono))
//                {
//                    cliente.Telefono = Regex.Replace(cliente.Telefono, @"[^\d]", "");
//                }

//                // Abrir conexión y validar duplicados
//                using (var connection = _dapperContext.AbrirConexion("SGP"))
//                {
//                    // Validar si la Razon Social ya existe en la base de datos
//                    string sqlValidarRazonSocial = "SELECT COUNT(*) FROM Clientes WHERE RazonSocial = @RazonSocial;";
//                    var parametrosValidarRazonSocial = new DynamicParameters();
//                    parametrosValidarRazonSocial.Add("@RazonSocial", cliente.RazonSocial, DbType.String);

//                    int RazonSocialExistente = await connection.ExecuteScalarAsync<int>(sqlValidarRazonSocial, parametrosValidarRazonSocial);

//                    if (RazonSocialExistente > 0)
//                    {
//                        respuesta.Mensaje = "La razón social ya existe en la base de datos.";
//                        return respuesta;
//                    }

//                    // Validar si el Prefijo ya existe en la base de datos
//                    string sqlValidarPrefijo = "SELECT COUNT(*) FROM Clientes WHERE Prefijo = @Prefijo;";
//                    var parametrosValidarPrefijo = new DynamicParameters();
//                    parametrosValidarPrefijo.Add("@Prefijo", cliente.Prefijo, DbType.String);

//                    int PrefijoExistente = await connection.ExecuteScalarAsync<int>(sqlValidarPrefijo, parametrosValidarPrefijo);

//                    if (PrefijoExistente > 0)
//                    {
//                        respuesta.Mensaje = "El prefijo ya existe en la base de datos.";
//                        return respuesta;
//                    }

//                    // Validar si el RFC ya existe en la base de datos
//                    string sqlValidarRFC = "SELECT COUNT(*) FROM Clientes WHERE RFC = @RFC;";
//                    var parametrosValidarRFC = new DynamicParameters();
//                    parametrosValidarRFC.Add("@RFC", cliente.RFC, DbType.String);

//                    int rfcExistente = await connection.ExecuteScalarAsync<int>(sqlValidarRFC, parametrosValidarRFC);

//                    if (rfcExistente > 0)
//                    {
//                        respuesta.Mensaje = "El RFC ya existe en la base de datos.";
//                        return respuesta;
//                    }

//                    // Validar si el número de teléfono ya existe en la base de datos
//                    string sqlValidarTelefono = "SELECT COUNT(*) FROM Clientes WHERE Telefono = @Telefono;";
//                    var parametrosValidarTelefono = new DynamicParameters();
//                    parametrosValidarTelefono.Add("@Telefono", cliente.Telefono, DbType.String);

//                    int telefonoExistente = await connection.ExecuteScalarAsync<int>(sqlValidarTelefono, parametrosValidarTelefono);

//                    if (telefonoExistente > 0)
//                    {
//                        respuesta.Mensaje = "El número de teléfono ya existe en la base de datos.";
//                        return respuesta;
//                    }

//                    // Consulta SQL para insertar el cliente
//                    string sqlInsertar = @"
//                    INSERT INTO Clientes 
//                    (RazonSocial, RFC, Prefijo, Telefono, Logotipo, IdUsuarioSet) 
//                    VALUES 
//                    (@RazonSocial, @RFC, @Prefijo, @Telefono, @Logotipo, @IdUsuarioSet);";

//                    var parametrosInsertar = new DynamicParameters();
//                    parametrosInsertar.Add("@RazonSocial", cliente.RazonSocial, DbType.String);
//                    parametrosInsertar.Add("@RFC", cliente.RFC, DbType.String);
//                    parametrosInsertar.Add("@Prefijo", cliente.Prefijo, DbType.String);
//                    parametrosInsertar.Add("@Telefono", cliente.Telefono, DbType.String);
//                    parametrosInsertar.Add("@Logotipo", cliente.Logotipo, DbType.Binary);
//                    parametrosInsertar.Add("@IdUsuarioSet", cliente.IdUsuarioSet, DbType.Int32);

//                    // Ejecutar la consulta de inserción
//                    int filasAfectadas = await connection.ExecuteAsync(sqlInsertar, parametrosInsertar);

//                    if (filasAfectadas > 0)
//                    {
//                        respuesta.Resultado = true;
//                        respuesta.Mensaje = "Cliente agregado exitosamente.";
//                        respuesta.Data = new { ClienteId = cliente.Id }; // Puedes devolver el ID del cliente si lo necesitas
//                    }
//                    else
//                    {
//                        respuesta.Mensaje = "No se pudo agregar el cliente.";
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al agregar el cliente: " + ex.Message;
//                respuesta.Errores.Add(ex.Message);
//            }
//            finally
//            {
//                _dapperContext.Dispose(); // Liberar recursos
//            }

//            return respuesta;
//        }


//        public async Task<RespuestaJson> EditarCliente(Clientes cliente)
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {
//                // Validar que el archivo de imagen esté presente y sea válido
//                if (cliente.LogotipoFile != null && cliente.LogotipoFile.Length > 0)
//                {
//                    using (var memoryStream = new MemoryStream())
//                    {
//                        await cliente.LogotipoFile.CopyToAsync(memoryStream);
//                        cliente.Logotipo = memoryStream.ToArray(); // Convierte la imagen a byte[]
//                    }
//                }
//                else if (!string.IsNullOrEmpty(cliente.LogotipoBase64))
//                {
//                    // Si no hay una nueva imagen, pero hay una imagen existente en base64, convertirla a byte[]
//                    cliente.Logotipo = Convert.FromBase64String(cliente.LogotipoBase64.Split(',')[1]);
//                }

//                // Convertir RFC y Prefijo a mayúsculas
//                cliente.RFC = cliente.RFC?.ToUpper();
//                cliente.Prefijo = cliente.Prefijo?.ToUpper();

//                // Eliminar caracteres no numéricos del número de teléfono
//                if (!string.IsNullOrEmpty(cliente.Telefono))
//                {
//                    cliente.Telefono = Regex.Replace(cliente.Telefono, @"[^\d]", "");
//                }

//                // Obtener la fecha y hora actual
//                cliente.FechaUpd = DateTime.Now;

//                // Abrir conexión y validar duplicados
//                using (var connection = _dapperContext.AbrirConexion("SGP"))
//                {
//                    // Consulta SQL para actualizar el cliente
//                    string sqlActualizar = @"
//                    UPDATE Clientes 
//                    SET RazonSocial = @RazonSocial, 
//                        RFC = @RFC, 
//                        Prefijo = @Prefijo, 
//                        Telefono = @Telefono, 
//                        Logotipo = @Logotipo,
//                        IdUsuarioUpd = @IdUsuarioUpd,
//                        FechaUpd = @FechaUpd
//                    WHERE Id = @Id;";

//                    var parametrosActualizar = new DynamicParameters();
//                    parametrosActualizar.Add("@Id", cliente.Id, DbType.Int32);
//                    parametrosActualizar.Add("@RazonSocial", cliente.RazonSocial, DbType.String);
//                    parametrosActualizar.Add("@RFC", cliente.RFC, DbType.String);
//                    parametrosActualizar.Add("@Prefijo", cliente.Prefijo, DbType.String);
//                    parametrosActualizar.Add("@Telefono", cliente.Telefono, DbType.String);
//                    parametrosActualizar.Add("@Logotipo", cliente.Logotipo, DbType.Binary);
//                    parametrosActualizar.Add("@IdUsuarioUpd", cliente.IdUsuarioUpd, DbType.Int32);
//                    parametrosActualizar.Add("@FechaUpd", cliente.FechaUpd, DbType.DateTime);

//                    // Ejecutar la consulta de actualización
//                    int filasAfectadas = await connection.ExecuteAsync(sqlActualizar, parametrosActualizar);

//                    if (filasAfectadas > 0)
//                    {
//                        respuesta.Resultado = true;
//                        respuesta.Mensaje = "Cliente actualizado exitosamente.";
//                    }
//                    else
//                    {
//                        respuesta.Mensaje = "No se pudo actualizar el cliente.";
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al actualizar el cliente: " + ex.Message;
//                respuesta.Errores.Add(ex.Message);
//            }
//            finally
//            {
//                _dapperContext.Dispose(); // Liberar recursos
//            }

//            return respuesta;
//        }

//        public async Task<RespuestaJson> DesactivarCliente(Clientes cliente)
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {

//                // Obtener la fecha y hora actual
//                cliente.FechaDel = DateTime.Now;

//                using (var connection = _dapperContext.AbrirConexion("SGP"))
//                {
//                    // Consulta SQL para desactivar el cliente
//                    string sql = @"
//                    UPDATE Clientes 
//                    SET FlgActivo = 0, 
//                        IdUsuarioDel = @IdUsuarioDel, 
//                        FechaDel = @FechaDel 
//                    WHERE Id = @Id";

//                    var parametros = new DynamicParameters();
//                    parametros.Add("@Id", cliente.Id, DbType.Int32);
//                    parametros.Add("@IdUsuarioDel", cliente.IdUsuarioDel, DbType.Int32);
//                    parametros.Add("@FechaDel", cliente.FechaDel, DbType.DateTime);

//                    // Ejecutar la consulta
//                    int filasAfectadas = await connection.ExecuteAsync(sql, parametros);

//                    if (filasAfectadas > 0)
//                    {
//                        respuesta.Resultado = true;
//                        respuesta.Mensaje = "Cliente desactivado correctamente.";
//                    }
//                    else
//                    {
//                        respuesta.Mensaje = "No se encontró el cliente o ya está desactivado.";
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al desactivar el cliente: " + ex.Message;
//            }
//            finally
//            {
//                _dapperContext.Dispose();
//            }
//            return respuesta;
//        }

//        public async Task<RespuestaJson> ReactivarCliente(Clientes cliente)
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {

//                // Obtener la fecha y hora actual
//                cliente.FechaUpd = DateTime.Now;

//                using (var connection = _dapperContext.AbrirConexion("SGP"))
//                {
//                    // Consulta SQL para desactivar el cliente
//                    string sql = @"
//                    UPDATE Clientes 
//                    SET FlgActivo = 1, 
//                        IdUsuarioUpd = @IdUsuarioUpd, 
//                        FechaUpd = @FechaUpd
//                    WHERE Id = @Id";

//                    var parametros = new DynamicParameters();
//                    parametros.Add("@Id", cliente.Id, DbType.Int32);
//                    parametros.Add("@IdUsuarioUpd", cliente.IdUsuarioUpd, DbType.Int32);
//                    parametros.Add("@FechaUpd", cliente.FechaUpd, DbType.DateTime);

//                    // Ejecutar la consulta
//                    int filasAfectadas = await connection.ExecuteAsync(sql, parametros);

//                    if (filasAfectadas > 0)
//                    {
//                        respuesta.Resultado = true;
//                        respuesta.Mensaje = "Cliente reactivado correctamente.";
//                    }
//                    else
//                    {
//                        respuesta.Mensaje = "No se encontró el cliente o ya está reactivado.";
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al reactivar el cliente: " + ex.Message;
//            }
//            finally
//            {
//                _dapperContext.Dispose();
//            }
//            return respuesta;
//        }

//        //============================================================================================================================\\
//        //====================================================   Modelo Puestos   ==================================================== \\
//        //==============================================================================================================================\\

//        public async Task<RespuestaJson> GetPuestos()
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {
//                // Abrir la conexión a la base de datos
//                _dapperContext.AbrirConexion("SGP");

//                string sql = "SELECT Id, Descripcion, Tipo, FlgActivo FROM Puestos";
//                var puestos = await _dapperContext.QueryAsync<Puestos>(sql);
//                if (puestos != null)
//                {
//                    respuesta.Resultado = true;
//                    respuesta.Data = puestos.Select(s => new Puestos
//                    {
//                        Id = s.Id,
//                        Descripcion = s.Descripcion,
//                        Tipo = s.Tipo,
//                        FlgActivo = s.FlgActivo
//                    }).ToList();
//                }
//                else
//                {
//                    respuesta.Mensaje = "No se encontraron puestos activos.";
//                    respuesta.Data = new List<Puestos>(); // Inicializar Data para evitar null
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al obtener los datos de los puestos." + ex.Message;
//                respuesta.Data = new List<Puestos>(); // Inicializar Data para evitar null
//            }
//            finally
//            {
//                // Cerrar o liberar la conexión (si es necesario)
//                _dapperContext.Dispose();
//            }
//            return respuesta;
//        }

//        public async Task<RespuestaJson> AgregarPuesto(Puestos puesto)
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {

//                // Abrir conexión y validar duplicados
//                using (var connection = _dapperContext.AbrirConexion("SGP"))
//                {

//                    // Validar si la Descripcion ya existe en la base de datos
//                    string sqlValidarDescripcion = "SELECT COUNT(*) FROM Puestos WHERE Descripcion = @Descripcion;";
//                    var parametrosValidarDescripcion = new DynamicParameters();
//                    parametrosValidarDescripcion.Add("@Descripcion", puesto.Descripcion, DbType.String);

//                    int DescripcionExistente = await connection.ExecuteScalarAsync<int>(sqlValidarDescripcion, parametrosValidarDescripcion);

//                    if (DescripcionExistente > 0)
//                    {
//                        respuesta.Mensaje = "El prefijo ya existe en la base de datos.";
//                        return respuesta;
//                    }

//                    // Consulta SQL para insertar el puesto
//                    string sqlInsertar = @"
//                    INSERT INTO Puestos 
//                    (Descripcion, Tipo, IdUsuarioSet) 
//                    VALUES 
//                    (@Descripcion, @Tipo, @IdUsuarioSet);";

//                    var parametrosInsertar = new DynamicParameters();
//                    parametrosInsertar.Add("@Descripcion", puesto.Descripcion, DbType.String);
//                    parametrosInsertar.Add("@Tipo", puesto.Tipo, DbType.String);
//                    parametrosInsertar.Add("@IdUsuarioSet", puesto.IdUsuarioSet, DbType.Int32);

//                    // Ejecutar la consulta de inserción
//                    int filasAfectadas = await connection.ExecuteAsync(sqlInsertar, parametrosInsertar);

//                    if (filasAfectadas > 0)
//                    {
//                        respuesta.Resultado = true;
//                        respuesta.Mensaje = "Puesto agregado exitosamente.";
//                        respuesta.Data = new { PuestoId = puesto.Id }; // Puedes devolver el ID del puesto si lo necesitas
//                    }
//                    else
//                    {
//                        respuesta.Mensaje = "No se pudo agregar el puesto.";
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al agregar el puesto: " + ex.Message;
//                respuesta.Errores.Add(ex.Message);
//            }
//            finally
//            {
//                _dapperContext.Dispose(); // Liberar recursos
//            }

//            return respuesta;
//        }

//        public async Task<RespuestaJson> EditarPuesto(Puestos puesto)
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {

//                // Obtener la fecha y hora actual
//                puesto.FechaUpd = DateTime.Now;

//                // Abrir conexión y validar duplicados
//                using (var connection = _dapperContext.AbrirConexion("SGP"))
//                {
//                    // Consulta SQL para actualizar el puesto
//                    string sqlActualizar = @"
//                    UPDATE Puestos 
//                    SET Descripcion = @Descripcion, 
//                        Tipo = @Tipo, 
//                        IdUsuarioUpd = @IdUsuarioUpd,
//                        FechaUpd = @FechaUpd
//                    WHERE Id = @Id;";

//                    var parametrosActualizar = new DynamicParameters();
//                    parametrosActualizar.Add("@Id", puesto.Id, DbType.Int32);
//                    parametrosActualizar.Add("@Descripcion", puesto.Descripcion, DbType.String);
//                    parametrosActualizar.Add("@Tipo", puesto.Tipo, DbType.String);
//                    parametrosActualizar.Add("@IdUsuarioUpd", puesto.IdUsuarioUpd, DbType.Int32);
//                    parametrosActualizar.Add("@FechaUpd", puesto.FechaUpd, DbType.DateTime);

//                    // Ejecutar la consulta de actualización
//                    int filasAfectadas = await connection.ExecuteAsync(sqlActualizar, parametrosActualizar);

//                    if (filasAfectadas > 0)
//                    {
//                        respuesta.Resultado = true;
//                        respuesta.Mensaje = "Puesto actualizado exitosamente.";
//                    }
//                    else
//                    {
//                        respuesta.Mensaje = "No se pudo actualizar el puesto.";
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al actualizar el puesto: " + ex.Message;
//                respuesta.Errores.Add(ex.Message);
//            }
//            finally
//            {
//                _dapperContext.Dispose(); // Liberar recursos
//            }

//            return respuesta;
//        }

//        public async Task<RespuestaJson> DesactivarPuesto(Puestos puesto)
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {
//                // Obtener la fecha y hora actual
//                puesto.FechaDel = DateTime.Now;

//                using (var connection = _dapperContext.AbrirConexion("SGP"))
//                {
//                    // Consulta SQL para desactivar el puesto
//                    string sql = @"
//                    UPDATE Puestos 
//                    SET FlgActivo = 0, 
//                        IdUsuarioDel = @IdUsuarioDel, 
//                        FechaDel = @FechaDel 
//                    WHERE Id = @Id";

//                    var parametros = new DynamicParameters();
//                    parametros.Add("@Id", puesto.Id, DbType.Int32);
//                    parametros.Add("@IdUsuarioDel", puesto.IdUsuarioDel, DbType.Int32);
//                    parametros.Add("@FechaDel", puesto.FechaDel, DbType.DateTime);

//                    // Ejecutar la consulta
//                    int filasAfectadas = await connection.ExecuteAsync(sql, parametros);

//                    if (filasAfectadas > 0)
//                    {
//                        respuesta.Resultado = true;
//                        respuesta.Mensaje = "Puesto desactivado correctamente.";
//                    }
//                    else
//                    {
//                        respuesta.Mensaje = "No se encontró el puesto o ya está desactivado.";
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al desactivar el puesto: " + ex.Message;
//            }
//            finally
//            {
//                _dapperContext.Dispose();
//            }
//            return respuesta;
//        }

//        public async Task<RespuestaJson> ReactivarPuesto(Puestos puesto)
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {
//                // Obtener la fecha y hora actual
//                puesto.FechaUpd = DateTime.Now;

//                using (var connection = _dapperContext.AbrirConexion("SGP"))
//                {
//                    // Consulta SQL para reactivar el puesto
//                    string sql = @"
//                    UPDATE Puestos 
//                    SET FlgActivo = 1, 
//                        IdUsuarioUpd = @IdUsuarioUpd, 
//                        FechaUpd = @FechaUpd
//                    WHERE Id = @Id";

//                    var parametros = new DynamicParameters();
//                    parametros.Add("@Id", puesto.Id, DbType.Int32);
//                    parametros.Add("@IdUsuarioUpd", puesto.IdUsuarioUpd, DbType.Int32);
//                    parametros.Add("@FechaUpd", puesto.FechaUpd, DbType.DateTime);

//                    // Ejecutar la consulta
//                    int filasAfectadas = await connection.ExecuteAsync(sql, parametros);

//                    if (filasAfectadas > 0)
//                    {
//                        respuesta.Resultado = true;
//                        respuesta.Mensaje = "Puesto reactivado correctamente.";
//                    }
//                    else
//                    {
//                        respuesta.Mensaje = "No se encontró el puesto o ya está reactivado.";
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al reactivar el puesto: " + ex.Message;
//            }
//            finally
//            {
//                _dapperContext.Dispose();
//            }
//            return respuesta;
//        }

//        //============================================================================================================================\\
//        //====================================================   Modelo Sistemas   =================================================== \\
//        //==============================================================================================================================\\

//        public async Task<RespuestaJson> GetSistemas()
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {
//                // Abrir la conexión a la base de datos
//                _dapperContext.AbrirConexion("SGP");

//                string sql = @"
//                SELECT 
//                    s.Id, 
//                    s.Descripcion, 
//                    c.RazonSocial,  -- Obtener la RazonSocial del cliente
//                    s.Repositorio, 
//                    s.Prefijo, 
//                    s.FlgActivo 
//                FROM 
//                    Sistemas s
//                INNER JOIN 
//                    Clientes c ON s.IdCliente = c.Id";
//                var sistemas = await _dapperContext.QueryAsync<Sistemas>(sql);
//                if (sistemas != null)
//                {
//                    respuesta.Resultado = true;
//                    respuesta.Data = sistemas.Select(s => new Sistemas
//                    {
//                        Id = s.Id,
//                        Descripcion = s.Descripcion,
//                        RazonSocial = s.RazonSocial,
//                        Repositorio = s.Repositorio,
//                        Prefijo = s.Prefijo,
//                        FlgActivo = s.FlgActivo
//                    }).ToList();
//                }
//                else
//                {
//                    respuesta.Mensaje = "No se encontraron sistemas activos.";
//                    respuesta.Data = new List<Sistemas>(); // Inicializar Data para evitar null
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al obtener los datos de los sistemas." + ex.Message;
//                respuesta.Data = new List<Sistemas>(); // Inicializar Data para evitar null
//            }
//            finally
//            {
//                // Cerrar o liberar la conexión (si es necesario)
//                _dapperContext.Dispose();
//            }
//            return respuesta;
//        }

//        public async Task<RespuestaJson> GetSistemasClientes()
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {
//                // Abrir la conexión a la base de datos
//                _dapperContext.AbrirConexion("SGP");

//                string sql = @"
//                SELECT 
//                    Id, 
//                    RazonSocial 
//                FROM 
//                    Clientes";

//                var clientes = await _dapperContext.QueryAsync<Clientes>(sql);
//                if (clientes != null)
//                {
//                    respuesta.Resultado = true;
//                    respuesta.Data = clientes.Select(c => new
//                    {
//                        Id = c.Id,
//                        RazonSocial = c.RazonSocial
//                    }).ToList();
//                }
//                else
//                {
//                    respuesta.Mensaje = "No se encontraron clientes.";
//                    respuesta.Data = new List<Clientes>(); // Inicializar Data para evitar null
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al obtener los datos de los clientes." + ex.Message;
//                respuesta.Data = new List<Clientes>(); // Inicializar Data para evitar null
//            }
//            finally
//            {
//                // Cerrar o liberar la conexión (si es necesario)
//                _dapperContext.Dispose();
//            }
//            return respuesta;
//        }

//        public async Task<RespuestaJson> AgregarSistema(Sistemas sistema)
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {
//                sistema.Prefijo = sistema.Prefijo?.ToUpper();

//                // Abrir conexión y validar duplicados
//                using (var connection = _dapperContext.AbrirConexion("SGP"))
//                {
//                    // Validar si la Descripcion ya existe en la base de datos
//                    string sqlValidarDescripcion = "SELECT COUNT(*) FROM Sistemas WHERE Descripcion = @Descripcion;";
//                    var parametrosValidarDescripcion = new DynamicParameters();
//                    parametrosValidarDescripcion.Add("@Descripcion", sistema.Descripcion, DbType.String);

//                    int DescripcionExistente = await connection.ExecuteScalarAsync<int>(sqlValidarDescripcion, parametrosValidarDescripcion);

//                    if (DescripcionExistente > 0)
//                    {
//                        respuesta.Mensaje = "La Descripcion ya existe en la base de datos.";
//                        return respuesta;
//                    }

//                    // Validar si el Repositorio ya existe en la base de datos
//                    string sqlValidarRepositorio = "SELECT COUNT(*) FROM Sistemas WHERE Repositorio = @Repositorio;";
//                    var parametrosValidarRepositorio = new DynamicParameters();
//                    parametrosValidarRepositorio.Add("@Repositorio", sistema.Repositorio, DbType.String);

//                    int RepositorioExistente = await connection.ExecuteScalarAsync<int>(sqlValidarRepositorio, parametrosValidarRepositorio);

//                    if (RepositorioExistente > 0)
//                    {
//                        respuesta.Mensaje = "El repositorio ya existe en la base de datos.";
//                        return respuesta;
//                    }

//                    // Validar si el Prefijo ya existe en la base de datos
//                    string sqlValidarPrefijo = "SELECT COUNT(*) FROM Sistemas WHERE Prefijo = @Prefijo;";
//                    var parametrosValidarPrefijo = new DynamicParameters();
//                    parametrosValidarPrefijo.Add("@Prefijo", sistema.Prefijo, DbType.String);

//                    int PrefijoExistente = await connection.ExecuteScalarAsync<int>(sqlValidarPrefijo, parametrosValidarPrefijo);

//                    if (PrefijoExistente > 0)
//                    {
//                        respuesta.Mensaje = "El prefijo ya existe en la base de datos.";
//                        return respuesta;
//                    }

//                    // Consulta SQL para insertar el sistema
//                    string sqlInsertar = @"
//                    INSERT INTO Sistemas 
//                    (Descripcion, IdCliente, Repositorio, Prefijo, IdUsuarioSet) 
//                    VALUES 
//                    (@Descripcion, @IdCliente, @Repositorio, @Prefijo, @IdUsuarioSet);";

//                    var parametrosInsertar = new DynamicParameters();
//                    parametrosInsertar.Add("@Descripcion", sistema.Descripcion, DbType.String);
//                    parametrosInsertar.Add("@IdCliente", sistema.IdCliente, DbType.String);
//                    parametrosInsertar.Add("@Repositorio", sistema.Repositorio, DbType.String);
//                    parametrosInsertar.Add("@Prefijo", sistema.Prefijo, DbType.String);
//                    parametrosInsertar.Add("@IdUsuarioSet", sistema.IdUsuarioSet, DbType.Int32);

//                    // Ejecutar la consulta de inserción
//                    int filasAfectadas = await connection.ExecuteAsync(sqlInsertar, parametrosInsertar);

//                    if (filasAfectadas > 0)
//                    {
//                        respuesta.Resultado = true;
//                        respuesta.Mensaje = "Sistema agregado exitosamente.";
//                        respuesta.Data = new { SistemaId = sistema.Id }; // Puedes devolver el ID del sistema si lo necesitas
//                    }
//                    else
//                    {
//                        respuesta.Mensaje = "No se pudo agregar el sistema.";
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al agregar el sistema: " + ex.Message;
//                respuesta.Errores.Add(ex.Message);
//            }
//            finally
//            {
//                _dapperContext.Dispose(); // Liberar recursos
//            }

//            return respuesta;
//        }

//        public async Task<RespuestaJson> EditarSistema(Sistemas sistema)
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {
//                sistema.Prefijo = sistema.Prefijo?.ToUpper();
//                // Obtener la fecha y hora actual
//                sistema.FechaUpd = DateTime.Now;

//                // Abrir conexión y validar duplicados
//                using (var connection = _dapperContext.AbrirConexion("SGP"))
//                {
//                    // Consulta SQL base para actualizar el sistema
//                    string sqlActualizar = @"
//                    UPDATE Sistemas 
//                    SET Descripcion = @Descripcion, 
//                        Repositorio = @Repositorio, 
//                        Prefijo = @Prefijo, 
//                        IdUsuarioUpd = @IdUsuarioUpd,
//                        FechaUpd = @FechaUpd";

//                    // Agregar IdCliente a la consulta solo si tiene un valor válido (> 0)
//                    if (sistema.IdCliente > 0)
//                    {
//                        sqlActualizar += ", IdCliente = @IdCliente";
//                    }

//                    sqlActualizar += " WHERE Id = @Id;";

//                    var parametrosActualizar = new DynamicParameters();
//                    parametrosActualizar.Add("@Id", sistema.Id, DbType.Int32);
//                    parametrosActualizar.Add("@Descripcion", sistema.Descripcion, DbType.String);
//                    parametrosActualizar.Add("@Repositorio", sistema.Repositorio, DbType.String);
//                    parametrosActualizar.Add("@Prefijo", sistema.Prefijo, DbType.String);
//                    parametrosActualizar.Add("@IdUsuarioUpd", sistema.IdUsuarioUpd, DbType.Int32);
//                    parametrosActualizar.Add("@FechaUpd", sistema.FechaUpd, DbType.DateTime);

//                    // Agregar IdCliente a los parámetros solo si tiene un valor válido (> 0)
//                    if (sistema.IdCliente > 0)
//                    {
//                        parametrosActualizar.Add("@IdCliente", sistema.IdCliente, DbType.Int32);
//                    }

//                    // Ejecutar la consulta de actualización
//                    int filasAfectadas = await connection.ExecuteAsync(sqlActualizar, parametrosActualizar);

//                    if (filasAfectadas > 0)
//                    {
//                        respuesta.Resultado = true;
//                        respuesta.Mensaje = "Sistema actualizado exitosamente.";
//                    }
//                    else
//                    {
//                        respuesta.Mensaje = "No se pudo actualizar el sistema.";
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al actualizar el sistema: " + ex.Message;
//                respuesta.Errores.Add(ex.Message);
//            }
//            finally
//            {
//                _dapperContext.Dispose(); // Liberar recursos
//            }

//            return respuesta;
//        }


//        public async Task<RespuestaJson> DesactivarSistema(Sistemas sistema)
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {
//                // Obtener la fecha y hora actual
//                sistema.FechaDel = DateTime.Now;

//                using (var connection = _dapperContext.AbrirConexion("SGP"))
//                {
//                    // Consulta SQL para desactivar el sistema
//                    string sql = @"
//                    UPDATE Sistemas 
//                    SET FlgActivo = 0, 
//                        IdUsuarioDel = @IdUsuarioDel, 
//                        FechaDel = @FechaDel 
//                    WHERE Id = @Id";

//                    var parametros = new DynamicParameters();
//                    parametros.Add("@Id", sistema.Id, DbType.Int32);
//                    parametros.Add("@IdUsuarioDel", sistema.IdUsuarioDel, DbType.Int32);
//                    parametros.Add("@FechaDel", sistema.FechaDel, DbType.DateTime);

//                    // Ejecutar la consulta
//                    int filasAfectadas = await connection.ExecuteAsync(sql, parametros);

//                    if (filasAfectadas > 0)
//                    {
//                        respuesta.Resultado = true;
//                        respuesta.Mensaje = "Sistema desactivado correctamente.";
//                    }
//                    else
//                    {
//                        respuesta.Mensaje = "No se encontró el sistema o ya está desactivado.";
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al desactivar el sistema: " + ex.Message;
//            }
//            finally
//            {
//                _dapperContext.Dispose();
//            }
//            return respuesta;
//        }

//        public async Task<RespuestaJson> ReactivarSistema(Sistemas sistema)
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {
//                // Obtener la fecha y hora actual
//                sistema.FechaUpd = DateTime.Now;

//                using (var connection = _dapperContext.AbrirConexion("SGP"))
//                {
//                    // Consulta SQL para reactivar el sistema
//                    string sql = @"
//                    UPDATE Sistemas 
//                    SET FlgActivo = 1, 
//                        IdUsuarioUpd = @IdUsuarioUpd, 
//                        FechaUpd = @FechaUpd
//                    WHERE Id = @Id";

//                    var parametros = new DynamicParameters();
//                    parametros.Add("@Id", sistema.Id, DbType.Int32);
//                    parametros.Add("@IdUsuarioUpd", sistema.IdUsuarioUpd, DbType.Int32);
//                    parametros.Add("@FechaUpd", sistema.FechaUpd, DbType.DateTime);

//                    // Ejecutar la consulta
//                    int filasAfectadas = await connection.ExecuteAsync(sql, parametros);

//                    if (filasAfectadas > 0)
//                    {
//                        respuesta.Resultado = true;
//                        respuesta.Mensaje = "Sistema reactivado correctamente.";
//                    }
//                    else
//                    {
//                        respuesta.Mensaje = "No se encontró el sistema o ya está reactivado.";
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al reactivar el sistema: " + ex.Message;
//            }
//            finally
//            {
//                _dapperContext.Dispose();
//            }
//            return respuesta;
//        }

//        //============================================================================================================================\\
//        //====================================================   Modelo Usuarios   =================================================== \\
//        //==============================================================================================================================\\

//        public async Task<RespuestaJson> GetUsuarios()
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {
//                // Abrir la conexión a la base de datos
//                _dapperContext.AbrirConexion("SGP");
//                string sql = @"SELECT 
//                                s.Id, 
//                                s.Usuario, 
//                                s.NombreUsuario, 
//                                s.ApellidoPaternoUsuario, 
//                                s.ApellidoMaternoUsuario, 
//                                c.Descripcion, 
//                                s.Telefono, 
//                                s.Correo, 
//                                s.FotoPerfil, 
//                                s.FlgActivo 
//                            FROM 
//                                Usuarios s 
//                            INNER JOIN 
//                                Puestos c ON s.IdPuesto = c.Id;";
//                var usuarios = await _dapperContext.QueryAsync<Usuarios>(sql);
//                if (usuarios != null)
//                {
//                    respuesta.Resultado = true;
//                    respuesta.Data = usuarios.Select(s => new Usuarios
//                    {
//                        Id = s.Id,
//                        Usuario = s.Usuario,
//                        NombreUsuario = s.NombreUsuario,
//                        ApellidoPaternoUsuario = s.ApellidoPaternoUsuario,
//                        ApellidoMaternoUsuario = s.ApellidoMaternoUsuario,
//                        Descripcion = s.Descripcion,
//                        Telefono = s.Telefono,
//                        Correo = s.Correo,
//                        FotoPerfil = s.FotoPerfil,
//                        FlgActivo = s.FlgActivo
//                    }).ToList();
//                }
//                else
//                {
//                    respuesta.Mensaje = "No se encontraron usuarios activos.";
//                    respuesta.Data = new List<Usuarios>(); // Inicializar Data para evitar null
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al obtener los datos de los usuarios." + ex.Message;
//                respuesta.Data = new List<Usuarios>(); // Inicializar Data para evitar null
//            }
//            finally
//            {
//                // Cerrar o liberar la conexión (si es necesario)
//                _dapperContext.Dispose();
//            }
//            return respuesta;
//        }

//        public async Task<RespuestaJson> GetUsuariosPuestos()
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {
//                // Abrir la conexión a la base de datos
//                _dapperContext.AbrirConexion("SGP");

//                string sql = @"
//                SELECT 
//                    Id, 
//                    Descripcion 
//                FROM 
//                    Puestos WHERE Tipo = 1";

//                var puestos = await _dapperContext.QueryAsync<Puestos>(sql);
//                if (puestos != null)
//                {
//                    respuesta.Resultado = true;
//                    respuesta.Data = puestos.Select(c => new
//                    {
//                        Id = c.Id,
//                        Descripcion = c.Descripcion
//                    }).ToList();
//                }
//                else
//                {
//                    respuesta.Mensaje = "No se encontraron clientes.";
//                    respuesta.Data = new List<Puestos>(); // Inicializar Data para evitar null
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al obtener los datos de los clientes." + ex.Message;
//                respuesta.Data = new List<Puestos>(); // Inicializar Data para evitar null
//            }
//            finally
//            {
//                // Cerrar o liberar la conexión (si es necesario)
//                _dapperContext.Dispose();
//            }
//            return respuesta;
//        }

//        public async Task<RespuestaJson> AgregarUsuario(Usuarios usuario)
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {
//                // Validar que el archivo de imagen esté presente y sea válido
//                if (usuario.FotoFile != null && usuario.FotoFile.Length > 0)
//                {
//                    using (var memoryStream = new MemoryStream())
//                    {
//                        await usuario.FotoFile.CopyToAsync(memoryStream);
//                        usuario.FotoPerfil = memoryStream.ToArray(); // Convierte la imagen a byte[]
//                    }
//                }

//                // Eliminar caracteres no numéricos del número de teléfono
//                if (!string.IsNullOrEmpty(usuario.Telefono))
//                {
//                    usuario.Telefono = Regex.Replace(usuario.Telefono, @"[^\d]", "");
//                }

//                // Abrir conexión y validar duplicados
//                using (var connection = _dapperContext.AbrirConexion("SGP"))
//                {
//                    // Validar si el Usuario ya existe en la base de datos
//                    string sqlValidarUsuario = "SELECT COUNT(*) FROM Usuarios WHERE Usuario = @Usuario;";
//                    var parametrosValidarUsuario = new DynamicParameters();
//                    parametrosValidarUsuario.Add("@Usuario", usuario.Usuario, DbType.String);

//                    int usuarioExistente = await connection.ExecuteScalarAsync<int>(sqlValidarUsuario, parametrosValidarUsuario);

//                    if (usuarioExistente > 0)
//                    {
//                        respuesta.Mensaje = "El nombre de usuario ya existe en la base de datos.";
//                        return respuesta;
//                    }

//                    // Validar si el correo ya existe en la base de datos
//                    string sqlValidarCorreo = "SELECT COUNT(*) FROM Usuarios WHERE Correo = @Correo;";
//                    var parametrosValidarCorreo = new DynamicParameters();
//                    parametrosValidarCorreo.Add("@Correo", usuario.Correo, DbType.String);

//                    int correoExistente = await connection.ExecuteScalarAsync<int>(sqlValidarCorreo, parametrosValidarCorreo);

//                    if (correoExistente > 0)
//                    {
//                        respuesta.Mensaje = "El correo ya existe en la base de datos.";
//                        return respuesta;
//                    }

//                    // Validar si el número de teléfono ya existe en la base de datos
//                    string sqlValidarTelefono = "SELECT COUNT(*) FROM Usuarios WHERE Telefono = @Telefono;";
//                    var parametrosValidarTelefono = new DynamicParameters();
//                    parametrosValidarTelefono.Add("@Telefono", usuario.Telefono, DbType.String);

//                    int telefonoExistente = await connection.ExecuteScalarAsync<int>(sqlValidarTelefono, parametrosValidarTelefono);

//                    if (telefonoExistente > 0)
//                    {
//                        respuesta.Mensaje = "El número de teléfono ya existe en la base de datos.";
//                        return respuesta;
//                    }

//                    // Consulta SQL para insertar el usuario
//                    string sqlInsertar = @"
//                    INSERT INTO Usuarios 
//                    (Usuario, NombreUsuario, ApellidoPaternoUsuario, ApellidoMaternoUsuario, IdPuesto, Telefono, Correo, Contrasena, FotoPerfil, IdUsuarioSet) 
//                    VALUES 
//                    (@Usuario, @NombreUsuario, @ApellidoPaternoUsuario, @ApellidoMaternoUsuario, @IdPuesto, @Telefono, @Correo, 
//                    HASHBYTES('SHA2_256', CONVERT(NVARCHAR(50), @Contrasena)), @FotoPerfil, @IdUsuarioSet);";

//                    var parametrosInsertar = new DynamicParameters();
//                    parametrosInsertar.Add("@Usuario", usuario.Usuario, DbType.String);
//                    parametrosInsertar.Add("@NombreUsuario", usuario.NombreUsuario, DbType.String);
//                    parametrosInsertar.Add("@ApellidoPaternoUsuario", usuario.ApellidoPaternoUsuario, DbType.String);
//                    parametrosInsertar.Add("@ApellidoMaternoUsuario", usuario.ApellidoMaternoUsuario, DbType.String);
//                    parametrosInsertar.Add("@IdPuesto", usuario.IdPuesto, DbType.String);
//                    parametrosInsertar.Add("@Telefono", usuario.Telefono, DbType.String);
//                    parametrosInsertar.Add("@Correo", usuario.Correo, DbType.String);
//                    parametrosInsertar.Add("@Contrasena", usuario.Contrasena, DbType.String);
//                    parametrosInsertar.Add("@FotoPerfil", usuario.FotoPerfil, DbType.Binary);
//                    parametrosInsertar.Add("@IdUsuarioSet", usuario.IdUsuarioSet, DbType.Int32);

//                    // Ejecutar la consulta de inserción
//                    int filasAfectadas = await connection.ExecuteAsync(sqlInsertar, parametrosInsertar);

//                    if (filasAfectadas > 0)
//                    {
//                        respuesta.Resultado = true;
//                        respuesta.Mensaje = "Usuario agregado exitosamente.";
//                        respuesta.Data = new { UsuarioId = usuario.Id }; // Puedes devolver el ID del usuario si lo necesitas
//                    }
//                    else
//                    {
//                        respuesta.Mensaje = "No se pudo agregar el usuario.";
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al agregar el usuario: " + ex.Message;
//                respuesta.Errores.Add(ex.Message);
//            }
//            finally
//            {
//                _dapperContext.Dispose(); // Liberar recursos
//            }

//            return respuesta;
//        }

//        public async Task<RespuestaJson> EditarUsuario(Usuarios usuario)
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {
//                // Validar que el archivo de imagen esté presente y sea válido
//                if (usuario.FotoFile != null && usuario.FotoFile.Length > 0)
//                {
//                    using (var memoryStream = new MemoryStream())
//                    {
//                        await usuario.FotoFile.CopyToAsync(memoryStream);
//                        usuario.FotoPerfil = memoryStream.ToArray(); // Convierte la imagen a byte[]
//                    }
//                }
//                else if (!string.IsNullOrEmpty(usuario.FotoBase64))
//                {
//                    // Si no hay una nueva imagen, pero hay una imagen existente en base64, convertirla a byte[]
//                    usuario.FotoPerfil = Convert.FromBase64String(usuario.FotoBase64.Split(',')[1]);
//                }

//                // Eliminar caracteres no numéricos del número de teléfono
//                if (!string.IsNullOrEmpty(usuario.Telefono))
//                {
//                    usuario.Telefono = Regex.Replace(usuario.Telefono, @"[^\d]", "");
//                }

//                // Obtener la fecha y hora actual
//                usuario.FechaUpd = DateTime.Now;

//                // Abrir conexión y validar duplicados
//                using (var connection = _dapperContext.AbrirConexion("SGP"))
//                {
//                    // Consulta SQL base para actualizar el usuario
//                    string sqlActualizar = @"
//                    UPDATE Usuarios 
//                    SET Usuario = @Usuario, 
//                        NombreUsuario = @NombreUsuario, 
//                        ApellidoPaternoUsuario = @ApellidoPaternoUsuario, 
//                        ApellidoMaternoUsuario = @ApellidoMaternoUsuario,
//                        Telefono = @Telefono,
//                        Correo = @Correo,
//                        FotoPerfil = @FotoPerfil,
//                        IdUsuarioUpd = @IdUsuarioUpd,
//                        FechaUpd = @FechaUpd";

//                    // Agregar IdPuesto a la consulta si tiene un valor válido (> 0)
//                    if (usuario.IdPuesto > 0)
//                    {
//                        sqlActualizar += ", IdPuesto = @IdPuesto";
//                    }

//                    // Agregar Contrasena a la consulta si no es nula o vacía
//                    if (!string.IsNullOrEmpty(usuario.Contrasena))
//                    {
//                        sqlActualizar += ", Contrasena = CONVERT(NVARCHAR(50), @Contrasena))";
//                    }

//                    sqlActualizar += " WHERE Id = @Id;";

//                    var parametrosActualizar = new DynamicParameters();
//                    parametrosActualizar.Add("@Id", usuario.Id, DbType.Int32);
//                    parametrosActualizar.Add("@Usuario", usuario.Usuario, DbType.String);
//                    parametrosActualizar.Add("@NombreUsuario", usuario.NombreUsuario, DbType.String);
//                    parametrosActualizar.Add("@ApellidoPaternoUsuario", usuario.ApellidoPaternoUsuario, DbType.String);
//                    parametrosActualizar.Add("@ApellidoMaternoUsuario", usuario.ApellidoMaternoUsuario, DbType.String);
//                    parametrosActualizar.Add("@Telefono", usuario.Telefono, DbType.String);
//                    parametrosActualizar.Add("@Correo", usuario.Correo, DbType.String);
//                    parametrosActualizar.Add("@FotoPerfil", usuario.FotoPerfil, DbType.Binary);
//                    parametrosActualizar.Add("@IdUsuarioUpd", usuario.IdUsuarioUpd, DbType.Int32);
//                    parametrosActualizar.Add("@FechaUpd", usuario.FechaUpd, DbType.DateTime);

//                    // Agregar IdPuesto a los parámetros si tiene un valor válido (> 0)
//                    if (usuario.IdPuesto > 0)
//                    {
//                        parametrosActualizar.Add("@IdPuesto", usuario.IdPuesto, DbType.Int32);
//                    }

//                    // Agregar Contrasena a los parámetros si no es nula o vacía
//                    if (!string.IsNullOrEmpty(usuario.Contrasena))
//                    {
//                        parametrosActualizar.Add("@Contrasena", usuario.Contrasena, DbType.String);
//                    }

//                    // Ejecutar la consulta de actualización
//                    int filasAfectadas = await connection.ExecuteAsync(sqlActualizar, parametrosActualizar);

//                    if (filasAfectadas > 0)
//                    {
//                        respuesta.Resultado = true;
//                        respuesta.Mensaje = "Usuario actualizado exitosamente.";
//                    }
//                    else
//                    {
//                        respuesta.Mensaje = "No se pudo actualizar el usuario.";
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al actualizar el usuario: " + ex.Message;
//                respuesta.Errores.Add(ex.Message);
//            }
//            finally
//            {
//                _dapperContext.Dispose(); // Liberar recursos
//            }

//            return respuesta;
//        }

//        public async Task<RespuestaJson> DesactivarUsuario(Usuarios usuario)
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {
//                // Obtener la fecha y hora actual
//                usuario.FechaDel = DateTime.Now;

//                using (var connection = _dapperContext.AbrirConexion("SGP"))
//                {
//                    // Consulta SQL para desactivar el usuario
//                    string sql = @"
//                    UPDATE Usuarios 
//                    SET FlgActivo = 0, 
//                        IdUsuarioDel = @IdUsuarioDel, 
//                        FechaDel = @FechaDel 
//                    WHERE Id = @Id";

//                    var parametros = new DynamicParameters();
//                    parametros.Add("@Id", usuario.Id, DbType.Int32);
//                    parametros.Add("@IdUsuarioDel", usuario.IdUsuarioDel, DbType.Int32);
//                    parametros.Add("@FechaDel", usuario.FechaDel, DbType.DateTime);

//                    // Ejecutar la consulta
//                    int filasAfectadas = await connection.ExecuteAsync(sql, parametros);

//                    if (filasAfectadas > 0)
//                    {
//                        respuesta.Resultado = true;
//                        respuesta.Mensaje = "Usuario desactivado correctamente.";
//                    }
//                    else
//                    {
//                        respuesta.Mensaje = "No se encontró el usuario o ya está desactivado.";
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al desactivar el usuario: " + ex.Message;
//            }
//            finally
//            {
//                _dapperContext.Dispose();
//            }
//            return respuesta;
//        }

//        public async Task<RespuestaJson> ReactivarUsuario(Usuarios usuario)
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {
//                // Obtener la fecha y hora actual
//                usuario.FechaUpd = DateTime.Now;

//                using (var connection = _dapperContext.AbrirConexion("SGP"))
//                {
//                    // Consulta SQL para desactivar el usuario
//                    string sql = @"
//                    UPDATE Usuarios 
//                    SET FlgActivo = 1, 
//                        IdUsuarioUpd = @IdUsuarioUpd, 
//                        FechaUpd = @FechaUpd
//                    WHERE Id = @Id";

//                    var parametros = new DynamicParameters();
//                    parametros.Add("@Id", usuario.Id, DbType.Int32);
//                    parametros.Add("@IdUsuarioUpd", usuario.IdUsuarioUpd, DbType.Int32);
//                    parametros.Add("@FechaUpd", usuario.FechaUpd, DbType.DateTime);

//                    // Ejecutar la consulta
//                    int filasAfectadas = await connection.ExecuteAsync(sql, parametros);

//                    if (filasAfectadas > 0)
//                    {
//                        respuesta.Resultado = true;
//                        respuesta.Mensaje = "Usuario reactivado correctamente.";
//                    }
//                    else
//                    {
//                        respuesta.Mensaje = "No se encontró el usuario o ya está reactivado.";
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al reactivar el usuario: " + ex.Message;
//            }
//            finally
//            {
//                _dapperContext.Dispose();
//            }
//            return respuesta;
//        }

//        //============================================================================================================================\\
//        //====================================================   Modelo ClientesUsuarios   =========================================== \\
//        //==============================================================================================================================\\

//        public async Task<RespuestaJson> GetClientesUsuarios()
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {
//                // Abrir la conexión a la base de datos
//                _dapperContext.AbrirConexion("SGP");
//                string sql = @"SELECT 
//                s.Id, 
//                s.ClienteUsuario, 
//                s.NombreClienteUsuario, 
//                s.ApellidoPaternoClienteUsuario, 
//                s.ApellidoMaternoClienteUsuario, 
//                c.Descripcion, 
//                s.Telefono, 
//                s.Correo, 
//                cli.RazonSocial,
//                s.FotoPerfil, 
//                s.FlgActivo
//            FROM 
//                ClientesUsuarios s 
//            INNER JOIN 
//                Puestos c ON s.IdPuesto = c.Id
//            INNER JOIN 
//                Clientes cli ON s.IdCliente = cli.Id;";
//                var clientesusuarios = await _dapperContext.QueryAsync<ClientesUsuarios>(sql);
//                if (clientesusuarios != null)
//                {
//                    respuesta.Resultado = true;
//                    respuesta.Data = clientesusuarios.Select(s => new ClientesUsuarios
//                    {
//                        Id = s.Id,
//                        ClienteUsuario = s.ClienteUsuario,
//                        NombreClienteUsuario = s.NombreClienteUsuario,
//                        ApellidoPaternoClienteUsuario = s.ApellidoPaternoClienteUsuario,
//                        ApellidoMaternoClienteUsuario = s.ApellidoMaternoClienteUsuario,
//                        Descripcion = s.Descripcion,
//                        Telefono = s.Telefono,
//                        Correo = s.Correo,
//                        FotoPerfil = s.FotoPerfil,
//                        FlgActivo = s.FlgActivo,
//                        RazonSocial = s.RazonSocial  // Asegúrate de que la clase ClientesUsuarios tenga esta propiedad
//                    }).ToList();
//                }
//                else
//                {
//                    respuesta.Mensaje = "No se encontraron clientes usuarios activos.";
//                    respuesta.Data = new List<ClientesUsuarios>(); // Inicializar Data para evitar null
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al obtener los datos de los clientes usuarios." + ex.Message;
//                respuesta.Data = new List<ClientesUsuarios>(); // Inicializar Data para evitar null
//            }
//            finally
//            {
//                // Cerrar o liberar la conexión (si es necesario)
//                _dapperContext.Dispose();
//            }
//            return respuesta;
//        }

//        public async Task<RespuestaJson> GetClientesUsuariosPuestos()
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {
//                // Abrir la conexión a la base de datos
//                _dapperContext.AbrirConexion("SGP");

//                string sql = @"
//                SELECT 
//                    Id, 
//                    Descripcion 
//                FROM 
//                    Puestos WHERE Tipo = 2";

//                var puestos = await _dapperContext.QueryAsync<Puestos>(sql);
//                if (puestos != null)
//                {
//                    respuesta.Resultado = true;
//                    respuesta.Data = puestos.Select(c => new
//                    {
//                        Id = c.Id,
//                        Descripcion = c.Descripcion
//                    }).ToList();
//                }
//                else
//                {
//                    respuesta.Mensaje = "No se encontraron puestos.";
//                    respuesta.Data = new List<Puestos>(); // Inicializar Data para evitar null
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al obtener los datos de los puestos." + ex.Message;
//                respuesta.Data = new List<Puestos>(); // Inicializar Data para evitar null
//            }
//            finally
//            {
//                // Cerrar o liberar la conexión (si es necesario)
//                _dapperContext.Dispose();
//            }
//            return respuesta;
//        }


//        public async Task<RespuestaJson> AgregarClienteUsuario(ClientesUsuarios clienteusuario)
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {
//                // Validar que el archivo de imagen esté presente y sea válido
//                if (clienteusuario.FotoFile != null && clienteusuario.FotoFile.Length > 0)
//                {
//                    using (var memoryStream = new MemoryStream())
//                    {
//                        await clienteusuario.FotoFile.CopyToAsync(memoryStream);
//                        clienteusuario.FotoPerfil = memoryStream.ToArray(); // Convierte la imagen a byte[]
//                    }
//                }

//                // Eliminar caracteres no numéricos del número de teléfono
//                if (!string.IsNullOrEmpty(clienteusuario.Telefono))
//                {
//                    clienteusuario.Telefono = Regex.Replace(clienteusuario.Telefono, @"[^\d]", "");
//                }

//                // Abrir conexión y validar duplicados
//                using (var connection = _dapperContext.AbrirConexion("SGP"))
//                {
//                    // Validar si el ClienteUsuario ya existe en la base de datos
//                    string sqlValidarClienteUsuario = "SELECT COUNT(*) FROM ClientesUsuarios WHERE ClienteUsuario = @ClienteUsuario;";
//                    var parametrosValidarClienteUsuario = new DynamicParameters();
//                    parametrosValidarClienteUsuario.Add("@ClienteUsuario", clienteusuario.ClienteUsuario, DbType.String);

//                    int clienteusuarioExistente = await connection.ExecuteScalarAsync<int>(sqlValidarClienteUsuario, parametrosValidarClienteUsuario);

//                    if (clienteusuarioExistente > 0)
//                    {
//                        respuesta.Mensaje = "El nombre de cliente usuario ya existe en la base de datos.";
//                        return respuesta;
//                    }

//                    // Validar si el correo ya existe en la base de datos
//                    string sqlValidarCorreo = "SELECT COUNT(*) FROM ClientesUsuarios WHERE Correo = @Correo;";
//                    var parametrosValidarCorreo = new DynamicParameters();
//                    parametrosValidarCorreo.Add("@Correo", clienteusuario.Correo, DbType.String);

//                    int correoExistente = await connection.ExecuteScalarAsync<int>(sqlValidarCorreo, parametrosValidarCorreo);

//                    if (correoExistente > 0)
//                    {
//                        respuesta.Mensaje = "El correo ya existe en la base de datos.";
//                        return respuesta;
//                    }

//                    // Validar si el número de teléfono ya existe en la base de datos
//                    string sqlValidarTelefono = "SELECT COUNT(*) FROM ClientesUsuarios WHERE Telefono = @Telefono;";
//                    var parametrosValidarTelefono = new DynamicParameters();
//                    parametrosValidarTelefono.Add("@Telefono", clienteusuario.Telefono, DbType.String);

//                    int telefonoExistente = await connection.ExecuteScalarAsync<int>(sqlValidarTelefono, parametrosValidarTelefono);

//                    if (telefonoExistente > 0)
//                    {
//                        respuesta.Mensaje = "El número de teléfono ya existe en la base de datos.";
//                        return respuesta;
//                    }

//                    // Consulta SQL para insertar el cliente usuario
//                    string sqlInsertar = @"
//                    INSERT INTO ClientesUsuarios 
//                    (ClienteUsuario, NombreClienteUsuario, ApellidoPaternoClienteUsuario, ApellidoMaternoClienteUsuario, IdPuesto, Telefono, Correo, Contrasena, IdCliente, FotoPerfil, IdUsuarioSet) 
//                    VALUES 
//                    (@ClienteUsuario, @NombreClienteUsuario, @ApellidoPaternoClienteUsuario, @ApellidoMaternoClienteUsuario, @IdPuesto, @Telefono, @Correo, 
//                    HASHBYTES('SHA2_256', CONVERT(NVARCHAR(50), @Contrasena)), @IdCliente, @FotoPerfil, @IdUsuarioSet);";

//                    var parametrosInsertar = new DynamicParameters();
//                    parametrosInsertar.Add("@ClienteUsuario", clienteusuario.ClienteUsuario, DbType.String);
//                    parametrosInsertar.Add("@NombreClienteUsuario", clienteusuario.NombreClienteUsuario, DbType.String);
//                    parametrosInsertar.Add("@ApellidoPaternoClienteUsuario", clienteusuario.ApellidoPaternoClienteUsuario, DbType.String);
//                    parametrosInsertar.Add("@ApellidoMaternoClienteUsuario", clienteusuario.ApellidoMaternoClienteUsuario, DbType.String);
//                    parametrosInsertar.Add("@IdPuesto", clienteusuario.IdPuesto, DbType.String);
//                    parametrosInsertar.Add("@Telefono", clienteusuario.Telefono, DbType.String);
//                    parametrosInsertar.Add("@Correo", clienteusuario.Correo, DbType.String);
//                    parametrosInsertar.Add("@Contrasena", clienteusuario.Contrasena, DbType.String);
//                    parametrosInsertar.Add("@IdCliente", clienteusuario.IdCliente, DbType.String);
//                    parametrosInsertar.Add("@FotoPerfil", clienteusuario.FotoPerfil, DbType.Binary);
//                    parametrosInsertar.Add("@IdUsuarioSet", clienteusuario.IdUsuarioSet, DbType.Int32);

//                    // Ejecutar la consulta de inserción
//                    int filasAfectadas = await connection.ExecuteAsync(sqlInsertar, parametrosInsertar);

//                    if (filasAfectadas > 0)
//                    {
//                        respuesta.Resultado = true;
//                        respuesta.Mensaje = "Cliente usuario agregado exitosamente.";
//                        respuesta.Data = new { ClienteUsuarioId = clienteusuario.Id }; // Puedes devolver el ID del cliente usuario si lo necesitas
//                    }
//                    else
//                    {
//                        respuesta.Mensaje = "No se pudo agregar el cliente usuario.";
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al agregar el cliente usuario: " + ex.Message;
//                respuesta.Errores.Add(ex.Message);
//            }
//            finally
//            {
//                _dapperContext.Dispose(); // Liberar recursos
//            }

//            return respuesta;
//        }

//        public async Task<RespuestaJson> EditarClienteUsuario(ClientesUsuarios clienteusuario)
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {
//                // Validar que el archivo de imagen esté presente y sea válido
//                if (clienteusuario.FotoFile != null && clienteusuario.FotoFile.Length > 0)
//                {
//                    using (var memoryStream = new MemoryStream())
//                    {
//                        await clienteusuario.FotoFile.CopyToAsync(memoryStream);
//                        clienteusuario.FotoPerfil = memoryStream.ToArray(); // Convierte la imagen a byte[]
//                    }
//                }
//                else if (!string.IsNullOrEmpty(clienteusuario.FotoBase64))
//                {
//                    // Si no hay una nueva imagen, pero hay una imagen existente en base64, convertirla a byte[]
//                    clienteusuario.FotoPerfil = Convert.FromBase64String(clienteusuario.FotoBase64.Split(',')[1]);
//                }

//                // Eliminar caracteres no numéricos del número de teléfono
//                if (!string.IsNullOrEmpty(clienteusuario.Telefono))
//                {
//                    clienteusuario.Telefono = Regex.Replace(clienteusuario.Telefono, @"[^\d]", "");
//                }

//                // Obtener la fecha y hora actual
//                clienteusuario.FechaUpd = DateTime.Now;

//                // Abrir conexión y validar duplicados
//                using (var connection = _dapperContext.AbrirConexion("SGP"))
//                {
//                    // Consulta SQL base para actualizar el cliente usuario
//                    string sqlActualizar = @"
//                    UPDATE ClientesUsuarios 
//                    SET ClienteUsuario = @ClienteUsuario, 
//                        NombreClienteUsuario = @NombreClienteUsuario, 
//                        ApellidoPaternoClienteUsuario = @ApellidoPaternoClienteUsuario, 
//                        ApellidoMaternoClienteUsuario = @ApellidoMaternoClienteUsuario,
//                        Telefono = @Telefono,
//                        Correo = @Correo,
//                        FotoPerfil = @FotoPerfil,
//                        IdUsuarioUpd = @IdUsuarioUpd,
//                        FechaUpd = @FechaUpd";

//                    // Agregar IdPuesto a la consulta si tiene un valor válido (> 0)
//                    if (clienteusuario.IdPuesto > 0)
//                    {
//                        sqlActualizar += ", IdPuesto = @IdPuesto";
//                    }
//                    if (clienteusuario.IdCliente > 0)
//                    {
//                        sqlActualizar += ", IdCliente = @IdCliente";
//                    }

//                    // Agregar Contrasena a la consulta si no es nula o vacía
//                    if (!string.IsNullOrEmpty(clienteusuario.Contrasena))
//                    {
//                        sqlActualizar += ", Contrasena = @Contrasena";
//                    }

//                    sqlActualizar += " WHERE Id = @Id;";

//                    var parametrosActualizar = new DynamicParameters();
//                    parametrosActualizar.Add("@Id", clienteusuario.Id, DbType.Int32);
//                    parametrosActualizar.Add("@ClienteUsuario", clienteusuario.ClienteUsuario, DbType.String);
//                    parametrosActualizar.Add("@NombreClienteUsuario", clienteusuario.NombreClienteUsuario, DbType.String);
//                    parametrosActualizar.Add("@ApellidoPaternoClienteUsuario", clienteusuario.ApellidoPaternoClienteUsuario, DbType.String);
//                    parametrosActualizar.Add("@ApellidoMaternoClienteUsuario", clienteusuario.ApellidoMaternoClienteUsuario, DbType.String);
//                    parametrosActualizar.Add("@Telefono", clienteusuario.Telefono, DbType.String);
//                    parametrosActualizar.Add("@Correo", clienteusuario.Correo, DbType.String);
//                    parametrosActualizar.Add("@FotoPerfil", clienteusuario.FotoPerfil, DbType.Binary);
//                    parametrosActualizar.Add("@IdUsuarioUpd", clienteusuario.IdUsuarioUpd, DbType.Int32);
//                    parametrosActualizar.Add("@FechaUpd", clienteusuario.FechaUpd, DbType.DateTime);

//                    // Agregar IdPuesto a los parámetros si tiene un valor válido (> 0)
//                    if (clienteusuario.IdPuesto > 0)
//                    {
//                        parametrosActualizar.Add("@IdPuesto", clienteusuario.IdPuesto, DbType.Int32);
//                    }
//                    if (clienteusuario.IdCliente > 0)
//                    {
//                        parametrosActualizar.Add("@IdCliente", clienteusuario.IdCliente, DbType.Int32);
//                    }

//                    // Agregar Contrasena a los parámetros si no es nula o vacía
//                    if (!string.IsNullOrEmpty(clienteusuario.Contrasena))
//                    {
//                        parametrosActualizar.Add("@Contrasena", clienteusuario.Contrasena, DbType.String);
//                    }

//                    // Ejecutar la consulta de actualización
//                    int filasAfectadas = await connection.ExecuteAsync(sqlActualizar, parametrosActualizar);

//                    if (filasAfectadas > 0)
//                    {
//                        respuesta.Resultado = true;
//                        respuesta.Mensaje = "Cliente usuario actualizado exitosamente.";
//                    }
//                    else
//                    {
//                        respuesta.Mensaje = "No se pudo actualizar el cliente usuario.";
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al actualizar el cliente usuario: " + ex.Message;
//                respuesta.Errores.Add(ex.Message);
//            }
//            finally
//            {
//                _dapperContext.Dispose(); // Liberar recursos
//            }

//            return respuesta;
//        }

//        public async Task<RespuestaJson> DesactivarClienteUsuario(ClientesUsuarios clienteusuario)
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {
//                // Obtener la fecha y hora actual
//                clienteusuario.FechaDel = DateTime.Now;

//                using (var connection = _dapperContext.AbrirConexion("SGP"))
//                {
//                    // Consulta SQL para desactivar el cliente usuario
//                    string sql = @"
//            UPDATE ClientesUsuarios 
//            SET FlgActivo = 0, 
//                IdUsuarioDel = @IdUsuarioDel, 
//                FechaDel = @FechaDel 
//            WHERE Id = @Id";

//                    var parametros = new DynamicParameters();
//                    parametros.Add("@Id", clienteusuario.Id, DbType.Int32);
//                    parametros.Add("@IdUsuarioDel", clienteusuario.IdUsuarioDel, DbType.Int32);
//                    parametros.Add("@FechaDel", clienteusuario.FechaDel, DbType.DateTime);

//                    // Ejecutar la consulta
//                    int filasAfectadas = await connection.ExecuteAsync(sql, parametros);

//                    if (filasAfectadas > 0)
//                    {
//                        respuesta.Resultado = true;
//                        respuesta.Mensaje = "Cliente usuario desactivado correctamente.";
//                    }
//                    else
//                    {
//                        respuesta.Mensaje = "No se encontró el cliente usuario o ya está desactivado.";
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al desactivar el cliente usuario: " + ex.Message;
//            }
//            finally
//            {
//                _dapperContext.Dispose();
//            }
//            return respuesta;
//        }

//        public async Task<RespuestaJson> ReactivarClienteUsuario(ClientesUsuarios clienteusuario)
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {
//                // Obtener la fecha y hora actual
//                clienteusuario.FechaUpd = DateTime.Now;

//                using (var connection = _dapperContext.AbrirConexion("SGP"))
//                {
//                    // Consulta SQL para reactivar el cliente usuario
//                    string sql = @"
//            UPDATE ClientesUsuarios 
//            SET FlgActivo = 1, 
//                IdUsuarioUpd = @IdUsuarioUpd, 
//                FechaUpd = @FechaUpd
//            WHERE Id = @Id";

//                    var parametros = new DynamicParameters();
//                    parametros.Add("@Id", clienteusuario.Id, DbType.Int32);
//                    parametros.Add("@IdUsuarioUpd", clienteusuario.IdUsuarioUpd, DbType.Int32);
//                    parametros.Add("@FechaUpd", clienteusuario.FechaUpd, DbType.DateTime);

//                    // Ejecutar la consulta
//                    int filasAfectadas = await connection.ExecuteAsync(sql, parametros);

//                    if (filasAfectadas > 0)
//                    {
//                        respuesta.Resultado = true;
//                        respuesta.Mensaje = "Cliente usuario reactivado correctamente.";
//                    }
//                    else
//                    {
//                        respuesta.Mensaje = "No se encontró el cliente usuario o ya está reactivado.";
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al reactivar el cliente usuario: " + ex.Message;
//            }
//            finally
//            {
//                _dapperContext.Dispose();
//            }
//            return respuesta;
//        }

//        //============================================================================================================================\\
//        //=================================================   Modelo ModuloSistemas   ================================================ \\
//        //==============================================================================================================================\\

//        public async Task<RespuestaJson> GetModuloSistemas()
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {
//                // Abrir la conexión a la base de datos
//                _dapperContext.AbrirConexion("SGP");

//                string sql = @"SELECT 
//                    s.Id, 
//                    s.ModuloDescripcion, 
//                    c.Descripcion, 
//                    C.Prefijo,
//                    s.FlgActivo,
//                    s.IdSistema
//                FROM 
//                    ModuloSistemas s 
//                INNER JOIN 
//                    Sistemas c ON s.IdSistema = c.Id
//                ORDER BY s.IdSistema"; // Ordenar por IdSistema

//                var modulosistemas = await _dapperContext.QueryAsync<ModuloSistema>(sql);
//                if (modulosistemas != null)
//                {
//                    respuesta.Resultado = true;
//                    respuesta.Data = modulosistemas.Select(s => new ModuloSistema
//                    {
//                        Id = s.Id,
//                        ModuloDescripcion = s.ModuloDescripcion,
//                        Descripcion = s.Descripcion,
//                        Prefijo = s.Prefijo,
//                        FlgActivo = s.FlgActivo
//                    }).ToList();
//                }
//                else
//                {
//                    respuesta.Mensaje = "No se encontraron modulosistemas activos.";
//                    respuesta.Data = new List<ModuloSistema>(); // Inicializar Data para evitar null
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al obtener los datos de los modulosistemas." + ex.Message;
//                respuesta.Data = new List<ModuloSistema>(); // Inicializar Data para evitar null
//            }
//            finally
//            {
//                // Cerrar o liberar la conexión (si es necesario)
//                _dapperContext.Dispose();
//            }
//            return respuesta;
//        }

//        public async Task<RespuestaJson> AgregarModuloSistema(ModuloSistema modulosistema)
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {

//                // Abrir conexión y validar duplicados
//                using (var connection = _dapperContext.AbrirConexion("SGP"))
//                {

//                    // Validar si la combinación de ModuloDescripcion y IdSistema ya existe en la base de datos
//                    string sqlValidarModuloDescripcion = "SELECT COUNT(*) FROM ModuloSistemas WHERE ModuloDescripcion = @ModuloDescripcion AND IdSistema = @IdSistema;";
//                    var parametrosValidarModuloDescripcion = new DynamicParameters();
//                    parametrosValidarModuloDescripcion.Add("@ModuloDescripcion", modulosistema.ModuloDescripcion, DbType.String);
//                    parametrosValidarModuloDescripcion.Add("@IdSistema", modulosistema.IdSistema, DbType.Int32);

//                    int ModuloDescripcionExistente = await connection.ExecuteScalarAsync<int>(sqlValidarModuloDescripcion, parametrosValidarModuloDescripcion);

//                    if (ModuloDescripcionExistente > 0)
//                    {
//                        respuesta.Mensaje = "Ya existe un registro con la misma descripción para este sistema.";
//                        return respuesta;
//                    }

//                    // Consulta SQL para insertar el modulosistema
//                    string sqlInsertar = @"
//                    INSERT INTO ModuloSistemas 
//                    (ModuloDescripcion, IdSistema, IdUsuarioSet) 
//                    VALUES 
//                    (@ModuloDescripcion, @IdSistema, @IdUsuarioSet);";

//                    var parametrosInsertar = new DynamicParameters();
//                    parametrosInsertar.Add("@ModuloDescripcion", modulosistema.ModuloDescripcion, DbType.String);
//                    parametrosInsertar.Add("@IdSistema", modulosistema.IdSistema, DbType.String);
//                    parametrosInsertar.Add("@IdUsuarioSet", modulosistema.IdUsuarioSet, DbType.Int32);

//                    // Ejecutar la consulta de inserción
//                    int filasAfectadas = await connection.ExecuteAsync(sqlInsertar, parametrosInsertar);

//                    if (filasAfectadas > 0)
//                    {
//                        respuesta.Resultado = true;
//                        respuesta.Mensaje = "ModuloSistema agregado exitosamente.";
//                        respuesta.Data = new { ModuloSistemaId = modulosistema.Id }; // Puedes devolver el ID del modulosistema si lo necesitas
//                    }
//                    else
//                    {
//                        respuesta.Mensaje = "No se pudo agregar el modulosistema.";
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al agregar el modulosistema: " + ex.Message;
//                respuesta.Errores.Add(ex.Message);
//            }
//            finally
//            {
//                _dapperContext.Dispose(); // Liberar recursos
//            }

//            return respuesta;
//        }

//        public async Task<RespuestaJson> EditarModuloSistema(ModuloSistema modulosistema)
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {
//                // Obtener la fecha y hora actual
//                modulosistema.FechaUpd = DateTime.Now;

//                // Abrir conexión y validar duplicados
//                using (var connection = _dapperContext.AbrirConexion("SGP"))
//                {
//                    // Consulta SQL base para actualizar el usuario
//                    string sqlActualizar = @"
//                    UPDATE ModuloSistemas
//                    SET ModuloDescripcion = @ModuloDescripcion, 
//                        IdUsuarioUpd = @IdUsuarioUpd,
//                        FechaUpd = @FechaUpd";

//                    // Agregar IdPuesto a la consulta si tiene un valor válido (> 0)
//                    if (modulosistema.IdSistema > 0)
//                    {
//                        sqlActualizar += ", IdSistema = @IdSistema";
//                    }

//                    sqlActualizar += " WHERE Id = @Id;";

//                    var parametrosActualizar = new DynamicParameters();
//                    parametrosActualizar.Add("@Id", modulosistema.Id, DbType.Int32);
//                    parametrosActualizar.Add("@ModuloDescripcion", modulosistema.ModuloDescripcion, DbType.String);
//                    parametrosActualizar.Add("@IdUsuarioUpd", modulosistema.IdUsuarioUpd, DbType.Int32);
//                    parametrosActualizar.Add("@FechaUpd", modulosistema.FechaUpd, DbType.DateTime);

//                    // Agregar IdPuesto a los parámetros si tiene un valor válido (> 0)
//                    if (modulosistema.IdSistema > 0)
//                    {
//                        parametrosActualizar.Add("@IdSistema", modulosistema.IdSistema, DbType.Int32);
//                    }

//                    // Ejecutar la consulta de actualización
//                    int filasAfectadas = await connection.ExecuteAsync(sqlActualizar, parametrosActualizar);

//                    if (filasAfectadas > 0)
//                    {
//                        respuesta.Resultado = true;
//                        respuesta.Mensaje = "ModeloSistemas actualizado exitosamente.";
//                    }
//                    else
//                    {
//                        respuesta.Mensaje = "No se pudo actualizar el ModeloSistemas.";
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al actualizar el ModeloSistemas: " + ex.Message;
//                respuesta.Errores.Add(ex.Message);
//            }
//            finally
//            {
//                _dapperContext.Dispose(); // Liberar recursos
//            }

//            return respuesta;
//        }

//        public async Task<RespuestaJson> DesactivarModuloSistema(ModuloSistema modulosistema)
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {
//                // Obtener la fecha y hora actual
//                modulosistema.FechaDel = DateTime.Now;

//                using (var connection = _dapperContext.AbrirConexion("SGP"))
//                {
//                    // Consulta SQL para desactivar el modulosistema
//                    string sql = @"
//                    UPDATE ModuloSistemas 
//                    SET FlgActivo = 0, 
//                        IdUsuarioDel = @IdUsuarioDel, 
//                        FechaDel = @FechaDel 
//                    WHERE Id = @Id";

//                    var parametros = new DynamicParameters();
//                    parametros.Add("@Id", modulosistema.Id, DbType.Int32);
//                    parametros.Add("@IdUsuarioDel", modulosistema.IdUsuarioDel, DbType.Int32);
//                    parametros.Add("@FechaDel", modulosistema.FechaDel, DbType.DateTime);

//                    // Ejecutar la consulta
//                    int filasAfectadas = await connection.ExecuteAsync(sql, parametros);

//                    if (filasAfectadas > 0)
//                    {
//                        respuesta.Resultado = true;
//                        respuesta.Mensaje = "ModuloSistema desactivado correctamente.";
//                    }
//                    else
//                    {
//                        respuesta.Mensaje = "No se encontró el modulosistema o ya está desactivado.";
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al desactivar el modulosistema: " + ex.Message;
//            }
//            finally
//            {
//                _dapperContext.Dispose();
//            }
//            return respuesta;
//        }

//        public async Task<RespuestaJson> ReactivarModuloSistema(ModuloSistema modulosistema)
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {
//                // Obtener la fecha y hora actual
//                modulosistema.FechaUpd = DateTime.Now;

//                using (var connection = _dapperContext.AbrirConexion("SGP"))
//                {
//                    // Consulta SQL para reactivar el modulosistema
//                    string sql = @"
//                    UPDATE ModuloSistemas 
//                    SET FlgActivo = 1, 
//                        IdUsuarioUpd = @IdUsuarioUpd, 
//                        FechaUpd = @FechaUpd
//                    WHERE Id = @Id";

//                    var parametros = new DynamicParameters();
//                    parametros.Add("@Id", modulosistema.Id, DbType.Int32);
//                    parametros.Add("@IdUsuarioUpd", modulosistema.IdUsuarioUpd, DbType.Int32);
//                    parametros.Add("@FechaUpd", modulosistema.FechaUpd, DbType.DateTime);

//                    // Ejecutar la consulta
//                    int filasAfectadas = await connection.ExecuteAsync(sql, parametros);

//                    if (filasAfectadas > 0)
//                    {
//                        respuesta.Resultado = true;
//                        respuesta.Mensaje = "ModuloSistema reactivado correctamente.";
//                    }
//                    else
//                    {
//                        respuesta.Mensaje = "No se encontró el modulosistema o ya está reactivado.";
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al reactivar el modulosistema: " + ex.Message;
//            }
//            finally
//            {
//                _dapperContext.Dispose();
//            }
//            return respuesta;
//        }

//        //============================================================================================================================\\
//        //=================================================   Modelo EstatusProyectos   ================================================ \\
//        //==============================================================================================================================\\

//        public async Task<RespuestaJson> GetEstatusProyectos()
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {
//                // Abrir la conexión a la base de datos
//                _dapperContext.AbrirConexion("SGP");

//                string sql = @"SELECT 
//                    s.Id, 
//                    s.EstatusProyectosDescripcion, 
//                    c.Descripcion, 
//                    C.Prefijo,
//                    s.FlgActivo,
//                    s.IdSistema
//                FROM 
//                    EstatusProyectos s 
//                INNER JOIN 
//                    Sistemas c ON s.IdSistema = c.Id
//                ORDER BY s.IdSistema"; // Ordenar por IdSistema

//                var estatusproyectos = await _dapperContext.QueryAsync<EstatusProyectos>(sql);
//                if (estatusproyectos != null)
//                {
//                    respuesta.Resultado = true;
//                    respuesta.Data = estatusproyectos.Select(s => new EstatusProyectos
//                    {
//                        Id = s.Id,
//                        EstatusProyectosDescripcion = s.EstatusProyectosDescripcion,
//                        Descripcion = s.Descripcion,
//                        Prefijo = s.Prefijo,
//                        FlgActivo = s.FlgActivo
//                    }).ToList();
//                }
//                else
//                {
//                    respuesta.Mensaje = "No se encontraron estatusproyectos activos.";
//                    respuesta.Data = new List<EstatusProyectos>(); // Inicializar Data para evitar null
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al obtener los datos de los estatusproyectos." + ex.Message;
//                respuesta.Data = new List<EstatusProyectos>(); // Inicializar Data para evitar null
//            }
//            finally
//            {
//                // Cerrar o liberar la conexión (si es necesario)
//                _dapperContext.Dispose();
//            }
//            return respuesta;
//        }

//        public async Task<RespuestaJson> AgregarEstatusProyecto(EstatusProyectos estatusproyecto)
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {

//                // Abrir conexión y validar duplicados
//                using (var connection = _dapperContext.AbrirConexion("SGP"))
//                {

//                    // Validar si ya existe un registro con el mismo IdSistema en la base de datos
//                    string sqlValidarIdSistema = "SELECT COUNT(*) FROM EstatusProyectos WHERE IdSistema = @IdSistema;";
//                    var parametrosValidarIdSistema = new DynamicParameters();
//                    parametrosValidarIdSistema.Add("@IdSistema", estatusproyecto.IdSistema, DbType.Int32);

//                    int IdSistemaExistente = await connection.ExecuteScalarAsync<int>(sqlValidarIdSistema, parametrosValidarIdSistema);

//                    if (IdSistemaExistente > 0)
//                    {
//                        respuesta.Mensaje = "Ya existe un estatus para este sistema.";
//                        return respuesta;
//                    }

//                    // Consulta SQL para insertar el estatusproyecto
//                    string sqlInsertar = @"
//                    INSERT INTO EstatusProyectos 
//                    (EstatusProyectosDescripcion, IdSistema, IdUsuarioSet) 
//                    VALUES 
//                    (@EstatusProyectosDescripcion, @IdSistema, @IdUsuarioSet);";

//                    var parametrosInsertar = new DynamicParameters();
//                    parametrosInsertar.Add("@EstatusProyectosDescripcion", estatusproyecto.EstatusProyectosDescripcion, DbType.String);
//                    parametrosInsertar.Add("@IdSistema", estatusproyecto.IdSistema, DbType.String);
//                    parametrosInsertar.Add("@IdUsuarioSet", estatusproyecto.IdUsuarioSet, DbType.Int32);

//                    // Ejecutar la consulta de inserción
//                    int filasAfectadas = await connection.ExecuteAsync(sqlInsertar, parametrosInsertar);

//                    if (filasAfectadas > 0)
//                    {
//                        respuesta.Resultado = true;
//                        respuesta.Mensaje = "EstatusProyecto agregado exitosamente.";
//                        respuesta.Data = new { EstatusProyectoId = estatusproyecto.Id }; // Puedes devolver el ID del estatusproyecto si lo necesitas
//                    }
//                    else
//                    {
//                        respuesta.Mensaje = "No se pudo agregar el estatusproyecto.";
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al agregar el estatusproyecto: " + ex.Message;
//                respuesta.Errores.Add(ex.Message);
//            }
//            finally
//            {
//                _dapperContext.Dispose(); // Liberar recursos
//            }

//            return respuesta;
//        }

//        public async Task<RespuestaJson> EditarEstatusProyecto(EstatusProyectos estatusproyecto)
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {
//                // Obtener la fecha y hora actual
//                estatusproyecto.FechaUpd = DateTime.Now;

//                // Abrir conexión y validar duplicados
//                using (var connection = _dapperContext.AbrirConexion("SGP"))
//                {
//                    // Consulta SQL base para actualizar el usuario
//                    string sqlActualizar = @"
//                    UPDATE EstatusProyectos
//                    SET EstatusProyectosDescripcion = @EstatusProyectosDescripcion, 
//                        IdUsuarioUpd = @IdUsuarioUpd,
//                        FechaUpd = @FechaUpd";

//                    //// Agregar IdPuesto a la consulta si tiene un valor válido (> 0)
//                    //if (estatusproyecto.IdSistema > 0)
//                    //{
//                    //    sqlActualizar += ", IdSistema = @IdSistema";
//                    //}

//                    sqlActualizar += " WHERE Id = @Id;";

//                    var parametrosActualizar = new DynamicParameters();
//                    parametrosActualizar.Add("@Id", estatusproyecto.Id, DbType.Int32);
//                    parametrosActualizar.Add("@EstatusProyectosDescripcion", estatusproyecto.EstatusProyectosDescripcion, DbType.String);
//                    parametrosActualizar.Add("@IdUsuarioUpd", estatusproyecto.IdUsuarioUpd, DbType.Int32);
//                    parametrosActualizar.Add("@FechaUpd", estatusproyecto.FechaUpd, DbType.DateTime);

//                    //// Agregar IdPuesto a los parámetros si tiene un valor válido (> 0)
//                    //if (estatusproyecto.IdSistema > 0)
//                    //{
//                    //    parametrosActualizar.Add("@IdSistema", estatusproyecto.IdSistema, DbType.Int32);
//                    //}

//                    // Ejecutar la consulta de actualización
//                    int filasAfectadas = await connection.ExecuteAsync(sqlActualizar, parametrosActualizar);

//                    if (filasAfectadas > 0)
//                    {
//                        respuesta.Resultado = true;
//                        respuesta.Mensaje = "EstatusProyectos actualizado exitosamente.";
//                    }
//                    else
//                    {
//                        respuesta.Mensaje = "No se pudo actualizar el EstatusProyectos.";
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al actualizar el EstatusProyectos: " + ex.Message;
//                respuesta.Errores.Add(ex.Message);
//            }
//            finally
//            {
//                _dapperContext.Dispose(); // Liberar recursos
//            }

//            return respuesta;
//        }

//        public async Task<RespuestaJson> DesactivarEstatusProyecto(EstatusProyectos estatusproyecto)
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {
//                // Obtener la fecha y hora actual
//                estatusproyecto.FechaDel = DateTime.Now;

//                using (var connection = _dapperContext.AbrirConexion("SGP"))
//                {
//                    // Consulta SQL para desactivar el estatusproyecto
//                    string sql = @"
//                    UPDATE EstatusProyectos 
//                    SET FlgActivo = 0, 
//                        IdUsuarioDel = @IdUsuarioDel, 
//                        FechaDel = @FechaDel 
//                    WHERE Id = @Id";

//                    var parametros = new DynamicParameters();
//                    parametros.Add("@Id", estatusproyecto.Id, DbType.Int32);
//                    parametros.Add("@IdUsuarioDel", estatusproyecto.IdUsuarioDel, DbType.Int32);
//                    parametros.Add("@FechaDel", estatusproyecto.FechaDel, DbType.DateTime);

//                    // Ejecutar la consulta
//                    int filasAfectadas = await connection.ExecuteAsync(sql, parametros);

//                    if (filasAfectadas > 0)
//                    {
//                        respuesta.Resultado = true;
//                        respuesta.Mensaje = "EstatusProyecto desactivado correctamente.";
//                    }
//                    else
//                    {
//                        respuesta.Mensaje = "No se encontró el estatusproyecto o ya está desactivado.";
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al desactivar el estatusproyecto: " + ex.Message;
//            }
//            finally
//            {
//                _dapperContext.Dispose();
//            }
//            return respuesta;
//        }

//        public async Task<RespuestaJson> ReactivarEstatusProyecto(EstatusProyectos estatusproyecto)
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {
//                // Obtener la fecha y hora actual
//                estatusproyecto.FechaUpd = DateTime.Now;

//                using (var connection = _dapperContext.AbrirConexion("SGP"))
//                {
//                    // Consulta SQL para reactivar el estatusproyecto
//                    string sql = @"
//                    UPDATE EstatusProyectos 
//                    SET FlgActivo = 1, 
//                        IdUsuarioUpd = @IdUsuarioUpd, 
//                        FechaUpd = @FechaUpd
//                    WHERE Id = @Id";

//                    var parametros = new DynamicParameters();
//                    parametros.Add("@Id", estatusproyecto.Id, DbType.Int32);
//                    parametros.Add("@IdUsuarioUpd", estatusproyecto.IdUsuarioUpd, DbType.Int32);
//                    parametros.Add("@FechaUpd", estatusproyecto.FechaUpd, DbType.DateTime);

//                    // Ejecutar la consulta
//                    int filasAfectadas = await connection.ExecuteAsync(sql, parametros);

//                    if (filasAfectadas > 0)
//                    {
//                        respuesta.Resultado = true;
//                        respuesta.Mensaje = "EstatusProyecto reactivado correctamente.";
//                    }
//                    else
//                    {
//                        respuesta.Mensaje = "No se encontró el estatusproyecto o ya está reactivado.";
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al reactivar el estatusproyecto: " + ex.Message;
//            }
//            finally
//            {
//                _dapperContext.Dispose();
//            }
//            return respuesta;
//        }

//        //============================================================================================================================\\
//        //====================================================    EstatusTareas   ==================================================== \\
//        //==============================================================================================================================\\

//        public async Task<RespuestaJson> GetEstatusTareas()
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {
//                // Abrir la conexión a la base de datos
//                _dapperContext.AbrirConexion("SGP");

//                string sql = @"SELECT 
//                    s.Id, 
//                    s.TareaAsignada, 
//                    c.Descripcion, 
//                    C.Prefijo,
//                    s.FlgActivo,
//                    s.IdSistema
//                FROM 
//                    EstatusTareas s 
//                INNER JOIN 
//                    Sistemas c ON s.IdSistema = c.Id
//                ORDER BY s.IdSistema"; // Ordenar por IdSistema

//                var estatustareas = await _dapperContext.QueryAsync<EstatusTareas>(sql);
//                if (estatustareas != null)
//                {
//                    respuesta.Resultado = true;
//                    respuesta.Data = estatustareas.Select(s => new EstatusTareas
//                    {
//                        Id = s.Id,
//                        TareaAsignada = s.TareaAsignada,
//                        Descripcion = s.Descripcion,
//                        Prefijo = s.Prefijo,
//                        FlgActivo = s.FlgActivo
//                    }).ToList();
//                }
//                else
//                {
//                    respuesta.Mensaje = "No se encontraron estatustareas activos.";
//                    respuesta.Data = new List<EstatusTareas>(); // Inicializar Data para evitar null
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al obtener los datos de los estatustareas." + ex.Message;
//                respuesta.Data = new List<EstatusTareas>(); // Inicializar Data para evitar null
//            }
//            finally
//            {
//                // Cerrar o liberar la conexión (si es necesario)
//                _dapperContext.Dispose();
//            }
//            return respuesta;
//        }

//        public async Task<RespuestaJson> AgregarEstatusTarea(EstatusTareas estatustarea)
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {

//                // Abrir conexión y validar duplicados
//                using (var connection = _dapperContext.AbrirConexion("SGP"))
//                {

//                    // Validar si la combinación de EstatusDescripcion y IdSistema ya existe en la base de datos
//                    string sqlValidarTareaAsignada = "SELECT COUNT(*) FROM EstatusTareas WHERE TareaAsignada = @TareaAsignada AND IdSistema = @IdSistema;";
//                    var parametrosValidarTareaAsignada = new DynamicParameters();
//                    parametrosValidarTareaAsignada.Add("@TareaAsignada", estatustarea.TareaAsignada, DbType.String);
//                    parametrosValidarTareaAsignada.Add("@IdSistema", estatustarea.IdSistema, DbType.Int32);

//                    int EstatusDescripcionExistente = await connection.ExecuteScalarAsync<int>(sqlValidarTareaAsignada, parametrosValidarTareaAsignada);

//                    if (EstatusDescripcionExistente > 0)
//                    {
//                        respuesta.Mensaje = "El sistema ya cuenta con esta tarea.";
//                        return respuesta;
//                    }

//                    // Consulta SQL para insertar el estatustarea
//                    string sqlInsertar = @"
//                    INSERT INTO EstatusTareas 
//                    (TareaAsignada, IdSistema, IdUsuarioSet) 
//                    VALUES 
//                    (@TareaAsignada, @IdSistema, @IdUsuarioSet);";

//                    var parametrosInsertar = new DynamicParameters();
//                    parametrosInsertar.Add("@TareaAsignada", estatustarea.TareaAsignada, DbType.String);
//                    parametrosInsertar.Add("@IdSistema", estatustarea.IdSistema, DbType.String);
//                    parametrosInsertar.Add("@IdUsuarioSet", estatustarea.IdUsuarioSet, DbType.Int32);

//                    // Ejecutar la consulta de inserción
//                    int filasAfectadas = await connection.ExecuteAsync(sqlInsertar, parametrosInsertar);

//                    if (filasAfectadas > 0)
//                    {
//                        respuesta.Resultado = true;
//                        respuesta.Mensaje = "EstatusTarea agregado exitosamente.";
//                        respuesta.Data = new { EstatusTareaId = estatustarea.Id }; // Puedes devolver el ID del estatustarea si lo necesitas
//                    }
//                    else
//                    {
//                        respuesta.Mensaje = "No se pudo agregar el estatustarea.";
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al agregar el estatustarea: " + ex.Message;
//                respuesta.Errores.Add(ex.Message);
//            }
//            finally
//            {
//                _dapperContext.Dispose(); // Liberar recursos
//            }

//            return respuesta;
//        }

//        public async Task<RespuestaJson> EditarEstatusTarea(EstatusTareas estatustarea)
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {
//                // Obtener la fecha y hora actual
//                estatustarea.FechaUpd = DateTime.Now;

//                // Abrir conexión y validar duplicados
//                using (var connection = _dapperContext.AbrirConexion("SGP"))
//                {
//                    // Consulta SQL base para actualizar el usuario
//                    string sqlActualizar = @"
//                    UPDATE EstatusTareas
//                    SET TareaAsignada = @TareaAsignada, 
//                        IdUsuarioUpd = @IdUsuarioUpd,
//                        FechaUpd = @FechaUpd";

//                    // Agregar IdPuesto a la consulta si tiene un valor válido (> 0)
//                    if (estatustarea.IdSistema > 0)
//                    {
//                        sqlActualizar += ", IdSistema = @IdSistema";
//                    }

//                    sqlActualizar += " WHERE Id = @Id;";

//                    var parametrosActualizar = new DynamicParameters();
//                    parametrosActualizar.Add("@Id", estatustarea.Id, DbType.Int32);
//                    parametrosActualizar.Add("@TareaAsignada", estatustarea.TareaAsignada, DbType.String);
//                    parametrosActualizar.Add("@IdUsuarioUpd", estatustarea.IdUsuarioUpd, DbType.Int32);
//                    parametrosActualizar.Add("@FechaUpd", estatustarea.FechaUpd, DbType.DateTime);

//                    // Agregar IdPuesto a los parámetros si tiene un valor válido (> 0)
//                    if (estatustarea.IdSistema > 0)
//                    {
//                        parametrosActualizar.Add("@IdSistema", estatustarea.IdSistema, DbType.Int32);
//                    }

//                    // Ejecutar la consulta de actualización
//                    int filasAfectadas = await connection.ExecuteAsync(sqlActualizar, parametrosActualizar);

//                    if (filasAfectadas > 0)
//                    {
//                        respuesta.Resultado = true;
//                        respuesta.Mensaje = "EstatusTarea actualizado exitosamente.";
//                    }
//                    else
//                    {
//                        respuesta.Mensaje = "No se pudo actualizar el EstatusTarea.";
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al actualizar el EstatusTarea: " + ex.Message;
//                respuesta.Errores.Add(ex.Message);
//            }
//            finally
//            {
//                _dapperContext.Dispose(); // Liberar recursos
//            }

//            return respuesta;
//        }

//        public async Task<RespuestaJson> DesactivarEstatusTarea(EstatusTareas estatustarea)
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {
//                // Obtener la fecha y hora actual
//                estatustarea.FechaDel = DateTime.Now;

//                using (var connection = _dapperContext.AbrirConexion("SGP"))
//                {
//                    // Consulta SQL para desactivar el estatustarea
//                    string sql = @"
//                    UPDATE EstatusTareas 
//                    SET FlgActivo = 0, 
//                        IdUsuarioDel = @IdUsuarioDel, 
//                        FechaDel = @FechaDel 
//                    WHERE Id = @Id";

//                    var parametros = new DynamicParameters();
//                    parametros.Add("@Id", estatustarea.Id, DbType.Int32);
//                    parametros.Add("@IdUsuarioDel", estatustarea.IdUsuarioDel, DbType.Int32);
//                    parametros.Add("@FechaDel", estatustarea.FechaDel, DbType.DateTime);

//                    // Ejecutar la consulta
//                    int filasAfectadas = await connection.ExecuteAsync(sql, parametros);

//                    if (filasAfectadas > 0)
//                    {
//                        respuesta.Resultado = true;
//                        respuesta.Mensaje = "EstatusTarea desactivado correctamente.";
//                    }
//                    else
//                    {
//                        respuesta.Mensaje = "No se encontró el estatustarea o ya está desactivado.";
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al desactivar el estatustarea: " + ex.Message;
//            }
//            finally
//            {
//                _dapperContext.Dispose();
//            }
//            return respuesta;
//        }

//        public async Task<RespuestaJson> ReactivarEstatusTarea(EstatusTareas estatustarea)
//        {
//            RespuestaJson respuesta = new RespuestaJson();
//            try
//            {
//                // Obtener la fecha y hora actual
//                estatustarea.FechaUpd = DateTime.Now;

//                using (var connection = _dapperContext.AbrirConexion("SGP"))
//                {
//                    // Consulta SQL para reactivar el estatustarea
//                    string sql = @"
//                    UPDATE EstatusTareas 
//                    SET FlgActivo = 1, 
//                        IdUsuarioUpd = @IdUsuarioUpd, 
//                        FechaUpd = @FechaUpd
//                    WHERE Id = @Id";

//                    var parametros = new DynamicParameters();
//                    parametros.Add("@Id", estatustarea.Id, DbType.Int32);
//                    parametros.Add("@IdUsuarioUpd", estatustarea.IdUsuarioUpd, DbType.Int32);
//                    parametros.Add("@FechaUpd", estatustarea.FechaUpd, DbType.DateTime);

//                    // Ejecutar la consulta
//                    int filasAfectadas = await connection.ExecuteAsync(sql, parametros);

//                    if (filasAfectadas > 0)
//                    {
//                        respuesta.Resultado = true;
//                        respuesta.Mensaje = "EstatusTarea reactivado correctamente.";
//                    }
//                    else
//                    {
//                        respuesta.Mensaje = "No se encontró el estatustarea o ya está reactivado.";
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                respuesta.Mensaje = "Ocurrió un error al reactivar el estatustarea: " + ex.Message;
//            }
//            finally
//            {
//                _dapperContext.Dispose();
//            }
//            return respuesta;
//        }
//    }
//}
