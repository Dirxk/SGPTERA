using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionProyectos.Models
{
    public class Puestos
    {
        [Key]
        public int Id { get; set; }
        public required string Descripcion { get; set; }
        public int Tipo { get; set; }
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
    }

    public class Clientes
    {
        [Key]
        public int Id { get; set; }
        public required string RazonSocial { get; set; }
        public required string RFC { get; set; }
        public required string Prefijo { get; set; }
        public required string Telefono { get; set; }
        public byte[] Logotipo { get; set; }
        public int? IdUsuarioSet { get; set; }
        public DateTime FechaSet { get; set; } = DateTime.UtcNow;
        public int? IdUsuarioUpd { get; set; }
        public DateTime? FechaUpd { get; set; }
        public int? IdUsuarioDel { get; set; }
        public DateTime? FechaDel { get; set; }
        public bool FlgActivo { get; set; } = true;
    }

    public class ClienteUsuarios
    {
        [Key]
        public int Id { get; set; }
        public required string ClienteUsuario { get; set; }
        public required string NombreClienteUsuario { get; set; }
        public required string ApellidoPaternoClienteUsuario { get; set; }
        public required string ApellidoMaternoClienteUsuario { get; set; }
        public int? IdPuesto { get; set; }
        public required string Telefono { get; set; }
        public required string Correo { get; set; }
        [Required]
        public int IdCliente { get; set; }

        public byte[] FotoPerfil { get; set; }
        public int? IdUsuarioSet { get; set; }
        public DateTime FechaSet { get; set; } = DateTime.UtcNow;
        public int? IdUsuarioUpd { get; set; }
        public DateTime? FechaUpd { get; set; }
        public int? IdUsuarioDel { get; set; }
        public DateTime? FechaDel { get; set; }
        public bool FlgActivo { get; set; } = true;
    }

    public class Sistemas
    {
        [Key]
        public int Id { get; set; }
        public required string Descripcion { get; set; }
        [Required]
        public int IdCliente { get; set; }
        [MaxLength(255)]
        public string Repositorio { get; set; }
        [MaxLength(20)]
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

