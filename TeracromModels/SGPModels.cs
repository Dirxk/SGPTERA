using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionProyectos.Models
{
    public class Puestos
    {
        [Key]
        public int Id { get; set; }
        public string Descripcion { get; set; }
        public string  Tipo { get; set; }
        public int? IdUsuarioSet { get; set; }
        public DateTime FechaSet { get; set; } = DateTime.UtcNow;
        public int? IdUsuarioUpd { get; set; }
        public DateTime? FechaUpd { get; set; }
        public int? IdUsuarioDel { get; set; }
        public DateTime? FechaDel { get; set; }
        public bool FlgActivo { get; set; } = true;
    }

    public class Usuarios
    {
        [Key]
        public int Id { get; set; }
        public  string Usuario { get; set; }
        public  string NombreUsuario { get; set; }
        public  string ApellidoPaternoUsuario { get; set; }
        public  string ApellidoMaternoUsuario { get; set; }
        public string Descripcion { get; set; }
        public int? IdPuesto { get; set; }
        public string Telefono { get; set; }
        public  string Correo { get; set; }
        public  string Contrasena { get; set; }
        public byte[] FotoPerfil { get; set; }
        public int? IdUsuarioSet { get; set; }
        public DateTime FechaSet { get; set; } = DateTime.UtcNow;
        public int? IdUsuarioUpd { get; set; }
        public DateTime? FechaUpd { get; set; }
        public int? IdUsuarioDel { get; set; }
        public DateTime? FechaDel { get; set; }
        public bool FlgActivo { get; set; } = true;

        [NotMapped]
        public IFormFile FotoFile { get; set; } // Para manejar la imagen como archivo

        [NotMapped]
        public string FotoBase64 { get; set; } // Para manejar la imagen existente en base64
    }

    public class Clientes
    {
        [Key]
        public int Id { get; set; }

        public string RazonSocial { get; set; }

        public string RFC { get; set; }

        public string Prefijo { get; set; }
        public string Telefono { get; set; }

        public byte[] Logotipo { get; set; }

        public int IdUsuarioSet { get; set; }

        public DateTime FechaSet { get; set; } = DateTime.UtcNow;

        public int? IdUsuarioUpd { get; set; }

        public DateTime? FechaUpd { get; set; }

        public int? IdUsuarioDel { get; set; }

        public DateTime? FechaDel { get; set; }

        public bool FlgActivo { get; set; } = true;

        [NotMapped]
        public IFormFile LogotipoFile { get; set; } // Para manejar la imagen como archivo

        [NotMapped]
        public string LogotipoBase64 { get; set; } // Para manejar la imagen existente en base64
    }

    public class ClientesUsuarios
    {
        [Key]
        public int Id { get; set; }
        public string ClienteUsuario { get; set; }
        public string NombreClienteUsuario { get; set; }
        public string ApellidoPaternoClienteUsuario { get; set; }
        public string ApellidoMaternoClienteUsuario { get; set; }
        public string Descripcion { get; set; }
        public int? IdPuesto { get; set; }
        public string Telefono { get; set; }
        public string Correo { get; set; }
        public string Contrasena { get; set; }
        public string RazonSocial { get; set; }
        public int IdCliente { get; set; }
        public byte[] FotoPerfil { get; set; }
        public int? IdUsuarioSet { get; set; }
        public DateTime FechaSet { get; set; } = DateTime.UtcNow;
        public int? IdUsuarioUpd { get; set; }
        public DateTime? FechaUpd { get; set; }
        public int? IdUsuarioDel { get; set; }
        public DateTime? FechaDel { get; set; }
        public bool FlgActivo { get; set; } = true;
        [NotMapped]
        public IFormFile FotoFile { get; set; } // Para manejar la imagen como archivo

        [NotMapped]
        public string FotoBase64 { get; set; } // Para manejar la imagen existente en base64
    }

    public class Sistemas
    {
        [Key]
        public int Id { get; set; }
        public required string Descripcion { get; set; }
        public string RazonSocial { get; set; }
        public int IdCliente { get; set; }
        public string Repositorio { get; set; }
        public string Prefijo { get; set; }
        public int? IdUsuarioSet { get; set; }
        public DateTime FechaSet { get; set; } = DateTime.UtcNow;
        public int? IdUsuarioUpd { get; set; }
        public DateTime? FechaUpd { get; set; }
        public int? IdUsuarioDel { get; set; }
        public DateTime? FechaDel { get; set; }
        public bool FlgActivo { get; set; } = true;
    }

    public class ModuloSistema
    {
        [Key]
        public int Id { get; set; }
        public required string Descripcion { get; set; }
        public int? IdUsuarioSet { get; set; }
        public DateTime FechaSet { get; set; } = DateTime.UtcNow;
        public int? IdUsuarioUpd { get; set; }
        public DateTime? FechaUpd { get; set; }
        public int? IdUsuarioDel { get; set; }
        public DateTime? FechaDel { get; set; }
        public bool FlgActivo { get; set; } = true;
    }

    public class EstatusProyectos
    {
        [Key]
        public int Id { get; set; }
        public required string Descripcion { get; set; }
        public int? IdUsuarioSet { get; set; }
        public DateTime FechaSet { get; set; } = DateTime.UtcNow;
        public int? IdUsuarioUpd { get; set; }
        public DateTime? FechaUpd { get; set; }
        public int? IdUsuarioDel { get; set; }
        public DateTime? FechaDel { get; set; }
        public bool FlgActivo { get; set; } = true;
    }

    public class EstatusTareas
    {
        [Key]
        public int Id { get; set; }
        public required string Descripcion { get; set; }
        public int? IdUsuarioSet { get; set; }
        public DateTime FechaSet { get; set; } = DateTime.UtcNow;

        public int? IdUsuarioUpd { get; set; }
        public DateTime? FechaUpd { get; set; }
        public int? IdUsuarioDel { get; set; }
        public DateTime? FechaDel { get; set; }
        public bool FlgActivo { get; set; } = true;
    }
    
}

