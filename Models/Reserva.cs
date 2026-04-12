using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using StartStop.Models;


public class Reserva
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int VeiculoId { get; set; }

    [ForeignKey("VeiculoId")]
    public Veiculo Veiculo { get; set; }

    [Required]
    public int UsuarioId { get; set; }

    [ForeignKey("UsuarioId")]
    public Usuario Usuario { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime DataInicio { get; set; }   // Dia e hora de início

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime DataFim { get; set; }      // Dia e hora de término

    [Required]
    [StringLength(20)]
    public string Status { get; set; } // Ativa, Finalizada, Cancelada

 
    
}
