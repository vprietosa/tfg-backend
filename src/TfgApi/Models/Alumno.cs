using System.ComponentModel.DataAnnotations;

namespace TfgApi.Models;

public class Alumno
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = null!;

    [Required]
    [MaxLength(150)]
    public string Apellidos { get; set; } = null!;

    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = null!;

    [Range(12, 120)]
    public int Edad { get; set; }

    [Required]
    [MaxLength(100)]
    public string Ciudad { get; set; } = null!;

    [Required]
    [MaxLength(150)]
    public string InstitucionEducativa { get; set; } = null!;

    [Required]
    [MaxLength(500)]
    public string PasswordHash { get; set; } = null!;

    public DateTime FechaRegistro { get; set; }
   
    [MaxLength(500)]
    public string? FotoPerfilUrl { get; set; }
}
