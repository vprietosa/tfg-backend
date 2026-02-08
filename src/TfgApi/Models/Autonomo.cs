using System.ComponentModel.DataAnnotations;

namespace TfgApi.Models;

public class Autonomo
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Nombre { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string Oficio { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string Ciudad { get; set; } = null!;

    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = null!;

    [Required]
    [MaxLength(500)]
    public string PasswordHash { get; set; } = null!;

    public DateTime FechaRegistro { get; set; }
}
