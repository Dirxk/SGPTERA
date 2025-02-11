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
        public int Tipo { get; set; }
        public int? IdUsuarioSet { get; set; }
        public DateTime FechaSet { get; set; } = DateTime.Now;
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
        public string Descripcion { get; set; }
        public string NombreUsuario { get; set; }
        public int? IdPuesto { get; set; }
        public string Telefono { get; set; }
        public string Correo { get; set; }
        public byte[] FotoPerfil { get; set; }
        public int? IdUsuarioSet { get; set; }
        public DateTime FechaSet { get; set; } = DateTime.Now;
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
        public string Descripcion { get; set; }
        public string Prefijo { get; set; }
        public string Telefono { get; set; }
        public byte[] Logotipo { get; set; }
        public int? IdUsuarioSet { get; set; }
        public DateTime FechaSet { get; set; } = DateTime.Now;
        public int? IdUsuarioUpd { get; set; }
        public DateTime? FechaUpd { get; set; }
        public int? IdUsuarioDel { get; set; }
        public DateTime? FechaDel { get; set; }
        public bool FlgActivo { get; set; } = true;
    }

    public class ClientesUsuarios
    {
        [Key]
        public int Id { get; set; }
        public string Descripcion { get; set; }
        public int? IdPuesto { get; set; }
        public string Telefono { get; set; }
        public string Correo { get; set; }
        public int IdCliente { get; set; }
        public byte[] FotoPerfil { get; set; }
        public int? IdUsuarioSet { get; set; }
        public DateTime FechaSet { get; set; } = DateTime.Now;
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
        public string Descripcion { get; set; }
        public int IdCliente { get; set; }
        public string Repositorio { get; set; }
        public string Prefijo { get; set; }
        public int? IdUsuarioSet { get; set; }
        public DateTime FechaSet { get; set; } = DateTime.Now;
        public int? IdUsuarioUpd { get; set; }
        public DateTime? FechaUpd { get; set; }
        public int? IdUsuarioDel { get; set; }
        public DateTime? FechaDel { get; set; }
        public bool FlgActivo { get; set; } = true;
    }

    public class ModulosSistemas
    {
        [Key]
        public int Id { get; set; }
        public string Descripcion { get; set; }
        public int? IdUsuarioSet { get; set; }
        public DateTime FechaSet { get; set; } = DateTime.Now;
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
        public string Descripcion { get; set; }
        public int? IdUsuarioSet { get; set; }
        public DateTime FechaSet { get; set; } = DateTime.Now;
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
        public string Descripcion { get; set; }
        public int? IdUsuarioSet { get; set; }
        public DateTime FechaSet { get; set; } = DateTime.Now;
        public int? IdUsuarioUpd { get; set; }
        public DateTime? FechaUpd { get; set; }
        public int? IdUsuarioDel { get; set; }
        public DateTime? FechaDel { get; set; }
        public bool FlgActivo { get; set; } = true;
    }
}

