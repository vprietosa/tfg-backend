using System.ComponentModel.DataAnnotations;

namespace TfgApi.Models;

public class PracticaRealizada
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int AlumnoId { get; set; }

    [Required]
    public int PracticaId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Estado { get; set; } = null!;

    public DateTime? FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    public DateTime FechaCreacion { get; set; }
}
