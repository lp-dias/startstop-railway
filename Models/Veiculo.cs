using System;
using System.ComponentModel.DataAnnotations;

namespace StartStop.Models
{
    public class Veiculo
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "A placa é obrigatória")]
        [StringLength(10, ErrorMessage = "Placa inválida")]
        public string Placa { get; set; }

        [Required]
        [RegularExpression("Disponível|Indisponível|Bloqueado|Reservado", ErrorMessage = "Status inválido")]
        public string Status { get; set; } = "Disponível";


         // Navegação
         public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();


        [Range(0, int.MaxValue, ErrorMessage = "Km acumulado não pode ser negativo")]
        public int KmAcumulado { get; set; }

        // ✅ Novos campos para controle de bloqueio
        public bool Bloqueado { get; set; }   // true = bloqueado, false = disponível

        public DateTime? DataInicio { get; set; } // quando começou o bloqueio
        public DateTime? DataFim { get; set; }    // quando terminou o bloqueio
        public string? Oficina { get; set; }       // motivo/descrição (ex.: "Oficina")

        // 🔑 Propriedade de navegação
        public ICollection<Movimentacao> Movimentacoes { get; set; } = new List<Movimentacao>();
    }
}
