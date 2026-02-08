using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TfgApi.Models;

public class Empresa
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string NombreEmpresa { get; set; } = null!;

    [Required]
    [MaxLength(250)]
    [Column("DireccionEmpresa")]
    public string Direccion { get; set; } = null!;

    [MaxLength(1000)]
    public string? DescripcionEmpresa { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = null!;

    [Required]
    [MaxLength(500)]
    public string PasswordHash { get; set; } = null!;

    public DateTime FechaRegistro { get; set; }
}
