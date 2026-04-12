using System;
using System.ComponentModel.DataAnnotations;

namespace StartStop.Models
{
    public class Manutencao
    {
        public int Id { get; set; }

        [Required]
        public int VeiculoId { get; set; }
        public Veiculo Veiculo { get; set; }

        public int Bloqueado { get; set; } // 0 = Não, 1 = Sim


        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }

        public string Tipo { get; set; }

        [Range(0, double.MaxValue)]
        public double? Custo { get; set; }

        [StringLength(500)]
        public string Observacoes { get; set; }
    }
}
