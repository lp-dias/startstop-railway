using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StartStop.Models
{
    public class Manutencao
    {
        public int Id { get; set; }

        [Required]
        public int VeiculoId { get; set; }
        public Veiculo Veiculo { get; set; } = null!;

        // ✅ Melhor usar bool para MySQL (mapeado para TINYINT(1))
        public bool Bloqueado { get; set; } = false;

        [Column(TypeName = "datetime")]
        public DateTime? DataInicio { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? DataFim { get; set; }

        [Required]
        [StringLength(100)]
        public string Tipo { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? Custo { get; set; }

        [StringLength(500)]
        public string Observacoes { get; set; } = string.Empty;
    }
}
