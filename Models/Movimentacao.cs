using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StartStop.Models
{
    public class Movimentacao
    {
        public int Id { get; set; }

        [Required]
        public int MotoristaId { get; set; }
        public Motorista Motorista { get; set; } = null!;

        [Required]
        public int VeiculoId { get; set; }
        public Veiculo Veiculo { get; set; } = null!;

        [Required]
        [Column(TypeName = "datetime")]
        public DateTime DataRetirada { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? DataRetorno { get; set; }

        [Required(ErrorMessage = "O destino é obrigatório.")]
        [StringLength(150)]
        public string Destino { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int KmSaida { get; set; }

        public int? KmRetorno { get; set; }

        public string? MotivoCancelamento { get; set; }

        [Required]
        [StringLength(50)]
        public string TipoMovimentacao { get; set; } = "Retirada";

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pendente";

        public int? KmPercorrido { get; set; }
        public int? TempoPosse { get; set; }
    }
}
