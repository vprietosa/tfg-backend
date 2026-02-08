using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TfgApi.Models;

public class Practica
{
    [Key]
    public int Id { get; set; }

    public int? EmpresaId { get; set; }

    public int? AutonomoId { get; set; }

    [Required]
    [MaxLength(1000)]
    [Column("DescripcionPracticas")]
    public string DescripcionPractica { get; set; } = null!;

    [Range(1, 365)]
    [Column("DiasPracticas")]
    public int NumeroDias { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("HorarioPracticas")]
    public string Horario { get; set; } = null!;

    public bool Activa { get; set; }

    public DateTime FechaCreacion { get; set; }
}
