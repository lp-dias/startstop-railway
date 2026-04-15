using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StartStop.Models
{
    public class Veiculo
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "A placa é obrigatória")]
        [StringLength(10, ErrorMessage = "Placa inválida")]
        public string Placa { get; set; } = string.Empty;

        [Required]
        [RegularExpression("Disponível|Indisponível|Bloqueado|Reservado", ErrorMessage = "Status inválido")]
        public string Status { get; set; } = "Disponível";

        // Navegação
        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

        [Range(0, int.MaxValue, ErrorMessage = "Km acumulado não pode ser negativo")]
        public int KmAcumulado { get; set; } = 0;

        // ✅ Controle de bloqueio
        public bool Bloqueado { get; set; } = false;   // mapeado para TINYINT(1) no MySQL

        [Column(TypeName = "datetime")]
        public DateTime? DataInicio { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? DataFim { get; set; }

        [StringLength(100)]
        public string? Oficina { get; set; }

        // 🔑 Propriedade de navegação
        public ICollection<Movimentacao> Movimentacoes { get; set; } = new List<Movimentacao>();
    }
}
